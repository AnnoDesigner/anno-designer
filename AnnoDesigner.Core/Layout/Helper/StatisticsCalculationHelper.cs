﻿using System;
using System.Collections.Generic;
using System.Linq;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Layout.Helper
{
    public class StatisticsCalculationHelper
    {
        /// <summary>
        /// Computes statistics for a given collection of <see cref="AnnoObject"/>.
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="includeRoads"></param>
        /// <returns>Properties about the collection - minX, maxX</returns>
        public StatisticsCalculationResult CalculateStatistics(IEnumerable<AnnoObject> objects, bool includeRoads = false)
        {
            if (objects == null)
            {
                return null;
            }

            if (objects.Count() == 0 || objects.WithoutIgnoredObjects().Count() == 0)
            {
                return StatisticsCalculationResult.Empty;
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
            foreach (var curObject in objects.WithoutIgnoredObjects())
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

                if (includeRoads || !curObject.Road)
                {
                    sum += (curSize.Width * curSize.Height);
                }
            }

            // calculate bouding box
            var boxX = maxX - minX;
            var boxY = maxY - minY;
            // calculate area of all buildings
            var minTiles = sum;

            var usedTiles = boxX * boxY;
            var efficiency = Math.Round(minTiles / boxX / boxY * 100);

            return new StatisticsCalculationResult(minX, minY, maxX, maxY, boxX, boxY, usedTiles, minTiles, efficiency);
        }
    }
}
