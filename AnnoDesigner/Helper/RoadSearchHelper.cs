using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Helper
{
    public static class RoadSearchHelper
    {
        private static void DoNothing(AnnoObject objectInRange) { }

        /// <summary>
        /// Creates sparse 2D dictionary from input AnnoObjects.
        /// Every input AnnoObject will be accessable from every key which is covered by that AnnoObject on the grid.
        /// If object covers multiple grid cells, it will be in the dictionary at every key, which it covers.
        /// </summary>
        public static Dictionary<double, Dictionary<double, AnnoObject>> PrepareGridDictionary(IEnumerable<AnnoObject> placedObjects)
        {
            var result = new Dictionary<double, Dictionary<double, AnnoObject>>();

            foreach (var placedObject in placedObjects)
            {
                var x = placedObject.Position.X;
                var y = placedObject.Position.Y;
                for (var i = 0; i < placedObject.Size.Width; i++)
                {
                    if (!result.ContainsKey(x + i))
                    {
                        result.Add(x + i, new Dictionary<double, AnnoObject>());
                    }

                    for (var j = 0; j < placedObject.Size.Height; j++)
                    {
                        result[x + i][y + j] = placedObject;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Initiates breadth first search along objects which have Road property set to true.
        /// Search starts from grid cells (that contain road) adjecent to objects from startObjects input.
        /// Cells are processed FirstIn-FirstOut (queue). When cell is dequeued, adjecent cells are checked
        /// if they weren't visited before AND aren't already in queue then:
        ///   - if they contain road, they are added to the queue,
        ///   - otherwise inRangeAction is invoked with the object in that cell.
        /// Either way, the cell is marked as visited to avoid it being queued multiple times.
        ///   
        /// Cells are queued with remaining distance from the start object.
        /// When adjecent cell is queued, it is queued with distance decresed by 1.
        /// When dequeued cell has remaining distance lower than 1, nothing is done, because adjecent cells would be outside of influence range.
        /// 
        /// These steps cause the search to happen in layers from start objects.
        /// First all cells 1 away from start objects are processed, then all cells with distance 2 and so on.
        /// </summary>
        public static HashSet<Point> BreadthFirstSearch(
            IEnumerable<AnnoObject> placedObjects,
            IEnumerable<AnnoObject> startObjects,
            Func<AnnoObject, double> rangeGetter,
            Action<AnnoObject> inRangeAction = null,
            Dictionary<double, Dictionary<double, AnnoObject>> gridDictionary = null)
        {
            var visitedCells = new HashSet<Point>();
            if (startObjects.Count() == 0)
                return visitedCells;

            inRangeAction = inRangeAction ?? DoNothing;
            gridDictionary = gridDictionary ?? PrepareGridDictionary(placedObjects);

            var searchedCells = new Queue<(double remainingDistance, double x, double y)>();
            var visitedObjects = new HashSet<AnnoObject>();

            // queue cells adjecent to starting objects, also sets cells inside of all start objects as visited, to exclude them from the search
            foreach (var startObject in startObjects)
            {
                // queue top and bottom edges
                for (var i = 0; i < startObject.Size.Width; i++)
                {
                    var x = i + startObject.Position.X;
                    if (gridDictionary.ContainsKey(x))
                    {
                        var y = startObject.Position.Y - 1;
                        if (gridDictionary[x].ContainsKey(y) && gridDictionary[x][y].Road)
                        {
                            searchedCells.Enqueue((rangeGetter(startObject), x, y));
                            visitedCells.Add(new Point(x, y));
                        }

                        y = startObject.Position.Y + startObject.Size.Height;
                        if (gridDictionary[x].ContainsKey(y) && gridDictionary[x][y].Road)
                        {
                            searchedCells.Enqueue((rangeGetter(startObject), i + startObject.Position.X, y));
                            visitedCells.Add(new Point(i + startObject.Position.X, y));
                        }
                    }

                }
                // queue left and right edges
                for (var i = 0; i < startObject.Size.Height; i++)
                {
                    var y = i + startObject.Position.Y;
                    var x = startObject.Position.X - 1;
                    if (gridDictionary.ContainsKey(x) && gridDictionary[x].ContainsKey(y) && gridDictionary[x][y].Road)
                    {
                        searchedCells.Enqueue((rangeGetter(startObject), x, y));
                        visitedCells.Add(new Point(x, y));
                    }
                    x = startObject.Position.X + startObject.Size.Width;
                    if (gridDictionary.ContainsKey(x) && gridDictionary[x].ContainsKey(y) && gridDictionary[x][y].Road)
                    {
                        searchedCells.Enqueue((rangeGetter(startObject), x, y));
                        visitedCells.Add(new Point(x, y));
                    }
                }

                // visit all cells under start object
                visitedObjects.Add(startObject);
                for (var i = 0; i < startObject.Size.Width; i++)
                    for (var j = 0; j < startObject.Size.Height; j++)
                        visitedCells.Add(new Point(startObject.Position.X + i, startObject.Position.Y + j));
            }

            void Enqueue(double distance, double x, double y)
            {
                if (!visitedCells.Contains(new Point(x, y)) && gridDictionary.ContainsKey(x) && gridDictionary[x].ContainsKey(y))
                {
                    var inRangeObject = gridDictionary[x][y];

                    if (inRangeObject.Road && distance > 0)
                    {
                        searchedCells.Enqueue((distance, x, y));
                    }
                    else if (!visitedObjects.Contains(inRangeObject))
                    {
                        inRangeAction(inRangeObject);
                    }
                    visitedObjects.Add(inRangeObject);
                }
                visitedCells.Add(new Point(x, y));
            }

            while (searchedCells.Count > 0)
            {
                var (distance, x, y) = searchedCells.Dequeue();
                if (gridDictionary.ContainsKey(x) && gridDictionary[x].ContainsKey(y))
                {
                    Enqueue(distance - 1, x + 1, y);
                    Enqueue(distance - 1, x - 1, y);
                    Enqueue(distance - 1, x, y + 1);
                    Enqueue(distance - 1, x, y - 1);
                }
            }

            return visitedCells;
        }
    }
}
