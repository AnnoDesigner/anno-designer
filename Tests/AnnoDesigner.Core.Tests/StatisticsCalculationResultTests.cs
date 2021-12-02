using System;
using AnnoDesigner.Core.Layout.Models;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class StatisticsCalculationResultTests
    {
        #region IEquatable tests

        [Fact]
        public void Implements_Interface_IEquatable()
        {
            Assert.True(typeof(IEquatable<StatisticsCalculationResult>).IsAssignableFrom(typeof(StatisticsCalculationResult)));
        }

        [Fact]
        public void Equals_IsEqual()
        {
            var result1 = new StatisticsCalculationResult(42, 42, 45, 45, 3, 3, 9, 9, 100);
            var result2 = new StatisticsCalculationResult(42, 42, 45, 45, 3, 3, 9, 9, 100);

            Assert.True(result1.Equals(result2));
            Assert.True(result1.Equals((object)result2));
            Assert.True(result1.Equals(result1));
        }

        [Fact]
        public void Equals_IsNotEqual()
        {
            var result1 = new StatisticsCalculationResult(42, 42, 45, 45, 3, 3, 9, 9, 100);
            var result2 = new StatisticsCalculationResult(21, 21, 45, 45, 3, 3, 9, 9, 100);

            Assert.False(result1.Equals(result2));
            Assert.False(result1.Equals((object)result2));
            Assert.False(result1.Equals(null));
        }

        [Fact]
        public void GetHashCode_IsEqual()
        {
            var result1 = new StatisticsCalculationResult(42, 42, 45, 45, 3, 3, 9, 9, 100);
            var result2 = new StatisticsCalculationResult(42, 42, 45, 45, 3, 3, 9, 9, 100);

            Assert.Equal(result1.GetHashCode(), result2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_IsNotEqual()
        {
            var result1 = new StatisticsCalculationResult(42, 42, 45, 45, 3, 3, 9, 9, 100);
            var result2 = new StatisticsCalculationResult(21, 21, 45, 45, 3, 3, 9, 9, 100);

            Assert.NotEqual(result1.GetHashCode(), result2.GetHashCode());
        }

        #endregion
    }
}
