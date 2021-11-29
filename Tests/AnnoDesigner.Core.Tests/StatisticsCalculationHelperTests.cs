using System.Collections.Generic;
using AnnoDesigner.Core.Layout.Helper;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class StatisticsCalculationHelperTests
    {
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

        #endregion
    }
}
