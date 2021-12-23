Each ZIP file in this folder and all subfolders should contain information about layout preset

Layout preset must have:
- layout file named layout.ad

Layout preset can have:
- info file names info.json
    - JSON schema of this file is defined in LayoutInfoSchema.json in this folder
    - if not provided the layout preset's name will be the ZIP file name
- any number of image files which should be associated with that layout
    - supported file types are "png", "jpg" and "jpeg" (case insensitive)

ZIP files which fail to load as layout preset are ignored and not shown in AnnoDesigner
Info about ZIP file failing to load is writen to the application log
