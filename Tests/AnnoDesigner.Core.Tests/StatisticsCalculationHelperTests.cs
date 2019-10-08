using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var result = new StatisticsCalculationResult();

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

            var result = new StatisticsCalculationResult();

            // Act
            var ex = Record.Exception(() => result = helper.CalculateStatistics(new List<AnnoObject>()));

            // Assert
            Assert.Null(ex);
            Assert.NotNull(result);
        }

        #endregion
    }
}
