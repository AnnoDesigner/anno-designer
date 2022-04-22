using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using AnnoDesigner.Core.Layout.Models;
using NLog;

namespace AnnoDesigner.Core.Layout.Presets
{
    public class PresetLayoutLoader
    {
        private readonly IFileSystem _fileSystem;
        private static readonly Logger Logger = LogManager.GetLogger(nameof(PresetLayoutLoader));

        public Func<LayoutFile, Task<ImageSource>> RenderLayoutToImage { get; set; }
        public PresetLayoutLoader(Func<LayoutFile, Task<ImageSource>> renderLayoutToImage, IFileSystem fileSystem = null)
        {
            RenderLayoutToImage = renderLayoutToImage;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public async Task<List<IPresetLayout>> LoadAsync(string rootDirectory)
        {
            var data = await LoadDirectoryAsync(rootDirectory).ConfigureAwait(false);

            return data.Presets;
        }

        private async Task<PresetLayoutDirectory> LoadDirectoryAsync(string directory)
        {
            var subdirectories = await Task.WhenAll(_fileSystem.Directory.GetDirectories(directory).Select(LoadDirectoryAsync)).ConfigureAwait(false);
            var layouts = await Task.WhenAll(_fileSystem.Directory.GetFiles(directory, "*.zip").Select(LoadLayoutAsync)).ConfigureAwait(false);

            return new PresetLayoutDirectory()
            {
                Name = _fileSystem.Path.GetFileName(directory),
                Presets = subdirectories.Cast<IPresetLayout>().Concat(layouts.Where(f => f != null)).ToList()
            };
        }

        private async Task<PresetLayout> LoadLayoutAsync(string file)
        {
            try
            {
                return await PresetLayout.OpenAsync(file, RenderLayoutToImage, _fileSystem).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"Failed to parse loadout preset {file}");
            }

            return null;
        }
    }
}
