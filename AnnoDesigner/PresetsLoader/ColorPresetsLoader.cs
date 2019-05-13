using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Presets;

namespace AnnoDesigner.PresetsLoader
{
    public class ColorPresetsLoader
    {
        public ColorPresets Load()
        {
            ColorPresets result = null;

            try
            {
                result = DataIO.LoadFromFile<ColorPresets>(Path.Combine(App.ApplicationPath, Constants.ColorPresetsFile));
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error loading the colors.{Environment.NewLine}{ex}");
                throw;
            }

            return result;
        }

        public ColorScheme LoadDefaultScheme()
        {
            ColorScheme result = null;

            try
            {
                var colorPresets = DataIO.LoadFromFile<ColorPresets>(Path.Combine(App.ApplicationPath, Constants.ColorPresetsFile));

                result = colorPresets?.AvailableSchemes.FirstOrDefault(x => x.Name.Equals("Default", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error loading the default scheme.{Environment.NewLine}{ex}");
                throw;
            }

            return result;
        }
    }
}
