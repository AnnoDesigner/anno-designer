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
        public StatisticsCalculationResult CalculateStatistics(IEnumerable<AnnoObject> placedObjects)
        {
            if (placedObjects == null)
            {
                return null;
            }

            var result = new StatisticsCalculationResult();

            if (!placedObjects.Any())
            {
                return result;
            }

            /* old logic is easier to understand, but slower
             // calculate bouding box
             var boxX = placedObjects.Max(_ => _.Position.X + _.Size.Width) - placedObjects.Min(_ => _.Position.X);
             var boxY = placedObjects.Max(_ => _.Position.Y + _.Size.Height) - placedObjects.Min(_ => _.Position.Y);
             // calculate area of all buildings
             var minTiles = placedObjects.Where(_ => !_.Road).Sum(_ => _.Size.Width * _.Size.Height);
            */

            var maxX = double.MinValue;
            var maxY = double.MinValue;
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var sum = 0d;
            foreach (var curObject in placedObjects)
            {
                var curPosX = curObject.Position.X;
                var curPosY = curObject.Position.Y;
                var curSize = curObject.Size;

                var curMaxX = curPosX + curSize.Width;
                var curMaxY = curPosY + curSize.Height;

                if (curMaxX > maxX) maxX = curMaxX;
                if (curPosX < minX) minX = curPosX;

                if (curMaxY > maxY) maxY = curMaxY;
                if (curPosY < minY) minY = curPosY;

                if (!curObject.Road)
                {
                    sum += (curSize.Width * curSize.Height);
                }
            }

            // calculate bouding box
            var boxX = maxX - minX;
            var boxY = maxY - minY;
            // calculate area of all buildings
            var minTiles = sum;

            result.UsedAreaX = boxX;
            result.UsedAreaY = boxY;
            result.UsedTiles = boxX * boxY;

            result.MinTiles = minTiles;
            result.Efficiency = Math.Round(minTiles / boxX / boxY * 100);

            return result;
        }
    }
}
