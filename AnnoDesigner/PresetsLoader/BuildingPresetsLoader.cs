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
    public class BuildingPresetsLoader
    {
        public BuildingPresets Load(string buildingPresetsFilePath = null)
        {
            BuildingPresets result = null;

            try
            {
                if (string.IsNullOrWhiteSpace(buildingPresetsFilePath))
                {
                    result = DataIO.LoadFromFile<BuildingPresets>(Path.Combine(App.ApplicationPath, Constants.BuildingPresetsFile));
                }
                else
                {
                    result = DataIO.LoadFromFile<BuildingPresets>(buildingPresetsFilePath);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error loading the buildings.{Environment.NewLine}{ex}");
                throw;
            }

            return result;
        }
    }
}
