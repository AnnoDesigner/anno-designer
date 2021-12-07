# Information about predefined colors

The app contains predefined buildings (commonly called `Presets` or `Preset buildings`) inside the file `presets.json`.</br>
Also the app supports and contains predefined colors for those buildings inside the file `colors.json`.</br>
This file can be generated and adjusted with the separate tool `ColorPresetsDesigner`.

## General information about a building

To distinguish the different predefined buildings in a layout, they have a unique identifier (`Identifier`) property.</br>
So e.g. all fire stations have the identifier "Institution_02". This value is directly extracted from the game files (there are some identifiers that are made by hand).</br>
To support a predefined color a building can also have a `Template` associated with it.

## Structure of a predefined color

A predefined color is represented by the type `PredefinedColor`.</br>
This type has properties

- for the color (`Color`)
- for the associated Template (`TargetTemplate`)
- for the list of associated identifiers (`TargetIdentifiers`)

## How to get a predefined color for a building

To get a predefined color for a building and loading the predefined colors there is the type `ColorPresetsHelper`.</br>
The general logic of finding a color for a building is:

1. load all predefined colors
2. get the value of `Template` from the building
3. get a list of predefined colors from 1. that have a matching value for the template (`TargetTemplate` == `Template`)
4. get first entry from 3. that is associated with the `Identifier` of the building (`TargetIdentifiers` contains `Identifier`)
    1. entry is found -> return `Color` from entry
    2. entry is not found -> return `Color` from first entry of 3. that has no associated identifiers (`TargetIdentifiers` is empty)
    3. if still no entry is found -> return `Color` from first entry of 3

### TL;DR

- a building can have a `Template`
- a `Template` can be general and only contain a `Color`
- a `Template` can be more specific and can also contain a list of `Identifier`s
