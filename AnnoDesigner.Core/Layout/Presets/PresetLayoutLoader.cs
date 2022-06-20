using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnnoDesigner.Core.Layout.Models;
using Newtonsoft.Json;
using NLog;

namespace AnnoDesigner.Core.Layout.Presets
{
    public class PresetLayoutLoader
    {
        private readonly IFileSystem _fileSystem;
        private static readonly Logger Logger = LogManager.GetLogger(nameof(PresetLayoutLoader));

        public Func<LayoutFile, ImageSource> RenderLayoutToImage { get; set; }
        public PresetLayoutLoader(Func<LayoutFile, ImageSource> renderLayoutToImage, IFileSystem fileSystem = null)
        {
            RenderLayoutToImage = renderLayoutToImage;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public async Task<List<IPresetLayout>> LoadAsync(string rootDirectory)
        {
            var data = await LoadDirectoryAsync(rootDirectory);

            return data.Presets;
        }

        private async Task<PresetLayoutDirectory> LoadDirectoryAsync(string directory)
        {
            var subdirectories = await Task.WhenAll(_fileSystem.Directory.GetDirectories(directory).Select(LoadDirectoryAsync));
            var layouts = await Task.WhenAll(_fileSystem.Directory.GetFiles(directory, "*.zip").Select(LoadLayout));
            return new PresetLayoutDirectory()
            {
                Name = _fileSystem.Path.GetFileName(directory),
                Presets = subdirectories.Cast<IPresetLayout>().Concat(layouts.Where(f => f != null)).ToList()
            };
        }

        private async Task<PresetLayout> LoadLayout(string file)
        {
            try
            {
                using var zipFile = ZipFile.OpenRead(file);
                using var layoutFile = zipFile.GetEntry("layout.ad").Open();
                using var infoFile = zipFile.GetEntry("info.json")?.Open() ?? Stream.Null;
                using var infoStream = new StreamReader(infoFile);

                var layout = await new LayoutLoader().LoadLayoutAsync(layoutFile, true);
                if (layout != null)
                {
                    var info = JsonConvert.DeserializeObject<LayoutPresetInfo>(await infoStream.ReadToEndAsync()) ?? new LayoutPresetInfo(_fileSystem.Path.GetFileNameWithoutExtension(file));
                    var images = new List<ImageSource>()
                    {
                        RenderLayoutToImage(layout)
                    };

                    foreach (var item in zipFile.Entries)
                    {
                        using var stream = item.Open();
                        switch (_fileSystem.Path.GetExtension(item.FullName).ToLowerInvariant())
                        {
                            case ".png":
                            case ".jpg":
                            case ".jpeg":
                                await Task.Yield();

                                var imageStream = new MemoryStream();
                                stream.CopyTo(imageStream);
                                var image = new BitmapImage();
                                image.BeginInit();
                                image.StreamSource = imageStream;
                                image.EndInit();
                                image.Freeze();
                                images.Add(image);
                                break;
                        }
                    }

                    return new PresetLayout()
                    {
                        Info = info,
                        Layout = layout,
                        Images = images
                    };
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"Failed to parse loadout preset {file}");
            }

            return null;
        }
    }
}
