using System;
using System.Collections.Generic;
using System.Linq;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Models;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void WithoutIgnoredObjects_ListIsNull_ShouldReturnNull()
        {
            // Arrange/Act
            var result = IEnumerableExtensions.WithoutIgnoredObjects(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void WithoutIgnoredObjects_ListIsEmpty_ShouldReturnEmptyList()
        {
            // Arrange/Act
            var result = IEnumerableExtensions.WithoutIgnoredObjects(new List<AnnoObject>());

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void WithoutIgnoredObjects_ListHasNoIgnorableObjects_ShouldReturnSameList()
        {
            // Arrange
            var list = new List<AnnoObject>
            {
                new AnnoObject
                {
                    Template = "Dummy"
                },
                new AnnoObject
                {
                    Template = "AnotherDummy"
                }
            };

            // Act
            var result = IEnumerableExtensions.WithoutIgnoredObjects(list);

            // Assert
            Assert.Equal(list.Count(), result.Count());
            Assert.All(result, x =>
            {
                Assert.Contains(x, list);
            });
        }

        [Fact]
        public void WithoutIgnoredObjects_ListHasIgnorableObjects_ShouldReturnFilteredList()
        {
            // Arrange
            var list = new List<AnnoObject>
            {
                new AnnoObject
                {
                    Template = "Blocker"
                },
                new AnnoObject
                {
                    Template = "Dummy"
                },
                new AnnoObject
                {
                    Template = "AnotherDummy"
                }
            };

            // Act
            var result = IEnumerableExtensions.WithoutIgnoredObjects(list);

            // Assert
            Assert.NotEqual(list.Count(), result.Count());
            Assert.All(result, x =>
            {
                Assert.NotEqual("Blocker", x.Template, StringComparer.OrdinalIgnoreCase);
            });
        }

        [Fact]
        public void Range_Increasing_ShouldReturnIncreasingSequence()
        {
            // Arrange/Act
            var result = IEnumerableExtensions.Range(1, 5).ToList();

            // Assert
            Assert.Equal(new double[] { 1, 2, 3, 4 }, result);
        }

        [Fact]
        public void Range_Decreasing_ShouldReturnDecreasingSequence()
        {
            // Arrange/Act
            var result = IEnumerableExtensions.Range(5, 1).ToList();

            // Assert
            Assert.Equal(new double[] { 5, 4, 3, 2 }, result);
        }

        [Fact]
        public void Range_NonDefaultStep_ShouldSkipSomeValues()
        {
            // Arrange/Act
            var result = IEnumerableExtensions.Range(1, 5, 2).ToList();

            // Assert
            Assert.Equal(new double[] { 1, 3 }, result);
        }

        [Fact]
        public void Range_ZeroStep_ShouldThrowException()
        {
            // Arrange/Act/Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => IEnumerableExtensions.Range(1, 5, 0).ToList());
        }

        [Fact]
        public void Range_NegativeStep_ShouldThrowException()
        {
            // Arrange/Act/Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => IEnumerableExtensions.Range(5, 1, -1).ToList());
        }

        [Fact]
        public void Range_InclusiveTo_ShouldReturnOneValueAfterEnd()
        {
            // Arrange/Act
            var result = IEnumerableExtensions.Range(1, 5, inclusiveTo: true).ToList();

            // Assert
            Assert.Equal(new double[] { 1, 2, 3, 4, 5 }, result);
        }
    }
}
