# FandomParser

This tool is used to create a preset file which contains all information from the wiki e.g. costs to build or maintain a building.

## Remarks

Currently only the [wiki of Anno 1800](https://anno1800.fandom.com/) is supported. Feel free to extend the functionality :smile:

Wiki pages containg the template [Infobox Buildings Old and New World](https://anno1800.fandom.com/wiki/Template:Infobox_Buildings_Old_and_New_World) will not be parsed.

## How it works

1. The tool downloads the WikiText of the [buildings overview page](https://anno1800.fandom.com/wiki/Buildings).
   This page contains a table with basic building information for each tier.
2. Those tables are parsed by the tool to get the basic informations of each building.
   Also a (maybe present) link to the buildings detail page (e.g. [Fire Station](https://anno1800.fandom.com/wiki/Fire_Station)) is parsed.
3. For each found details page the WikiText will be downloaded.
   The WikiText contains a template ([Infobox Buildings](https://anno1800.fandom.com/wiki/Template:Infobox_Buildings)) which will be parsed.
4. The parsed detailed information for each building will be added to the basic information from step 2.
5. In a final step all this information will be written to the presets file.

## How to use

After compiling the code start the `FandomParser.exe` console application.

### Possible switches

- `--version=`<br/>
  Use this switch to set the version of the preset file.<br/>
  Example: `--version=1.0.1.1`
- `--out=`
  Use this switch to specify a directory for the preset file.<br/>
  Example: `--out=C:\out`<br/>
  **Default:** "[location of `FandomParser.exe`]\\..\\..\\..\\..\\Presets"
- `--noWait`<br/>
  Use this switch to end the application without waiting for it to press any key.
- `--prettyPrint`<br/>
  Use this switch to save the preset file with indention.
