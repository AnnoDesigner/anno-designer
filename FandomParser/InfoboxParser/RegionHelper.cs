using System;
using System.Collections.Generic;
using System.Text;
using FandomParser.Core.Presets.Models;
using InfoboxParser.Models;
using NLog;

namespace InfoboxParser
{
    public class RegionHelper : IRegionHelper
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public WorldRegion GetRegion(string regionName)
        {
            var result = WorldRegion.Unknown;

            switch (regionName)
            {
                case "OW":
                case "Old World":
                case "The Old World":
                    result = WorldRegion.OldWorld;
                    break;
                case "NW":
                case "The New World":
                case "New World":
                    result = WorldRegion.NewWorld;
                    break;
                case "The Arctic":
                case "Arctic":
                    result = WorldRegion.Arctic;
                    break;
                default:
                    logger.Debug($"could not parse region: {regionName}");
                    break;
            }

            return result;
        }
    }
}
