# Presets

This directory contains all the different preset files that are used by the different tools.

## Remarks

The files located here are the most recent versions.

All files are generated by different tools.

## The different preset files

- `presets.json`<br/>
  This is the main presets file which contains all informations about the buildings directly extracted from the game files for all supported versions of the game (1404, 2070, 2205, 1800).<br/>
  It is generated by the `PresetsParser`.
- `colors.json`<br/>
  This presets file contains predefined colors for the buildings from the `presets.json` file for use in the main app.<br/>
  It is generated by the `ColorPresetsDesigner`.
- `icons.json`<br/>
  This presets file contains localized names of the icons from the `icons` directory for use in the main app.<br/>
  It is generated by the `PresetsParser`.
- `wikiBuildingInfo.json`<br/>
  This presets files conatins additional information for the buildings from the `presets.json` file which are parsed directly from the wiki.<br/>
  It is generated by the `FandomParser`.