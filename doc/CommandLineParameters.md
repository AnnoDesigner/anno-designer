# AnnoDesigner command line parameters

AnnoDesigner supports the following verbs when starting it from command line:

- `open`  
  AnnoDesigner starts with the specified layout file opened.
- `export`  
  AnnoDesigner exports the specified layout file to an image and closes immediately.  
  See supported export parameters with `AnnoDesigner.exe export --help`.

If no verb is specified AnnoDesigner starts with an empty layout (as usual).

## Example usages

### Show all supported verbs

```cmd
AnnoDesigner.exe --help
```

### Show info about specific verb

```cmd
AnnoDesigner.exe <verb> --help
```

For example to show information about the `export` verb, use the following command.

```cmd
AnnoDesigner.exe export --help
```

### Open specific layout file

```cmd
AnnoDesigner.exe open "C:\path\to\layout\file.ad"
```

### Export specific layout file to image

```cmd
AnnoDesigner.exe export "C:\path\to\layout\file.ad" "C:\path\to\exported\image.png"
```

```cmd
AnnoDesigner.exe export "C:\path\to\layout\file.ad" "C:\path\to\exported\image.png" --gridSize 10
```

```cmd
AnnoDesigner.exe export "C:\path\to\layout\file.ad" "C:\path\to\exported\image.png" --renderIcon=true --renderLabel=true
```

## Example script files

For easier use you can create some script files. The following examples are batch files (*.bat) which can be executed from explorer.  
*It is assumed that the batch file is in the same directory as the `AnnoDesigner.exe`.*  
To have a starting point, just create an empty batch file, copy the script and adjust the file paths and parameters.

### Script to open specific layout file

```bat
@echo off
rem first parameter is the window title
start "AnnoDesigner" "%~dp0\AnnoDesigner.exe" open "C:\path\to\layout\file.ad"
```

### Script to export specific layout file to image

```bat
@echo off
rem first parameter is the window title
start "AnnoDesigner" "%~dp0\AnnoDesigner.exe" export "C:\path\to\layout\file.ad" "C:\path\to\exported\image.png" --gridSize 20 --renderIcon=true --renderVersion=true
```

### Script to export all layouts in a directory to images

```bat
@echo off
set pathToAnnoDesigner="%~dp0\AnnoDesigner.exe"
set folderWithLayouts=C:\path\to\layouts\

rem /d = also change drive if necessary
cd /d %folderWithLayouts%

rem %%f = current file name with extension
rem %%~nf = current file name without extension
rem %%~dpnf = current file path without extension
rem %%~ff = current file path (fully qualified)
rem ^> = >, >, | need to be escaped
for %%f in (*.ad) do (
echo exporting layout: "%%~ff" -^> "%%~dpnf.png"

%pathToAnnoDesigner% export "%%f" "%%~dpnf.png" --gridSize 20 --renderIcon=false --renderVersion=true --renderStatistics=true
)

cd /d %~dp0

echo finished
```
