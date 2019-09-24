using AnnoDesigner.Core;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Presets.Loader
{
    public class IconLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Dictionary<string, IconImage> Load(string pathToIconFolder, IconMappingPresets iconNameMapping)
        {
            Dictionary<string, IconImage> result = null;

            try
            {
                result = new Dictionary<string, IconImage>();

                foreach (var path in Directory.EnumerateFiles(pathToIconFolder, CoreConstants.IconFolderFilter))
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
                result = result.OrderBy(x => x.Value.DisplayName).ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);//make sure ContainsKey is caseInSensitive
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading the icons.");
                throw;
            }

            return result;
        }
    }
}
