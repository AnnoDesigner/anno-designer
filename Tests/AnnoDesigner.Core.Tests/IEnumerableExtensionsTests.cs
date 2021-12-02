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
    }
}
