using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Presets.Models;

namespace AnnoDesigner.Core.Presets.Loader
{
    public class BuildingPresetsLoader
    {
        public BuildingPresets Load(string pathToBuildingPresetsFile = null)
        {
            BuildingPresets result = null;

            try
            {
                result = SerializationHelper.LoadFromFile<BuildingPresets>(pathToBuildingPresetsFile);
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
