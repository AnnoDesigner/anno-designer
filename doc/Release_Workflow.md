# Workflow for creating releases

## General information

- New releases of the main app (Anno Designer) contain all presets in their current version.
- Releases of the main app will have the tag `AnnoDesignervx.x` where x.x.x will be replaced with the current version.
- A preset is a file containing useful extra information for the main app.
- All preset files contain a field `Version` in the format `x.x.x.x`.

## Known preset files

### `presets.json`

This preset file is the main information source of the main app.

It contains all informations about the buildings directly extracted from the game files for all supported versions of the game (1404, 2070, 2205, 1800).

- Releases will have the tag `Presetsvx.x.x` where x.x.x will be replaced with the current version.
- Releases will only contain one asset (`presets.json`).

### `icons.json`

This preset file contains localized names of the icons from the `icons` folder for use in the main app.

- Releases will have the tag `PresetsIconsvx.x.x` where x.x.x will be replaced with the current version.
- Releases will only contain one asset (`icons.json`).

### update of `presets.json` with icons

This release contains an update of the `presets.json` and also some new icons.

- Releases will have the tag `Presetsvx.x.x` where x.x.x will be replaced with the current version.
- Releases will only contain one asset (`Presets.and.Icons.Update.vx.x.x.zip`).

### `colors.json`

This preset file contains predefined colors for the buildings from the `presets.json` file for use in the main app.

- Releases will have the tag `PresetsColorsvx.x.x` where x.x.x will be replaced with the current version.
- Releases will only contain one asset (`colors.json`).

### `wikiBuildingInfo.json`

This presets files conatins additional information for the buildings from the `presets.json` file which are parsed directly from the wiki.

- Releases will have the tag `PresetsWikiBuildingInfovx.x.x` where x.x.x will be replaced with the current version.
- Releases will only contain one asset (`wikiBuildingInfo.json`).