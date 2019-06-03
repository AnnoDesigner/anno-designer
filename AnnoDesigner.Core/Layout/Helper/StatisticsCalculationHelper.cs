using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Layout.Helper
{
    public class StatisticsCalculationHelper
    {
        public StatisticsCalculationResult CalculateStatistics(List<AnnoObject> placedObjects)
        {
            var result = new StatisticsCalculationResult();

            // calculate bouding box
            var boxX = placedObjects.Max(_ => _.Position.X + _.Size.Width) - placedObjects.Min(_ => _.Position.X);
            var boxY = placedObjects.Max(_ => _.Position.Y + _.Size.Height) - placedObjects.Min(_ => _.Position.Y);
            // calculate area of all buildings
            var minTiles = placedObjects.Where(_ => !_.Road).Sum(_ => _.Size.Width * _.Size.Height);

            result.UsedAreaX = boxX;
            result.UsedAreaY = boxY;
            result.UsedTiles = boxX * boxY;

            result.MinTiles = minTiles;
            result.Efficiency = Math.Round(minTiles / boxX / boxY * 100);

            return result;
        }
    }
}
