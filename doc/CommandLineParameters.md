# AnnoDesigner command line parameters

AnnoDesigner supports 2 verbs when starting it from command line.

- `open` - AnnoDesigner starts with specific layout file openned.
- `export`- AnnoDesigner exports specific layout file to image and closes immediately. See supported export parameters with `AnnoDesigner.exe export --help`.

If no verb is specified AnnoDesigner starts with empty layout.

## Example usages

### Show all verbs

`AnnoDesigner.exe --help`

### Show info about specific verb

`AnnoDeisgner.exe <verb> --help`
`AnnoDeisgner.exe export --help`

### Open specific layout file

`AnnoDeisgner.exe open C:\path\to\layout\file.ad`

### Export specific layout file to image

`AnnoDeisgner.exe export C:\path\to\layout\file.ad C:\path\to\exported\image.png`
`AnnoDeisgner.exe export C:\path\to\layout\file.ad C:\path\to\exported\image.png --gridSize 10`
`AnnoDeisgner.exe export C:\path\to\layout\file.ad C:\path\to\exported\image.png --renderIcon --renderLabel`
