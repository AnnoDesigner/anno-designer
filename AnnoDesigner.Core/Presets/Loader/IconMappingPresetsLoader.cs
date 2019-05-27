using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Presets.Models;
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
        public IconMappingPresets Load(string pathToIconNameMappingFile)
        {
            IconMappingPresets result = null;

            try
            {
                result = SerializationHelper.LoadFromFile<IconMappingPresets>(pathToIconNameMappingFile);
                //no mappings = old version of file without version info
                if (result?.IconNameMappings == null)
                {
                    var oldIconMapping = SerializationHelper.LoadFromFile<List<IconNameMap>>(pathToIconNameMappingFile);
                    result.IconNameMappings = oldIconMapping;

                    result.Version = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error loading the icon name mapping file.{Environment.NewLine}{ex}");
                throw;
            }

            return result;
        }
    }
}
