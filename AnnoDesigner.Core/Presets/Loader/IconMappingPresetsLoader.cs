using AnnoDesigner.Core.Helper;
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
    /// <summary>
    /// This encapsulates the logic of loading different versions of the icons.json file.
    /// </summary>
    public class IconMappingPresetsLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public IconMappingPresets Load(string pathToIconNameMappingFile)
        {
            if (string.IsNullOrWhiteSpace(pathToIconNameMappingFile))
            {
                throw new ArgumentNullException(nameof(pathToIconNameMappingFile));
            }

            using (var stream = File.Open(pathToIconNameMappingFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Load(stream);
            }
        }

        public IconMappingPresets Load(Stream streamWithIconNameMappingFile)
        {
            if (streamWithIconNameMappingFile == null)
            {
                throw new ArgumentNullException(nameof(streamWithIconNameMappingFile));
            }

            IconMappingPresets result = null;

            try
            {
                result = SerializationHelper.LoadFromStream<IconMappingPresets>(streamWithIconNameMappingFile);
                //no mappings = old version of file without version info
                if (result?.IconNameMappings == null)
                {
                    if (streamWithIconNameMappingFile.CanSeek)
                    {
                        streamWithIconNameMappingFile.Position = 0;
                    }

                    var oldIconMapping = SerializationHelper.LoadFromStream<List<IconNameMap>>(streamWithIconNameMappingFile);
                    result.IconNameMappings = oldIconMapping;

                    result.Version = string.Empty;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading the icon name mapping file.");
                throw;
            }

            return result;
        }
    }
}
