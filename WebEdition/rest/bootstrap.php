<?php
// service initialization
require "Slim/Slim.php";
\Slim\Slim::registerAutoloader();

require "../config.php";
require "../helpers.php";

$db = @new mysqli($db_address, $db_username, $db_password, $db_database);
if ($db->connect_errno != 0) {
    die("Connection to database failed: ".$db->connect_error);
}

// assign routes
$app = new \Slim\Slim();
$app->contentType("application/json");
$app->hook("slim.after", function() { global $db; $db->close(); });
$app->get("/user/:id", "GetUser");
$app->post("/user", "CreateUser");
$app->get("/layout", "GetAllLayouts");
$app->post("/layout", "SaveLayout");
$app->get("/layout/:id", "GetLayout");
$app->delete("/layout/:id", "DeleteLayout");
$app->run();

// helpers
class IksdehException extends Exception
{
    function __construct($code = 400, $message = "")
    {
        parent::__construct($message, $code);
    }
}

function IdQuery($query, $id) {
    global $app, $db;
    $cleanID = get_int($id);
    // bad request
    if ($cleanID == null) {
        $app->halt(400);
    }
    // execute query
    $result = $db->query(str_replace("_ID_", $cleanID, $query));
    return $result;
}

function UniqueIdQuery($query, $id) {
    global $app;
    $result = IdQuery($query, $id);
    // not found
    if ($result->num_rows == 0) {
        $app->halt(404);
    }
    return $result;
}

function TryExecuteQueries($queries) {
    global $db;
    foreach ($queries as $query) {
        if (!$db->query($query)) {
            return false;
        }
    }
    return true;
}

function GetInsertNames($names) {
    $result = array();
    foreach ($names as $name) {
        $result[] = "`".$name."`";
    }
    return $result;
}

function GetInsertValues($names, $values) {
    $result = array();
    foreach ($names as $name) {
        $result[] = "'".mysql_real_escape_string($values[$name])."'";
    }
    return $result;
}

function RetriveUser($id) {
    return UniqueIdQuery("select name from user where ID=_ID_", $id)->fetch_assoc();
}

// request handlers
function GetUser($id) {
    $user = RetriveUser($id);
    //TODO: add more information
    echo json_encode(array("name" => $user["name"]));
}

function CreateUser() {
    global $app, $db;
    //TODO: validate data
    $name = mysql_real_escape_string($_POST["name"]);
    $username = mysql_real_escape_string($_POST["username"]);
    $password = mysql_real_escape_string($_POST["password"]);
    if (!$db->query("insert into user (name,username,password) values('$name','$username','$password')")) {
        $app->halt(400);
    }
    echo json_encode(array("success" => true));
}

function GetAllLayouts() {
    // get all layouts
    global $db;
    // sorting is ignored because the datatables it on its own, but maybe this way its a bit faster,
    // because the data is already sorted correctly
    $result = $db->query("select * from layout order by created desc");
    $layouts = array();
    while($row = $result->fetch_assoc()) {
        $row["DT_RowId"] = $row["ID"];
        $row["author"] = RetriveUser($row["authorID"])["name"];
        $layouts[] = $row;
    }
    // return results for datatable
    $response["aaData"] = $layouts;
    echo json_encode($response);
}

function GetLayout($id) {
    $layout = UniqueIdQuery("select * from layout where ID=_ID_", $id)
        ->fetch_assoc();
    $layout["objects"] = IdQuery("select * from layout_object where layoutID=_ID_", $layout["ID"])
        ->fetch_all(MYSQLI_ASSOC);
    $layout["author"] = RetriveUser($layout["authorID"])["name"];
    echo json_encode($layout);
}

function DeleteLayout($id) {
    global $app;
    //$layout = UniqueIdQuery("select * from layout where ID=_ID_", $id)->fetch_assoc();
    //TODO: check permission
    if (!IdQuery("delete from layout where ID=_ID_", $id)) {
        $app->halt(500);
    }
    echo json_encode(array("success" => true));
}

function SaveLayout() {
    global $app, $db;
    $data = json_decode($app->request()->params("data"), true);
    $name = mysql_real_escape_string($data["name"]);
    $objects = $data["objects"];
    $left = $right = $top = $bottom = null;
    $usedFields = 0;
    foreach ($objects as $o) {
        //TODO: implement "normalize"
        //TODO: validate all properties
        // determine bounding box
        if ($left == null || $o["left"] < $left) {
            $left = $o["left"];
        }
        if ($top == null || $o["top"] < $top) {
            $top = $o["top"];
        }
        if ($right == null || $right < $o["left"] + $o["width"]) {
            $right = $o["left"] + $o["width"];
        }
        if ($bottom == null || $bottom < $o["top"] + $o["height"]) {
            $bottom = $o["top"] + $o["height"];
        }
        $usedFields += $o["width"] * $o["height"];
    }
    // calculate bounding box dimensions
    $width = $right - $left;
    $height = $bottom - $top;
    if (!$db->query("START TRANSACTION")) {
        throw new IksdehException(500, "start transaction failed");
    }
    try {
        //TODO: add current user as author
        $query = "insert into layout (name, authorID, created, width, height, usedFields) values ('$name', 1, NOW(), $width, $height, $usedFields)";
        if (!$db->query($query)) {
            throw new IksdehException(400, "adding layout failed: ".$query);
        }
        // retrieve ID of the new layout entry
        $layoutID = $db->insert_id;
        // add objects
        $names = array("top", "left", "width", "height", "color", "label", "enableLabel", "borderless", "road");
        foreach ($objects as $o) {
            $query = "insert into layout_object"
                ."(`layoutID`,".implode(",", GetInsertNames($names)).")"
                ." values ('$layoutID', ".implode(",", GetInsertValues($names, $o)).")";
            if (!$db->query($query)) {
                throw new IksdehException(400, "adding layout_object failed: ".$query);
            }
        }
        // every query was successful, commit changes
        if (!$db->query("COMMIT")) {
            throw new IksdehException(500, "commit transaction failed");
        }
        // return result
        $layout = UniqueIdQuery("select * from layout where ID=_ID_", $layoutID)->fetch_assoc();
        $layout["author"] = RetriveUser($layout["authorID"])["name"];
        echo json_encode(array("success" => true, "ID" => $layoutID, "layout" => $layout));
    } catch(IksdehException $ex) {
        $db->query("ROLLBACK");
        $app->halt($ex->getCode(), $ex->getMessage());
    } catch(Exception $ex) {
        $db->query("ROLLBACK");
        throw $ex;
    }
}

/*
    ID int AUTO_INCREMENT PRIMARY KEY,
    layoutID int NOT NULL,
TODO    buildingID int,
    top int NOT NULL,
    `left` int NOT NULL,
    width int NOT NULL,
    height int NOT NULL,
    color char(7),
    label varchar(50),
TODO    icon varchar(50),
    enableLabel bool NOT NULL,
    borderless bool NOT NULL,
    road bool NOT NULL
 */


