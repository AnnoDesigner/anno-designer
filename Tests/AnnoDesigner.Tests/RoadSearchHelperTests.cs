using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Helper;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class RoadSearchHelperTests
    {
        private string GetTestDataFile(string testCase)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            return Path.Combine(basePath, "TestData", "RoadSearchHelper", $"{testCase}.ad");
        }

        private void AssertGridDictionary(Dictionary<double, Dictionary<double, AnnoObject>> gridDictionary, IDictionary<double, IDictionary<double, AnnoObject>> expected)
        {
            Assert.Equal(gridDictionary.Count, expected.Count);
            foreach (var column in gridDictionary)
            {
                Assert.Contains(column.Key, expected);
                Assert.Equal(column.Value.Count, expected[column.Key].Count);

                foreach (var item in column.Value)
                {
                    Assert.Contains(item.Key, expected[column.Key]);
                    Assert.Equal(item.Value, expected[column.Key][item.Key]);
                }
            }
        }

        [Fact]
        public void PrepareGridDictionary_SingleObject()
        {
            // Arrange
            var placedObjects = new LayoutLoader().LoadLayout(GetTestDataFile("PrepareGridDictionary_SingleObject"), true);
            var expectedResult = new Dictionary<double, IDictionary<double, AnnoObject>>()
            {
                [1] = new Dictionary<double, AnnoObject>()
                {
                    [1] = placedObjects[0],
                    [2] = placedObjects[0],
                    [3] = placedObjects[0]
                },
                [2] = new Dictionary<double, AnnoObject>()
                {
                    [1] = placedObjects[0],
                    [2] = placedObjects[0],
                    [3] = placedObjects[0]
                }
            };

            // Act
            var gridDictionary = RoadSearchHelper.PrepareGridDictionary(placedObjects);

            // Assert
            AssertGridDictionary(gridDictionary, expectedResult);
        }

        [Fact]
        public void PrepareGridDictionary_MultipleObjects()
        {
            // Arrange
            var placedObjects = new LayoutLoader().LoadLayout(GetTestDataFile("PrepareGridDictionary_MultipleObjects"), true);
            var placedObject1 = placedObjects.FirstOrDefault(o => o.Position == new Point(1, 1));
            var placedObject2 = placedObjects.FirstOrDefault(o => o.Position == new Point(5, 2));
            var expectedResult = new Dictionary<double, IDictionary<double, AnnoObject>>()
            {
                [1] = new Dictionary<double, AnnoObject>()
                {
                    [1] = placedObject1,
                    [2] = placedObject1,
                    [3] = placedObject1
                },
                [2] = new Dictionary<double, AnnoObject>()
                {
                    [1] = placedObject1,
                    [2] = placedObject1,
                    [3] = placedObject1
                },
                [5] = new Dictionary<double, AnnoObject>()
                {
                    [2] = placedObject2,
                    [3] = placedObject2
                }
            };

            // Act
            var gridDictionary = RoadSearchHelper.PrepareGridDictionary(placedObjects);

            // Assert
            AssertGridDictionary(gridDictionary, expectedResult);
        }

        [Fact]
        public void BreadthFirstSearch_FindObjectsInInfluenceRange()
        {
            // Arrange
            var placedObjects = new LayoutLoader().LoadLayout(GetTestDataFile("BreadthFirstSearch_FindObjectsInInfluenceRange"), true);
            var startObjects = placedObjects.Where(o => o.Label == "Start").ToList();

            // Act
            var objectsInInfluence = new List<AnnoObject>();
            RoadSearchHelper.BreadthFirstSearch(placedObjects, startObjects, o => o.InfluenceRange + 1, o => objectsInInfluence.Add(o));

            // Assert
            Assert.Equal(objectsInInfluence, placedObjects.Where(o => o.Label == "TargetIn"));
            Assert.True(placedObjects.Where(o => o.Label == "TargetOut").All(o => !objectsInInfluence.Contains(o)));
        }

        [Fact]
        public void BreadthFirstSearch_FindBuildingInfluenceRange()
        {
            // Arrange
            var placedObjects = new LayoutLoader().LoadLayout(GetTestDataFile("BreadthFirstSearch_FindBuildingInfluenceRange"), true);
            var startObjects = placedObjects.Where(o => o.Label == "Start").ToList();
            foreach (var startObject in startObjects)
            {
                var expectedCount = 4 * Enumerable.Range(1, (int)startObject.InfluenceRange).Sum() + 1;

                // Act
                var visitedCells = RoadSearchHelper.BreadthFirstSearch(placedObjects, new[] { startObject }, o => o.InfluenceRange);

                // Assert
                Assert.Equal(expectedCount, visitedCells.Count);
            }
        }
    }
}
