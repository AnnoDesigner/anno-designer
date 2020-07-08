using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Helper
{
    public static class RoadSearchHelper
    {
        private static void DoNothing(AnnoObject objectInRange) { }

        /// <summary>
        /// Creates 2D array from input AnnoObjects.
        /// Every input AnnoObject will be accessable from every index which is covered by that AnnoObject on the grid.
        /// If object covers multiple grid cells, it will be in the array at every index, which it covers.
        /// </summary>
        public static AnnoObject[][] PrepareGridDictionary(IEnumerable<AnnoObject> placedObjects)
        {
            if (placedObjects is null || placedObjects.Count() < 1)
            {
                return null;
            }

            var maxX = Math.Max(0, (int)placedObjects.Max(o => o.Position.X + o.Size.Width) + 1);
            var maxY = Math.Max(0, (int)placedObjects.Max(o => o.Position.Y + o.Size.Height) + 1);

            var result = Enumerable.Range(0, maxX).Select(i => new AnnoObject[maxY]).ToArray();

            Parallel.ForEach(placedObjects, placedObject =>
            {
                var x = (int)placedObject.Position.X;
                var y = (int)placedObject.Position.Y;
                var w = placedObject.Size.Width;
                var h = placedObject.Size.Height;
                for (var i = 0; i < w; i++)
                    for (var j = 0; j < h; j++)
                        if (x + i >= 0 && y + j >= 0)
                            result[x + i][y + j] = placedObject;
            });

            return result;
        }

        /// <summary>
        /// Initiates breadth first search along objects which have Road property set to true.
        /// Algorithm iterates from maximum remaining influence range to 0.
        /// For each influence range it:
        ///   - adds all adjecent roads of start buildings with current influence range to current list of cells to visit
        ///   - visits every cell from current list and adds adjecent cells to list for next iteration
        /// </summary>
        public static bool[][] BreadthFirstSearch(
            IEnumerable<AnnoObject> placedObjects,
            IEnumerable<AnnoObject> startObjects,
            Func<AnnoObject, int> rangeGetter,
            AnnoObject[][] gridDictionary = null,
            Action<AnnoObject> inRangeAction = null)
        {
            if (startObjects.Count() == 0)
            {
                return new bool[0][];
            }

            gridDictionary = gridDictionary ?? PrepareGridDictionary(placedObjects);
            if (gridDictionary is null)
            {
                return new bool[0][];
            }

            inRangeAction = inRangeAction ?? DoNothing;

            var visitedObjects = new HashSet<AnnoObject>();
            var visitedCells = Enumerable.Range(0, gridDictionary.Length).Select(i => new bool[gridDictionary[0].Length]).ToArray();

            var distanceToStartObjects = startObjects.ToLookup(o => rangeGetter(o));
            var remainingDistance = distanceToStartObjects.Max(g => g.Key);
            var currentCells = new List<(int x, int y)>();
            var nextCells = new List<(int x, int y)>();

            void ProcessCell(int x, int y)
            {
                if (!visitedCells[x][y] && gridDictionary[x][y] is AnnoObject cellObject)
                {
                    if (cellObject.Road)
                    {
                        if (remainingDistance > 1)
                            nextCells.Add((x, y));
                    }
                    else if (visitedObjects.Add(cellObject))
                    {
                        inRangeAction(cellObject);
                    }
                }
                visitedCells[x][y] = true;
            }

            do
            {
                if (distanceToStartObjects.Contains(remainingDistance))
                {
                    // queue cells adjecent to starting objects, also sets cells inside of all start objects as visited, to exclude them from the search
                    foreach (var startObject in distanceToStartObjects[remainingDistance])
                    {
                        var initRange = rangeGetter(startObject);
                        var startX = (int)startObject.Position.X;
                        var startY = (int)startObject.Position.Y;
                        var leftX = startX - 1;
                        var rightX = (int)(startX + startObject.Size.Width);
                        var topY = startY - 1;
                        var bottomY = (int)(startY + startObject.Size.Height);

                        // queue top and bottom edges
                        for (var i = 0; i < startObject.Size.Width; i++)
                        {
                            var x = i + startX;

                            if (x >= 0 && topY >= 0 && gridDictionary[x][topY]?.Road == true)
                            {
                                nextCells.Add((x, topY));
                                visitedCells[x][topY] = true;
                            }

                            if (x >= 0 && bottomY >= 0 && gridDictionary[x][bottomY]?.Road == true)
                            {
                                nextCells.Add((x, bottomY));
                                visitedCells[x][bottomY] = true;
                            }

                        }
                        // queue left and right edges
                        for (var i = 0; i < startObject.Size.Height; i++)
                        {
                            var y = i + startY;

                            if (leftX >= 0 && y >= 0 && gridDictionary[leftX][y]?.Road == true)
                            {
                                nextCells.Add((leftX, y));
                                visitedCells[leftX][y] = true;
                            }

                            if (rightX >= 0 && y >= 0 && gridDictionary[rightX][y]?.Road == true)
                            {
                                nextCells.Add((rightX, y));
                                visitedCells[rightX][y] = true;
                            }
                        }

                        // visit all cells under start object
                        visitedObjects.Add(startObject);
                        for (var i = 0; i < startObject.Size.Width; i++)
                            for (var j = 0; j < startObject.Size.Height; j++)
                                if (startX + i >= 0 && startY + j >= 0)
                                    visitedCells[startX + i][startY + j] = true;
                    }
                }

                var temp = nextCells;
                nextCells = currentCells;
                currentCells = temp;

                if (remainingDistance > 1)
                {
                    foreach (var (x, y) in currentCells)
                    {
                        ProcessCell(x + 1, y);
                        if (x > 0)
                            ProcessCell(x - 1, y);
                        ProcessCell(x, y + 1);
                        if (y > 0)
                            ProcessCell(x, y - 1);
                    }
                }
                currentCells.Clear();
                remainingDistance--;
            } while (remainingDistance > 1);

            return visitedCells;
        }
    }
}
