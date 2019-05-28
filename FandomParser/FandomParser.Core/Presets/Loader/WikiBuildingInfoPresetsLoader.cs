using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FandomParser.Core.Helper;
using FandomParser.Core.Presets.Models;

namespace FandomParser.Core.Presets.Loader
{
    public class WikiBuildingInfoPresetsLoader
    {
        public WikiBuildingInfoPresets Load(string pathToPresetsFile)
        {
            WikiBuildingInfoPresets result = null;

            try
            {
                result = SerializationHelper.LoadFromFile<WikiBuildingInfoPresets>(pathToPresetsFile);
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
