using AnnoDesigner.model;
using AnnoDesigner.Presets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AnnoDesigner.PresetsLoader
{
    public class IconLoader
    {
        public Dictionary<string, IconImage> Load(IconMappingPresets iconNameMapping)
        {
            Dictionary<string, IconImage> result = null;

            try
            {
                result = new Dictionary<string, IconImage>();

                var pathToIconFolder = Path.Combine(App.ApplicationPath, Constants.IconFolder);

                foreach (var path in Directory.EnumerateFiles(pathToIconFolder, Constants.IconFolderFilter))
                {
                    var filenameWithoutExt = Path.GetFileNameWithoutExtension(path);
                    if (string.IsNullOrWhiteSpace(filenameWithoutExt))
                    {
                        continue;
                    }

                    var filenameWithExt = Path.GetFileName(path);

                    // try mapping to the icon translations
                    Dictionary<string, string> localizations = null;
                    if (iconNameMapping?.IconNameMappings != null)
                    {
                        var map = iconNameMapping.IconNameMappings.Find(x => string.Equals(x.IconFilename, filenameWithExt, StringComparison.OrdinalIgnoreCase));
                        if (map != null)
                        {
                            localizations = map.Localizations.Dict;
                        }
                    }

                    // add the current icon
                    result.Add(filenameWithoutExt, new IconImage(filenameWithoutExt, localizations, path));
                }

                // sort icons by their DisplayName
                result = result.OrderBy(x => x.Value.DisplayName).ToDictionary(x => x.Key, x => x.Value);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error loading the icons.{Environment.NewLine}{ex}");
                throw;
            }

            return result;
        }
    }
}
