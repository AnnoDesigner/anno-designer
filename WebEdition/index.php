<?
error_reporting(E_ALL);
?>
<!DOCTYPE html>
<html>

<head>
    <meta charset="utf-8">
    <title>Anno Designer - Web Edition</title>
    <link href="styles/jquery.miniColors.css" rel="stylesheet" type="text/css">
    <link href="styles/pepper-grinder/jquery-ui-1.9.2.custom.css" rel="stylesheet" type="text/css">
    <link href="styles/jquery.dataTables_themeroller.css" rel="stylesheet" type="text/css">
    <link href="styles/styles.css" rel="stylesheet" type="text/css">
    <script src="lib/jquery-1.8.3.min.js"></script>
    <script src="lib/jquery-ui-1.9.2.min.js"></script>
    <script src="lib/jquery.dataTables.min.js"></script>
    <script src="lib/jquery.dataTables.plugins.js"></script>
    <script src="lib/jquery.themeswitcher.min.js"></script>
    <script src="lib/jquery.miniColors.js"></script>
    <script src="lib/json2.js"></script>
    <script src="scripts/geometry.js"></script>
    <script src="scripts/helpers.js"></script>
    <script src="scripts/building.js"></script>
    <script src="scripts/designer.js"></script>
    <script type="text/javascript" language="JavaScript">
        var designer, datatable;
        $(function () {
            // initialize theme switcher
            // https://github.com/harborhoffer/Super-Theme-Switcher
            $("#themeSwitcher").themeswitcher({
                loadtheme: "pepper-grinder",
                imgpath: "styles/themes/",
                width: 250,
                height: 500,
                closeOnSelect: false
            });
            // initialize menu buttons
            $("#login").button({ icons: { primary: "ui-icon-key" } })
                    .click(function(e) {
                        $("#loginForm").toggle();
                        $("#registerForm").hide();
                    });
            $("#register").button({ icons: { primary: "ui-icon-person" } })
                    .click(function(e) {
                        $("#loginForm").hide();
                        $("#registerForm").toggle();
                    });
            $("#logout").button({ icons: { primary: "ui-icon-power" }, disabled: true })
                    .click(function(e) {
                        //TODO: logout
                    });
            $("#feedback").button({ icons: { primary: "ui-icon-script" }, disabled: true });
            $("#toggleEditor").button({ icons: { primary: "ui-icon-arrow-4-diag" } })
                    .click(function(e) {
                        $("#preview").toggleClass("preview");
                        $("#list").toggle();
                    });
            // initialize datatable
            datatable = $("#listTable").dataTable({
                bJQueryUI:true,
                bProcessing:true,
                sAjaxSource:"rest/layout",
                aoColumns:[
                    { mData:"name" },
                    { mData:"author" },
                    { mData:"width" },
                    { mData:"height" },
                    { mData:"created" },
                    { mData:"edited" }
                ],
                aaSorting: [[4, 'desc', 0]],
                fnInitComplete:function (oSettings, json) {
                    //TODO: select first row in a nicer way
                    if (json.aaData.length > 0) {
                        $("#" + json.aaData[0].ID).click();
                    }
                }
            });
            // handler to select and load layouts based on the table row
            function selectRowNode(node) {
                // select row
                if (!node.hasClass("row_selected")) {
                    datatable.$("tr.row_selected").removeClass("row_selected");
                    node.addClass("row_selected");
                }
                // load layout
                designer.Load(node[0].id);
            }
            // add a click handler to the rows
            $("#listTable tbody tr").live("click", function (event) {
                selectRowNode($(this));
            });
            // initialize layout rendering
            designer = new Designer({
                layoutReset: function () {
                    // unselect row
                    datatable.$("tr.row_selected").removeClass("row_selected");
                },
                layoutChanged: function (id) {
                    // reload data
                    datatable.fnReloadAjax(null, function(){
                        // re-select changed row, after reload completed
                        datatable.$("#" + id).addClass("row_selected");
                    });
                },
                layoutDeleted: function (id) {
                    // retrieve row to be deleted
                    var row = datatable.$("#" + id);
                    // check if row is currently selected
                    if (row.hasClass("row_selected")) {
                        // try to get next or previous row
                        var tr = datatable.fnGetAdjacentTr(row[0], true);
                        if (tr == null) {
                            tr = datatable.fnGetAdjacentTr(row[0], false);
                        }
                        // select row if found
                        if (tr != null) {
                            selectRowNode($(tr));
                        }
                    }
                    // remove deleted row
                    datatable.fnDeleteRow(row[0]);
                }
            });
            // prepare login and register forms
            $(".formContainer button.submitButton")
                    .button({ icons: { primary: "ui-icon-check" } });
            $(".formContainer button.cancelButton")
                    .button({ icons: { primary: "ui-icon-close" }, text: false })
                    .removeAttr("title")
                    .click(function() {
                        $("#loginForm").hide();
                        $("#registerForm").hide();
                    });
            $("#loginForm form").submit(function() {
                //TODO: login
                // prevent postback
                return false;
            });
            $("#registerForm form").submit(function() {
                //TODO: validation
                Rest("POST", "rest/user", {
                        name: $("#registerName").val(),
                        username: $("#registerUsername").val(),
                        password: $("#registerPassword").val()
                    }, function (data) {
                        //TODO: add nicer information
                        alert(data.success ? "successful": "FAIL");
                        $("#registerForm").hide();
                    });
                // prevent postback
                return false;
            });
            // initialize default tooltips
            $(".formContainer").tooltip({
                position: { my: "left+15 center", at: "right center", collision: "flipfit" }
            });
        });
    </script>
</head>

<body>

<div style="float: right">
    <div id="themeSwitcher"></div>
</div>

<div id="header">
    <h1>Anno Designer - Web Edition</h1>
    <button id="login">Login</button>
    <button id="register">Register</button>
    <button id="logout">Logout</button>
    <button id="feedback">Feedback</button>
    <button id="toggleEditor">Toggle editor</button>
</div>

<div id="loginForm" class="formContainer">
    <form>
        <table class="ui-widget">
            <thead class="ui-widget-header">
                <tr>
                    <th colspan="2">
                        Registration
                        <button class="cancelButton">Hide</button>
                    </th>
                </tr>
            </thead>
            <tbody class="ui-widget-content">
                <tr>
                    <td><label for="loginUsername">Username</label></td>
                    <td><input id="loginUsername" type="text" /></td>
                </tr>
                <tr>
                    <td><label for="loginPassword">Password</label></td>
                    <td><input id="loginPassword" type="password" /></td>
                </tr>
                <tr>
                    <td colspan="2" class="submit">
                        <button class="submitButton">Login</button>
                    </td>
                </tr>
            </tbody>
        </table>
    </form>
</div>

<div id="registerForm" class="formContainer">
    <form>
        <table class="ui-widget">
            <thead class="ui-widget-header">
                <tr>
                    <th colspan="2">
                        Registration
                        <button class="cancelButton">Hide</button>
                    </th>
                </tr>
            </thead>
            <tbody class="ui-widget-content">
                <tr>
                    <td><label for="registerName">Name</label></td>
                    <td><input id="registerName" type="text" title="The name which is publicly shown." /></td>
                </tr>
                <tr>
                    <td><label for="registerUsername">Username</label></td>
                    <td><input id="registerUsername" type="text" title="Used for login, not shown anywhere." /></td>
                </tr>
                <tr>
                    <td><label for="registerPassword">Password</label></td>
                    <td><input id="registerPassword" type="password" title="Must be at least 4 characters long and cannot be the same as the name or username." /></td>
                </tr>
                <tr>
                    <td colspan="2" class="submit">
                        <button class="submitButton">Register</button>
                    </td>
                </tr>
            </tbody>
        </table>
    </form>
</div>

<div id="preview" class="preview">
    <h2>Layout preview</h2>
    <div id="editor"></div>
</div>

<div id="list">
    <h2>Layout list</h2>
    <table id="listTable">
        <thead>
        <tr>
            <th>Name</th>
            <th>Author</th>
            <th>Width</th>
            <th>Height</th>
            <th>Created</th>
            <th>Edited</th>
        </tr>
        </thead>
    </table>
</div>


</body>

</html>