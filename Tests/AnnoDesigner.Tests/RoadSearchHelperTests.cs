using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Helper;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class RoadSearchHelperTests
    {
        private static readonly LayoutFile defaultObjectList;

        static RoadSearchHelperTests()
        {
            defaultObjectList = new LayoutLoader().LoadLayout(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "RoadSearchHelper", "BreadthFirstSearch_FindBuildingInfluenceRange.ad"), true);
        }

        private string GetTestDataFile(string testCase)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "RoadSearchHelper", $"{testCase}.ad");
        }

        [Fact]
        public void PrepareGridDictionary_SequenceIsNull_ShouldReturnNull_Issue197()
        {
            // Arrange/Act
            var gridDictionary = RoadSearchHelper.PrepareGridDictionary(null);

            // Assert
            Assert.Null(gridDictionary);
        }

        [Fact]
        public void PrepareGridDictionary_SequenceIsEmpty_ShouldReturnNull_Issue197()
        {
            // Arrange
            var inputSequence = Enumerable.Empty<AnnoObject>();

            // Act
            var gridDictionary = RoadSearchHelper.PrepareGridDictionary(inputSequence);

            // Assert
            Assert.Null(gridDictionary);
        }

        [Fact]
        public void PrepareGridDictionary_SingleObject()
        {
            // Arrange
            var placedObjects = new LayoutLoader().LoadLayout(GetTestDataFile("PrepareGridDictionary_SingleObject"), true).Objects;
            var expectedResult = new AnnoObject[][]
            {
                new AnnoObject[5],
                new AnnoObject[]
                {
                    null,
                    placedObjects[0],
                    placedObjects[0],
                    placedObjects[0],
                    null
                },
                new AnnoObject[]
                {
                    null,
                    placedObjects[0],
                    placedObjects[0],
                    placedObjects[0],
                    null
                },
                new AnnoObject[5]
            };

            // Act
            var gridDictionary = RoadSearchHelper.PrepareGridDictionary(placedObjects);

            // Assert
            Assert.Equal(expectedResult, gridDictionary);
            Assert.Equal(0, gridDictionary.Offset.x);
            Assert.Equal(0, gridDictionary.Offset.y);
        }

        [Fact]
        public void PrepareGridDictionary_MultipleObjects()
        {
            // Arrange
            var placedObjects = new LayoutLoader().LoadLayout(GetTestDataFile("PrepareGridDictionary_MultipleObjects"), true).Objects;
            var placedObject1 = placedObjects.FirstOrDefault(o => o.Label == "Object1");
            var placedObject2 = placedObjects.FirstOrDefault(o => o.Label == "Object2");
            var expectedResult = new AnnoObject[][]
            {
                new AnnoObject[5],
                new AnnoObject[]
                {
                    null,
                    placedObject1,
                    placedObject1,
                    placedObject1,
                    null
                },
                new AnnoObject[]
                {
                    null,
                    placedObject1,
                    placedObject1,
                    placedObject1,
                    null
                },
                new AnnoObject[5],
                new AnnoObject[5],
                new AnnoObject[]
                {
                    null,
                    null,
                    placedObject2,
                    placedObject2,
                    null
                },
                new AnnoObject[5]
            };

            // Act
            var gridDictionary = RoadSearchHelper.PrepareGridDictionary(placedObjects);

            // Assert
            Assert.Equal(expectedResult, gridDictionary);
            Assert.Equal(0, gridDictionary.Offset.x);
            Assert.Equal(0, gridDictionary.Offset.y);
        }

        [Fact]
        public void PrepareGridDictionary_MultipleObjectsWithNegativeCoordinates()
        {
            // Arrange
            var placedObjects = new LayoutLoader().LoadLayout(GetTestDataFile("PrepareGridDictionary_MultipleObjectsWithNegativeCoordinates"), true).Objects;

            // Act
            var gridDictionary = RoadSearchHelper.PrepareGridDictionary(placedObjects);

            // Assert
            Assert.Equal(-10, gridDictionary.Offset.x);
            Assert.Equal(-5, gridDictionary.Offset.y);
        }

        [Fact]
        public void BreadthFirstSearch_FindObjectsInInfluenceRange()
        {
            // Arrange
            var placedObjects = new LayoutLoader().LoadLayout(GetTestDataFile("BreadthFirstSearch_FindObjectsInInfluenceRange"), true).Objects;
            var startObjects = placedObjects.Where(o => o.Label == "Start").ToList();

            // Act
            var objectsInInfluence = new List<AnnoObject>();
            RoadSearchHelper.BreadthFirstSearch(placedObjects, startObjects, o => (int)o.InfluenceRange + 1, inRangeAction: o => objectsInInfluence.Add(o));

            // Assert
            Assert.Equal(placedObjects.Where(o => o.Label == "TargetIn").ToHashSet(), objectsInInfluence.ToHashSet());
            Assert.True(placedObjects.Where(o => o.Label == "TargetOut").All(o => !objectsInInfluence.Contains(o)));
        }

        [Fact]
        public void BreadthFirstSearch_FindBuildingInfluenceRange()
        {
            // Arrange
            var placedObjects = defaultObjectList.Objects;
            var startObjects = placedObjects.Where(o => o.Label == "Start").ToList();
            foreach (var startObject in startObjects)
            {
                var expectedCount = 4 * Enumerable.Range(1, (int)startObject.InfluenceRange).Sum() + 1;

                // Act
                var visitedCells = RoadSearchHelper.BreadthFirstSearch(placedObjects, new[] { startObject }, o => (int)o.InfluenceRange);

                // Assert
                Assert.Equal(expectedCount, visitedCells.Sum(c => c.Count(visited => visited)));
            }
        }

        [Fact]
        public void BreadthFirstSearch_StartObjectCountIsZero_ShouldReturnEmptyResult()
        {
            // Arrange
            var placedObjects = defaultObjectList.Objects;
            var startObjects = Enumerable.Empty<AnnoObject>();

            var expectedResult = new bool[0][];

            // Act
            var visitedCells = RoadSearchHelper.BreadthFirstSearch(placedObjects, startObjects, o => (int)o.InfluenceRange);

            // Assert
            Assert.Equal(expectedResult, visitedCells);

        }

        [Fact]
        public void BreadthFirstSearch_PlacedObjectsEmpty_ShouldReturnEmptyResult_Issue197()
        {
            // Arrange
            var placedObjects = Enumerable.Empty<AnnoObject>();
            var startObjects = defaultObjectList.Objects;

            var expectedResult = new bool[0][];

            // Act
            var visitedCells = RoadSearchHelper.BreadthFirstSearch(placedObjects, startObjects, o => (int)o.InfluenceRange);

            // Assert
            Assert.Equal(expectedResult, visitedCells);

        }

        [Fact]
        public void BreadthFirstSearch_PlacedObjectsNull_ShouldReturnEmptyResult_Issue197()
        {
            // Arrange
            IEnumerable<AnnoObject> placedObjects = null;
            var startObjects = defaultObjectList.Objects;

            var expectedResult = new bool[0][];

            // Act
            var visitedCells = RoadSearchHelper.BreadthFirstSearch(placedObjects, startObjects, o => (int)o.InfluenceRange);

            // Assert
            Assert.Equal(expectedResult, visitedCells);

        }
    }
}
