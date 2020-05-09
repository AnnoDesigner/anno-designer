using System;
using System.Collections.Generic;
using System.Linq;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Helper
{
    public static class RoadSearchHelper
    {
        private static void DoNothing(AnnoObject objectInRange) { }

        public static Dictionary<int, Dictionary<int, AnnoObject>> PrepareGridDictionary(IEnumerable<AnnoObject> placedObjects)
        {
            var dictionary = new Dictionary<int, Dictionary<int, AnnoObject>>();

            foreach (var placedObject in placedObjects)
            {
                var x = (int)placedObject.Position.X;
                var y = (int)placedObject.Position.Y;
                for (var i = 0; i < placedObject.Size.Width; i++)
                {
                    if (!dictionary.ContainsKey(x + i))
                    {
                        dictionary.Add(x + i, new Dictionary<int, AnnoObject>());
                    }

                    for (var j = 0; j < placedObject.Size.Height; j++)
                    {
                        dictionary[x + i][y + j] = placedObject;
                    }
                }
            }

            return dictionary;
        }

        public static HashSet<(int x, int y)> BreathFirstSearch(
            IEnumerable<AnnoObject> placedObjects,
            IEnumerable<AnnoObject> startObjects,
            Func<AnnoObject, int> rangeGetter,
            Action<AnnoObject> inRangeAction = null,
            Dictionary<int, Dictionary<int, AnnoObject>> gridDictionary = null)
        {
            inRangeAction = inRangeAction ?? DoNothing;
            gridDictionary = gridDictionary ?? PrepareGridDictionary(placedObjects);
            var blackCells = new HashSet<(int x, int y)>();

            startObjects = startObjects.Where(o => rangeGetter(o) > 0.5).ToList();
            if (startObjects.Count() == 0)
                return blackCells;

            var searchedCells = new Queue<(int remainingDistance, int x, int y)>();
            var blackObjects = new HashSet<AnnoObject>();

            foreach (var startObject in startObjects)
            {
                for (var i = 0; i < startObject.Size.Width; i++)
                {
                    searchedCells.Enqueue((rangeGetter(startObject), i + (int)startObject.Position.X, (int)startObject.Position.Y - 1));
                    searchedCells.Enqueue((rangeGetter(startObject), i + (int)startObject.Position.X, (int)(startObject.Position.Y + startObject.Size.Height)));
                    blackCells.Add((i + (int)startObject.Position.X, (int)startObject.Position.Y - 1));
                    blackCells.Add((i + (int)startObject.Position.X, (int)(startObject.Position.Y + startObject.Size.Height)));
                }
                for (var i = 0; i < startObject.Size.Height; i++)
                {
                    searchedCells.Enqueue((rangeGetter(startObject), (int)startObject.Position.X - 1, i + (int)startObject.Position.Y));
                    searchedCells.Enqueue((rangeGetter(startObject), (int)(startObject.Position.X + startObject.Size.Width), i + (int)startObject.Position.Y));
                    blackCells.Add(((int)startObject.Position.X - 1, i + (int)startObject.Position.Y));
                    blackCells.Add(((int)(startObject.Position.X + startObject.Size.Width), i + (int)startObject.Position.Y));
                }

                blackObjects.Add(startObject);
                for (var i = 0; i < startObject.Size.Width; i++)
                    for (var j = 0; j < startObject.Size.Height; j++)
                        blackCells.Add(((int)startObject.Position.X + i, (int)startObject.Position.Y + j));
            }

            void Enqueue(int distance, int x, int y)
            {
                if (!blackCells.Contains((x, y)) && gridDictionary.ContainsKey(x) && gridDictionary[x].ContainsKey(y))
                {
                    var inRangeObject = gridDictionary[x][y];

                    searchedCells.Enqueue((distance, x, y));
                    if (!blackObjects.Contains(inRangeObject) && !inRangeObject.Road)
                    {
                        inRangeAction(inRangeObject);
                    }
                    blackObjects.Add(inRangeObject);
                }
                blackCells.Add((x, y));
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

            return blackCells;
        }
    }
}
