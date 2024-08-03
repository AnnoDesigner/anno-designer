# Anno Designer (Improved Fork)

[![GitHub](https://img.shields.io/github/license/AnnoDesigner/anno-designer)](https://github.com/AnnoDesigner/anno-designer/blob/master/LICENSE) [![version](https://img.shields.io/badge/latest--version-9.4-blue)](https://github.com/AnnoDesigner/anno-designer/releases/tag/AnnoDesignerv9.4) [![presets version](https://img.shields.io/badge/presets--version-5.1-blue)](https://github.com/AnnoDesigner/anno-designer/releases/tag/Presetsv5.1) [![Discord](https://img.shields.io/discord/571011757317947406?label=help%2Fdiscord)](https://discord.gg/JJpHWRB)

A building layout designer for Ubisoft's Anno-series with significant improvements.

This is a fork of the project originally created by JcBernack - https://github.com/JcBernack/anno-designer

## Key Improvements in this Fork

- Updated to .NET 9 for improved performance and new language features
- Migrated to Fluent UI for a modern and consistent user interface
- Refactored project structure for better maintainability and extensibility

## Latest Releases

### [Anno Designer 9.4](https://github.com/AnnoDesigner/anno-designer/releases/tag/AnnoDesignerv9.4)
#### [Latest Presets file release](https://github.com/AnnoDesigner/anno-designer/releases/tag/Presetsv5.1)
The presets update can also be downloaded automatically by the application.

## Discord

Join our Discord community to keep up with the latest developments, share ideas, ask questions, or get help with any issues:

<https://discord.gg/JJpHWRB>

## Summary

The **Anno Designer** is a standalone Windows application for creating and exporting layouts for Anno games. It features an intuitive drag-and-drop interface, making it easy to use for both beginners and experienced players.

**Supported Anno versions: 1404, 2070, 2205, 1800**

## How to Use

1. **Download the latest version [here](https://github.com/AnnoDesigner/anno-designer/releases/tag/AnnoDesignerv9.4)** (select the .exe file).
2. Run the application to start designing your layouts.

For advanced usage, Anno Designer can be started from the command line. See [this documentation](doc/CommandLineParameters.md) for more information and examples.

## Technology

- Written in C# (.NET 9)
- Uses WPF (Windows Presentation Foundation) with Fluent UI
- Requires the .NET 9 runtime, which can be downloaded from the [official Microsoft website](https://dotnet.microsoft.com/download/dotnet/9.0)

## Game Data and Icons

Building presets and icons are extracted from game files using:
- [RDA Explorer](https://github.com/lysannschlegel/RDAExplorer)
- Custom script by Peter Hozak
- Additional work by [StingMcRay](https://github.com/StingMcRay) for Anno 2205 and Anno 1800 icons

For more information, visit the development pages at: <http://anno2070.wikia.com/wiki/Development_Pages>

A modified version of the PresetParser, supporting data extraction from all Anno versions, is included in this repository. It is not required to run the app and is not included in releases.

## Contributing

We welcome contributions to this project! If you'd like to contribute, please:

1. Fork the repository
2. Create a new branch for your feature or bug fix
3. Make your changes and commit them with clear, descriptive messages
4. Push your changes to your fork
5. Create a pull request with a description of your changes

## License

[MIT](https://github.com/AnnoDesigner/anno-designer/blob/master/LICENSE)