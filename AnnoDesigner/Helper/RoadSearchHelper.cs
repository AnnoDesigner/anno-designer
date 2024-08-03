using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Layout.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnoDesigner.Helper;

public static class RoadSearchHelper
{
    private static readonly StatisticsCalculationHelper _statisticsCalculationHelper = new();

    private static void DoNothing(AnnoObject objectInRange) { }

    /// <summary>
    /// Creates offset 2D array from input AnnoObjects.
    /// Whole array is offset so that all input AnnoObjects are fully inside the array with one empty grid on each edge.
    /// Every input AnnoObject will be accessable from every index which is covered by that AnnoObject on the grid.
    /// If object covers multiple grid cells, it will be in the array at every index, which it covers.
    /// </summary>
    public static Moved2DArray<AnnoObject> PrepareGridDictionary(IEnumerable<AnnoObject> placedObjects)
    {
        if (placedObjects is null || placedObjects.WithoutIgnoredObjects().Count() < 1)
        {
            return null;
        }

        Core.Layout.Models.StatisticsCalculationResult statistics = _statisticsCalculationHelper.CalculateStatistics(placedObjects);
        (int x, int y) offset = ((int)statistics.MinX - 1, (int)statistics.MinY - 1);

        // make an array with one free grid cell on each edge
        int countY = (int)(statistics.MaxY - statistics.MinY + 2);
        int countX = (int)(statistics.MaxX - statistics.MinX + 2);
        AnnoObject[][] result = Enumerable.Range(0, countX)
            .Select(i => new AnnoObject[countY])
            .ToArrayWithCapacity(countX);

        _ = Parallel.ForEach(placedObjects.WithoutIgnoredObjects(), placedObject =>
          {
              int x = (int)placedObject.Position.X;
              int y = (int)placedObject.Position.Y;
              double w = placedObject.Size.Width;
              double h = placedObject.Size.Height;
              for (int i = 0; i < w; i++)
              {
                  for (int j = 0; j < h; j++)
                  {
                      result[x + i - offset.x][y + j - offset.y] = placedObject;
                  }
              }
          });

        return new Moved2DArray<AnnoObject>()
        {
            Array = result,
            Offset = offset
        };
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
        Moved2DArray<AnnoObject> gridDictionary = null,
        Action<AnnoObject> inRangeAction = null)
    {
        if (startObjects.Count() == 0)
        {
            return new bool[0][];
        }

        gridDictionary ??= PrepareGridDictionary(placedObjects);
        if (gridDictionary is null)
        {
            return new bool[0][];
        }

        inRangeAction ??= DoNothing;

        HashSet<AnnoObject> visitedObjects = new(placedObjects.Count() / 2);//inital capacity is half of all placed objecs to avoid resizing the HashSet
        bool[][] visitedCells = Enumerable.Range(0, gridDictionary.Count).Select(i => new bool[gridDictionary[0].Length]).ToArrayWithCapacity(gridDictionary.Count);

        ILookup<int, AnnoObject> distanceToStartObjects = startObjects.ToLookup(o => rangeGetter(o));
        int remainingDistance = distanceToStartObjects.Max(g => g.Key);
        List<(int x, int y)> currentCells = [];
        List<(int x, int y)> nextCells = [];

        void ProcessCell(int x, int y)
        {
            if (!visitedCells[x][y] && gridDictionary[x][y] is AnnoObject cellObject)
            {
                if (cellObject.Road)
                {
                    if (remainingDistance > 1)
                    {
                        nextCells.Add((x, y));
                    }
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
            // ILookup returns empty collection if key is not found
            // queue cells adjecent to starting objects, also sets cells inside of all start objects as visited, to exclude them from the search
            foreach (AnnoObject startObject in distanceToStartObjects[remainingDistance])
            {
                int initRange = rangeGetter(startObject);
                int startX = (int)startObject.Position.X - gridDictionary.Offset.x;
                int startY = (int)startObject.Position.Y - gridDictionary.Offset.y;
                int leftX = startX - 1;
                int rightX = (int)(startX + startObject.Size.Width);
                int topY = startY - 1;
                int bottomY = (int)(startY + startObject.Size.Height);

                // queue top and bottom edges
                for (int i = 0; i < startObject.Size.Width; i++)
                {
                    int x = i + startX;

                    if (gridDictionary[x][topY]?.Road == true)
                    {
                        nextCells.Add((x, topY));
                        visitedCells[x][topY] = true;
                    }

                    if (gridDictionary[x][bottomY]?.Road == true)
                    {
                        nextCells.Add((x, bottomY));
                        visitedCells[x][bottomY] = true;
                    }

                }
                // queue left and right edges
                for (int i = 0; i < startObject.Size.Height; i++)
                {
                    int y = i + startY;

                    if (gridDictionary[leftX][y]?.Road == true)
                    {
                        nextCells.Add((leftX, y));
                        visitedCells[leftX][y] = true;
                    }

                    if (gridDictionary[rightX][y]?.Road == true)
                    {
                        nextCells.Add((rightX, y));
                        visitedCells[rightX][y] = true;
                    }
                }

                // visit all cells under start object
                _ = visitedObjects.Add(startObject);
                for (int i = 0; i < startObject.Size.Width; i++)
                {
                    for (int j = 0; j < startObject.Size.Height; j++)
                    {
                        visitedCells[startX + i][startY + j] = true;
                    }
                }
            }

            (currentCells, nextCells) = (nextCells, currentCells);
            if (remainingDistance > 1)
            {
                foreach ((int x, int y) in currentCells)
                {
                    ProcessCell(x + 1, y);
                    if (x > 0)
                    {
                        ProcessCell(x - 1, y);
                    }

                    ProcessCell(x, y + 1);
                    if (y > 0)
                    {
                        ProcessCell(x, y - 1);
                    }
                }
            }

            currentCells.Clear();
            remainingDistance--;
        } while (remainingDistance > 1);

        return visitedCells;
    }
}
