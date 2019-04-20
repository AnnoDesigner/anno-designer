# Anno Designer
A building layout designer for Ubisofts Anno-series

This is a fork of the project created by JcBernack - https://github.com/JcBernack/anno-designer

## Summary

This is a tool for creating building layouts for Ubisofts Anno-series.

Currently most layouts are created either by some kind of spreadsheet (excel, google-docs, ..) or directly with image editing. The target of this project is to supply users with an easy method to create good-looking, consistent layouts without having to think about how to do it.

## How to use

Download the latest version from the [Releases page](https://github.com/AgmasGold/anno-designer/releases). Run it, and use it to design layouts!

## Technology

This application is written in C# (.Net Framework 4.5.2) and uses WPF.

## Game data and icons

The building presets and icons are extracted from game files using the [RDA explorer](https://github.com/lysannschlegel/RDAExplorer)  and a custom script written by Peter Hozak. See the development pages at wikia: http://anno2070.wikia.com/wiki/Development_Pages

Sting_McRay has also done a lot of work on this, extracting icons from Anno 2205 and Anno 1800.

Included in this repo is a modified version of the PresetParser - which supports the extraction of data from all the different Anno versions. It is not required to run the app (and is not included in any release).

## Recent Changes

- Added support for 3 other languages - German, Polish and Russian. This work is ongoing (we don't have translations for everything yet).
- Added more color choices on the Buiding Settings menu.
- Reviewed the icon menu structure, so that buildings are now sorted by Anno version.
- Added Language Selection screen.
- Added persistent user settings (window size/position, language selection).

## License
MIT
