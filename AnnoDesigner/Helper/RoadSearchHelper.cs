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

        public static Dictionary<double, Dictionary<double, AnnoObject>> PrepareGridDictionary(IEnumerable<AnnoObject> placedObjects)
        {
            var dictionary = new Dictionary<double, Dictionary<double, AnnoObject>>();

            foreach (var placedObject in placedObjects)
            {
                var x = placedObject.Position.X;
                var y = placedObject.Position.Y;
                for (var i = 0; i < placedObject.Size.Width; i++)
                {
                    if (!dictionary.ContainsKey(x + i))
                    {
                        dictionary.Add(x + i, new Dictionary<double, AnnoObject>());
                    }

                    for (var j = 0; j < placedObject.Size.Height; j++)
                    {
                        dictionary[x + i][y + j] = placedObject;
                    }
                }
            }

            return dictionary;
        }

        public static HashSet<Point> BreadthFirstSearch(
            IEnumerable<AnnoObject> placedObjects,
            IEnumerable<AnnoObject> startObjects,
            Func<AnnoObject, double> rangeGetter,
            Action<AnnoObject> inRangeAction = null,
            Dictionary<double, Dictionary<double, AnnoObject>> gridDictionary = null)
        {
            inRangeAction = inRangeAction ?? DoNothing;
            gridDictionary = gridDictionary ?? PrepareGridDictionary(placedObjects);
            var visitedCells = new HashSet<Point>();

            startObjects = startObjects.Where(o => rangeGetter(o) > 0.5).ToList();
            if (startObjects.Count() == 0)
                return visitedCells;

            var searchedCells = new Queue<(double remainingDistance, double x, double y)>();
            var visitedObjects = new HashSet<AnnoObject>();

            foreach (var startObject in startObjects)
            {
                for (var i = 0; i < startObject.Size.Width; i++)
                {
                    searchedCells.Enqueue((rangeGetter(startObject), i + startObject.Position.X, startObject.Position.Y - 1));
                    searchedCells.Enqueue((rangeGetter(startObject), i + startObject.Position.X, (startObject.Position.Y + startObject.Size.Height)));
                    visitedCells.Add(new Point(i + startObject.Position.X, startObject.Position.Y - 1));
                    visitedCells.Add(new Point(i + startObject.Position.X, (startObject.Position.Y + startObject.Size.Height)));
                }
                for (var i = 0; i < startObject.Size.Height; i++)
                {
                    searchedCells.Enqueue((rangeGetter(startObject), startObject.Position.X - 1, i + startObject.Position.Y));
                    searchedCells.Enqueue((rangeGetter(startObject), (startObject.Position.X + startObject.Size.Width), i + startObject.Position.Y));
                    visitedCells.Add(new Point(startObject.Position.X - 1, i + startObject.Position.Y));
                    visitedCells.Add(new Point(startObject.Position.X + startObject.Size.Width, i + startObject.Position.Y));
                }

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

                    searchedCells.Enqueue((distance, x, y));
                    if (!visitedObjects.Contains(inRangeObject) && !inRangeObject.Road)
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
                if (
                    distance > 0 &&
                    gridDictionary.ContainsKey(x) &&
                    gridDictionary[x].ContainsKey(y) &&
                    gridDictionary[x][y].Road)
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
