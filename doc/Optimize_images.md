# How to opitimize images

## General information

- The presets contain a lot of images of the format PNG (Portable Network Graphics).
- Every bit saved is beneficial in terms of download size and size of files on disk.
- PNG-files (and all compatiple viewers) support compression via the DEFLATE-algorithm.
- A newer implementation of this algorithm is Zopfli.

## Where to get a binary of ZopfliPNG

<https://github.com/imagemin/zopflipng-bin/tree/master/vendor/win32>

## How to optimize all images

1. Create a new directory `optimized` in `AnnoDesigner\icons`.
2. Open command line at the location of the downloaded zopfliPNG binary.
3. Paste command

```cmd
zopflipng.exe -m -y --iterations=500 --prefix=optimized\ "Path to repo\AnnoDesigner\icons\*.png"
```

### Example

```cmd
zopflipng.exe -m -y --iterations=500 --prefix=optimized\ "C:\Workspace\github\anno-designer\AnnoDesigner\icons\*.png"
```

All images will be optimized and saved in the `AnnoDesigner\icons\optimized` directory.
Copy the images you want to check in from `AnnoDesigner\icons\optimized` to `AnnoDesigner\icons` and delete the `AnnoDesigner\icons\optimized` directory.
