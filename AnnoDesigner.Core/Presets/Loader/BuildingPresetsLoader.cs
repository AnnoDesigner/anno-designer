using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Presets.Models;
using NLog;

namespace AnnoDesigner.Core.Presets.Loader
{
    public class BuildingPresetsLoader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public BuildingPresets Load(string pathToBuildingPresetsFile)
        {
            BuildingPresets result = null;

            try
            {
                result = SerializationHelper.LoadFromFile<BuildingPresets>(pathToBuildingPresetsFile);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading the buildings.");
                throw;
            }

            return result;
        }
    }
}
