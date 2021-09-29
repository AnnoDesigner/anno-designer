using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Helper;
using AnnoDesigner.Models;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class LayoutObjectTests
    {
        private static readonly ICoordinateHelper coordinateHelper;

        static LayoutObjectTests()
        {
            coordinateHelper = new CoordinateHelper();
        }

        #region GridInfluenceRangeRect tests

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-0.001)]
        public void GridInfluenceRangeRect_InfluenceRangeIsZeroOrNegative_ShouldReturnEmptyRect(double influenceRangeToSet)
        {
            // Arrange
            var annoObject = new AnnoObject
            {
                InfluenceRange = influenceRangeToSet,
                Size = new Size(10, 10),
                Position = new Point(42, 42)
            };
            var layoutObject = new LayoutObject(annoObject, null, null, null);

            // Act
            var influenceRangeRect = layoutObject.GridInfluenceRangeRect;

            // Assert
            Assert.Equal(annoObject.Position, influenceRangeRect.Location);
            Assert.Equal(default(Size), influenceRangeRect.Size);
        }

        #endregion

        #region GetScreenRadius tests

        [Theory]
        [InlineData(5, 5, 100)]
        [InlineData(3, 3, 100)]
        [InlineData(5.5, 5.5, 100)]
        public void GetScreenRadius_SizeHeightAndWidthAreOdd_ShouldAdjustRadius(double widthToSet, double heightToSet, double expectedRadius)
        {
            // Arrange            
            var annoObject = new AnnoObject
            {
                Size = new Size(widthToSet, heightToSet),
                Radius = 10
            };
            var layoutObject = new LayoutObject(annoObject, coordinateHelper, null, null);

            // Act
            var screenRadius = layoutObject.GetScreenRadius(10);

            // Assert
            Assert.Equal(expectedRadius, screenRadius);
        }

        [Fact]
        public void GetScreenRadius_SizeHeightIsOdd_ShouldNotAdjustRadius()
        {
            // Arrange            
            var annoObject = new AnnoObject
            {
                Size = new Size(8, 5),
                Radius = 10
            };
            var layoutObject = new LayoutObject(annoObject, coordinateHelper, null, null);

            // Act
            var screenRadius = layoutObject.GetScreenRadius(10);

            // Assert
            Assert.Equal(100, screenRadius);
        }

        [Fact]
        public void GetScreenRadius_SizeWidthIsOdd_ShouldNotAdjustRadius()
        {
            // Arrange            
            var annoObject = new AnnoObject
            {
                Size = new Size(5, 8),
                Radius = 10
            };
            var layoutObject = new LayoutObject(annoObject, coordinateHelper, null, null);

            // Act
            var screenRadius = layoutObject.GetScreenRadius(10);

            // Assert
            Assert.Equal(100, screenRadius);
        }

        [Fact]
        public void GetScreenRadius_NeitherSizeWidthNorHeightIsOdd_ShouldNotAdjustRadius()
        {
            // Arrange            
            var annoObject = new AnnoObject
            {
                Size = new Size(8, 8),
                Radius = 10
            };
            var layoutObject = new LayoutObject(annoObject, coordinateHelper, null, null);

            // Act
            var screenRadius = layoutObject.GetScreenRadius(10);

            // Assert
            Assert.Equal(100, screenRadius);
        }

        #endregion
    }
}
