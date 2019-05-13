using AnnoDesigner.Presets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.PresetsLoader
{
    /// <summary>
    /// This encapsulates the logic of loading different versions of the icons.json file.
    /// </summary>
    public class IconMappingPresetsLoader
    {
        public IconMappingPresets Load()
        {
            IconMappingPresets result = null;

            try
            {
                result = DataIO.LoadFromFile<IconMappingPresets>(Path.Combine(App.ApplicationPath, Constants.IconNameFile));
                //no mappings = old version of file without version info
                if (result?.IconNameMappings == null)
                {
                    var oldIconMapping = DataIO.LoadFromFile<List<IconNameMap>>(Path.Combine(App.ApplicationPath, Constants.IconNameFile));
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
