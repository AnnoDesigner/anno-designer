[![GitHub](https://img.shields.io/github/license/AnnoDesigner/anno-designer)](https://github.com/AnnoDesigner/anno-designer/blob/master/LICENSE) [![version](https://img.shields.io/badge/latest--version-9.2-blue)](https://github.com/AnnoDesigner/anno-designer/releases/tag/AnnoDesignerv9.2) [![presets version](https://img.shields.io/badge/presets--version-4.0-blue)](https://github.com/AnnoDesigner/anno-designer/releases/tag/Presetsv4.0) [![Discord](https://img.shields.io/discord/571011757317947406?label=help%2Fdiscord)](https://discord.gg/JJpHWRB)

# Anno Designer

A building layout designer for Ubisoft's Anno-series

This is a fork of the project created by JcBernack - https://github.com/JcBernack/anno-designer

## Latest Releases

### [Anno Designer 9.2](https://github.com/AnnoDesigner/anno-designer/releases/tag/AnnoDesignerv9.2)

#### [Latest Presets file release](https://github.com/AnnoDesigner/anno-designer/releases/tag/Presetsv4.0)

The presets update can also be downloaded automatically by the application.

## Discord

Keep up to date the latest developments. If you have any ideas or questions that you want to share with us, or are running into an error with the designer, then the discord is the perfect place to join.

<https://discord.gg/JJpHWRB>

## Summary

The **Anno Designer** is a standalone windows application that can be used for creating and exporting layouts. It uses a drag/drop system and is intuitive and easy to use. **The Anno Designer supports the following Anno versions: 1404, 2070, 2205, 1800**.

## How to use

**Download the latest version from the [here](https://github.com/AnnoDesigner/anno-designer/releases/tag/AnnoDesignerv9.2)** (select the .exe file). Run it, and use it to design layouts!

Anno Designer can be started either without any arguments or by specifying one of supported verbs. When started without arguments, empty layout is open.

Run `AnnoDesigner.exe --help` for list of supported verbs

- `open` - opens specified layout file instead of empty layout
- `export` - exports specific layout file to PNG file and exits

Run `AnnoDesigner.exe <verb> --help` (for example `AnnoDesigner.exe open --help`) for more info about that verb and its arguments.


## Technology

This application is written in C# (.NET Framework 4.8) and uses WPF.

The .NET Framework is available via Windows Update and further information about installing is available on [this site](https://docs.microsoft.com/en-us/dotnet/framework/install/).

## Game data and icons

The building presets and icons are extracted from game files using the [RDA explorer](https://github.com/lysannschlegel/RDAExplorer)  and a custom script written by Peter Hozak. See the development pages at wikia: <http://anno2070.wikia.com/wiki/Development_Pages>

[StingMcRay](https://github.com/StingMcRay) has also done a lot of work on this, extracting icons from Anno 2205 and Anno 1800.

Included in this repo is a modified version of the PresetParser - which supports the extraction of data from all the different Anno versions. It is not required to run the app (and is not included in any release).

## License

[MIT](https://github.com/AnnoDesigner/anno-designer/blob/master/LICENSE)
