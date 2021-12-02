using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Extensions;
using AnnoDesigner.Models;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class IEnumerableExtensionsTests
    {
        private readonly ICoordinateHelper mockedCoordinateHelper;
        private readonly IBrushCache mockedBrushCache;
        private readonly IPenCache mockedPenCache;

        public IEnumerableExtensionsTests()
        {
            mockedCoordinateHelper = new Mock<ICoordinateHelper>().Object;
            mockedBrushCache = new Mock<IBrushCache>().Object;
            mockedPenCache = new Mock<IPenCache>().Object;
        }

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
            var result = IEnumerableExtensions.WithoutIgnoredObjects(new List<LayoutObject>());

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void WithoutIgnoredObjects_ListHasNoIgnorableObjects_ShouldReturnSameList()
        {
            // Arrange
            var list = new List<LayoutObject>
            {
                new LayoutObject(
                    new AnnoObject
                    {
                        Template = "Dummy"
                    } ,mockedCoordinateHelper, mockedBrushCache, mockedPenCache),
                new LayoutObject(
                    new AnnoObject
                    {
                        Template = "AnotherDummy"
                    } ,mockedCoordinateHelper, mockedBrushCache, mockedPenCache),
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
            var list = new List<LayoutObject>
            {
                new LayoutObject(
                    new AnnoObject
                    {
                        Template = "Blocker"
                    } ,mockedCoordinateHelper, mockedBrushCache, mockedPenCache),
                new LayoutObject(
                    new AnnoObject
                    {
                        Template = "Dummy"
                    } ,mockedCoordinateHelper, mockedBrushCache, mockedPenCache),
                new LayoutObject(
                    new AnnoObject
                    {
                        Template = "AnotherDummy"
                    } ,mockedCoordinateHelper, mockedBrushCache, mockedPenCache),
            };

            // Act
            var result = IEnumerableExtensions.WithoutIgnoredObjects(list);

            // Assert
            Assert.NotEqual(list.Count(), result.Count());
            Assert.All(result, x =>
            {
                Assert.NotEqual("Blocker", x.WrappedAnnoObject.Template, StringComparer.OrdinalIgnoreCase);
            });
        }
    }
}
