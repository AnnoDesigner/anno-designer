<?php

//TODO: add foreign keys with correct constraints (cascade n stuff..)

// describes a user
$db_defs["user"] = "
create table user (
    ID int AUTO_INCREMENT PRIMARY KEY,
    name varchar(50) character set UTF8,
    username varchar(50) character set UTF8 NOT NULL,
    password varchar(40) character set UTF8 NOT NULL
) ENGINE = InnoDB";

// describes a material, which can then be used to describe building costs or as a result of a production chain
$db_defs["materials"] = "
create table materials (
    ID int AUTO_INCREMENT PRIMARY KEY,
    name varchar(50),
    icon varchar(50)
) ENGINE = InnoDB";

// describes a user created layout
$db_defs["layout"] = "
create table layout (
    ID int AUTO_INCREMENT PRIMARY KEY,
    name varchar(50) character set UTF8 NOT NULL,
    authorID int NOT NULL,
    created datetime NOT NULL,
    edited datetime,
	width int NOT NULL,
	height int NOT NULL,
	usedFields int NOT NULL
) ENGINE = InnoDB";

// links produced materials to a layout
$db_defs["layout_production"] = "
create table layout_production (
    ID int AUTO_INCREMENT PRIMARY KEY,
    layoutID int,
    materialID int
) ENGINE = InnoDB";

// links the contained building objects to a layout
$db_defs["layout_object"] = "
create table layout_object (
    ID int AUTO_INCREMENT PRIMARY KEY,
    layoutID int NOT NULL,
    buildingID int,
    top int NOT NULL,
    `left` int NOT NULL,
    width int NOT NULL,
    height int NOT NULL,
    color char(7),
    label varchar(50),
    icon varchar(50),
    enableLabel bool NOT NULL,
    borderless bool NOT NULL,
    road bool NOT NULL
) ENGINE = InnoDB";

// links an up- or down-vote by a user to a layout
$db_defs["rating"] = "
create table rating (
    ID int AUTO_INCREMENT PRIMARY KEY,
    userID int,
    layoutID int,
    upvote bit,
    date timestamp
) ENGINE = InnoDB";

// described a building
$db_defs["building"] = "
create table building (
    ID int AUTO_INCREMENT PRIMARY KEY,
    name varchar(50),
    width int,
    height int,
    icon varchar(50),
    faction varchar(50),
    `group` varchar(50),
    template varchar(50)
) ENGINE = InnoDB";

// links a material cost to a building
$db_defs["costs"] = "
create table costs (
    ID int AUTO_INCREMENT PRIMARY KEY,
    buildingID int,
    materialID int,
    amount int
) ENGINE = InnoDB";

?>