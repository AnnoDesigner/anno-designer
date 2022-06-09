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
AnnoDesigner.exe export "C:\path\to\layout\file.ad" "C:\path\to\exported\image.png" --renderIcon --renderLabel
```
