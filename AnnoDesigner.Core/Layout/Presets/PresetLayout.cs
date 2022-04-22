using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using Newtonsoft.Json;

namespace AnnoDesigner.Core.Layout.Presets
{
    public class PresetLayout : Notify, IPresetLayout, IDisposable
    {
        private bool disposed;
        private List<ImageSource> images;
        private IFileSystem fileSystem;
        private Func<LayoutFile, Task<ImageSource>> renderLayoutToImage;

        public MultilangInfo Name => Info.Name;

        public LayoutPresetInfo Info { get; set; }

        public string Author { get; set; }

        public string AuthorContact { get; set; }

        public LayoutFile Layout { get; set; }

        public List<ImageSource> Images
        {
            get { return images; }
            set { UpdateProperty(ref images, value); }
        }

        private ZipArchive ZipArchive { get; set; }

        [Obsolete($"Constructor should not be used to construct {nameof(PresetLayout)}. Use {nameof(OpenAsync)} instead.")]
        public PresetLayout() { }

        public static async Task<PresetLayout> OpenAsync(string zipFile, Func<LayoutFile, Task<ImageSource>> renderLayoutToImage = null, IFileSystem fileSystem = null)
        {
            fileSystem ??= new FileSystem();
            var zip = ZipFile.OpenRead(zipFile);

            using var layoutFile = zip.GetEntry("layout.ad").Open();
            var layout = await new LayoutLoader().LoadLayoutAsync(layoutFile, true).ConfigureAwait(false);
            if (layout == null)
            {
                throw new ArgumentException("Provided ZIP file doesn't contain valid AD layout file with name layout.ad");
            }

            using var infoFile = zip.GetEntry("info.json")?.Open() ?? Stream.Null;
            using var infoStream = new StreamReader(infoFile);
            var info = JsonConvert.DeserializeObject<LayoutPresetInfo>(await infoStream.ReadToEndAsync().ConfigureAwait(false))
                        ?? new LayoutPresetInfo(fileSystem.Path.GetFileNameWithoutExtension(zipFile));

            return new PresetLayout()
            {
                Info = info,
                Layout = layout,
                ZipArchive = zip,
                fileSystem = fileSystem,
                renderLayoutToImage = renderLayoutToImage
            };
        }

        private bool IsImage(ZipArchiveEntry f)
        {
            switch (fileSystem.Path.GetExtension(f.FullName).ToLowerInvariant())
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                    return true;
                default:
                    return false;
            }
        }

        public async Task LoadImages(Func<LayoutFile, Task<ImageSource>> renderLayoutToImage = null)
        {
            renderLayoutToImage ??= this.renderLayoutToImage;

            if (renderLayoutToImage == null)
            {
                throw new ArgumentNullException(nameof(renderLayoutToImage), $"Argument not provided nor set in {nameof(OpenAsync)}");
            }

            var images = new List<ImageSource>
            {
                await renderLayoutToImage(Layout)
            };

            await Task.WhenAll(ZipArchive.Entries.Where(IsImage).Select(async f =>
            {
                using var stream = f.Open();
                var imageStream = new MemoryStream();
                await stream.CopyToAsync(imageStream).ConfigureAwait(true);

                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = imageStream;
                image.EndInit();
                image.Freeze();
                images.Add(image);
            }));

            Images = images;
        }

        public void UnloadImages()
        {
            Images = null;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ZipArchive.Dispose();
                    ZipArchive = null;
                }

                disposed = true;
            }
        }
    }
}
