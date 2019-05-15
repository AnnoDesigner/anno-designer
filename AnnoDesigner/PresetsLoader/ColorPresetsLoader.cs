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
        public ColorPresets Load(string colorPresetsFilePath = null)
        {
            ColorPresets result = null;

            try
            {
                if (String.IsNullOrWhiteSpace(colorPresetsFilePath))
                {
                    result = DataIO.LoadFromFile<ColorPresets>(Path.Combine(App.ApplicationPath, Constants.ColorPresetsFile));
                }
                else
                {
                    result = DataIO.LoadFromFile<ColorPresets>(colorPresetsFilePath);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error loading the colors.{Environment.NewLine}{ex}");
                throw;
            }

            return result;
        }

        public ColorScheme LoadDefaultScheme(string colorPresetsFilePath = null)
        {
            ColorScheme result = null;

            try
            {
                var colorPresets = Load(colorPresetsFilePath);

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
