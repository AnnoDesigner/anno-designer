# Anno Designer
A building layout designer for Ubisofts Anno-series

This is a fork of the project created by JcBernack - https://github.com/JcBernack/anno-designer

## Summary

This is a tool for creating building layouts for Ubisofts Anno-series.

Currently most layouts are created either by some kind of spreadsheet (excel, google-docs, ..) or directly with image editing. The target of this project is to supply users with an easy method to create good-looking, consistent layouts without having to think about how to do it.

## Technology

This application is written in C# (.Net Framework 4.6.1) and uses WPF.

The building presets and icons are extracted from game files using the [RDA explorer](https://github.com/lysannschlegel/RDAExplorer)  and a custom script written by Peter Hozak. See the development pages at wikia: http://anno2070.wikia.com/wiki/Development_Pages

Sting_McRay has also done a lot of work on this, extracting icons from Anno 2205 and Anno 1800.

## Recent Changes

- Added support for 3 other languages - German, Polish and Russian. This work is ongoing (we don't have translations for everything yet).
- Added more color choices on the Buiding Settings menu.
- Reviewed the icon menu structure, so that buildings are now sorted by Anno version.
- Added Language Selection screen.
- Added persistent user settings (window size/position, language selection).

## License
MIT
