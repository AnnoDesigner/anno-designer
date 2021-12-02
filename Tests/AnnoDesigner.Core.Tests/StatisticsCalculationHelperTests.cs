using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Layout.Helper;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class StatisticsCalculationHelperTests
    {
        #region testdata

        private static readonly string testData_layout_with_blocking_tiles;

        #endregion

        static StatisticsCalculationHelperTests()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            testData_layout_with_blocking_tiles = File.ReadAllText(Path.Combine(basePath, "Testdata", "StatisticsCalculation", "layout_with_block_tiles.ad"), Encoding.UTF8);
        }

        private StatisticsCalculationHelper GetHelper()
        {
            return new StatisticsCalculationHelper();
        }

        #region CalculateStatistics tests

        [Fact]
        public void CalculateStatistics_IsCalledWithNull_ShouldNotThrowAndReturnNull()
        {
            // Arrange            
            var helper = GetHelper();

            var result = StatisticsCalculationResult.Empty;

            // Act
            var ex = Record.Exception(() => result = helper.CalculateStatistics(null));

            // Assert
            Assert.Null(ex);
            Assert.Null(result);
        }

        [Fact]
        public void CalculateStatistics_IsCalledWithEmptyList_ShouldNotThrowAndNotReturnNull()
        {
            // Arrange            
            var helper = GetHelper();

            var result = StatisticsCalculationResult.Empty;

            // Act
            var ex = Record.Exception(() => result = helper.CalculateStatistics(new List<AnnoObject>()));

            // Assert
            Assert.Null(ex);
            Assert.NotNull(result);
        }

        [Fact]
        public void CalculateStatistics_IsCalledWithOnlyBlockTiles_ShouldReturnEmptyResult()
        {
            // Arrange            
            var helper = GetHelper();

            var objects = new List<AnnoObject>
            {
                new AnnoObject
                {
                    Template = "Blocker"
                }
            };

            // Act
            var result = helper.CalculateStatistics(objects);

            // Assert
            Assert.Equal(StatisticsCalculationResult.Empty, result);
        }

        [Fact]
        public void CalculateStatistics_IsCalledWithAlsoBlockTiles_ShouldIgnoreBlockTilesInCalculation()
        {
            // Arrange            
            var helper = GetHelper();

            var expected = new StatisticsCalculationResult(minX: 42, minY: 42, maxX: 45, maxY: 45, usedAreaWidth: 3, usedAreaHeight: 3, usedTiles: 9, minTiles: 9, efficiency: 100);

            var objects = new List<AnnoObject>
            {
                new AnnoObject
                {
                    Template = "Blocker"
                },
                new AnnoObject
                {
                    Template = "Dummy",
                    Position = new System.Windows.Point(42,42),
                    Size = new System.Windows.Size(3,3),
                    Road = false
                }
            };

            // Act
            var result = helper.CalculateStatistics(objects);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateStatistics_IsCalledWithKnownLayout_IgnoreRoadsAndBlockTiles_ShouldReturnCorrectResult()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();
            using var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(testData_layout_with_blocking_tiles));
            var loadedLayout = loader.LoadLayout(streamWithLayout, true);

            var helper = GetHelper();

            var expected = new StatisticsCalculationResult(minX: 3, minY: 3, maxX: 36, maxY: 31, usedAreaWidth: 33, usedAreaHeight: 28, usedTiles: 924, minTiles: 736, efficiency: 80);

            // Act
            var result = helper.CalculateStatistics(loadedLayout.Objects);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateStatistics_IsCalledWithKnownLayout_IgnoreRoadsButNotBlockTiles_ShouldReturnCorrectResult()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();
            using var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(testData_layout_with_blocking_tiles));
            var loadedLayout = loader.LoadLayout(streamWithLayout, true);

            var helper = GetHelper();

            var expected = new StatisticsCalculationResult(minX: 1, minY: 1, maxX: 38, maxY: 33, usedAreaWidth: 37, usedAreaHeight: 32, usedTiles: 1184, minTiles: 870, efficiency: 73);

            // Act
            var result = helper.CalculateStatistics(loadedLayout.Objects, includeIgnoredObjects: true);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateStatistics_IsCalledWithKnownLayout_IgnoreBlockTilesButNotRoads_ShouldReturnCorrectResult()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();
            using var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(testData_layout_with_blocking_tiles));
            var loadedLayout = loader.LoadLayout(streamWithLayout, true);

            var helper = GetHelper();

            var expected = new StatisticsCalculationResult(minX: 3, minY: 3, maxX: 36, maxY: 31, usedAreaWidth: 33, usedAreaHeight: 28, usedTiles: 924, minTiles: 923, efficiency: 100);

            // Act
            var result = helper.CalculateStatistics(loadedLayout.Objects, includeRoads: true);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateStatistics_IsCalledWithKnownLayout_IgnoreNothing_ShouldReturnCorrectResult()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();
            using var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(testData_layout_with_blocking_tiles));
            var loadedLayout = loader.LoadLayout(streamWithLayout, true);

            var helper = GetHelper();

            var expected = new StatisticsCalculationResult(minX: 1, minY: 1, maxX: 38, maxY: 33, usedAreaWidth: 37, usedAreaHeight: 32, usedTiles: 1184, minTiles: 1057, efficiency: 89);

            // Act
            var result = helper.CalculateStatistics(loadedLayout.Objects, includeRoads: true, includeIgnoredObjects: true);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion
    }
}
