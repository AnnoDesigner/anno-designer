using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Windows;
using AnnoDesigner.Helper;

namespace AnnoDesigner.Tests
{
    public class CoordinateHelperTests
    {
        #region Rotate tests

        [Theory]
        [InlineData(3, 4)]
        [InlineData(4, 3)]
        [InlineData(5, 8)]
        [InlineData(4, 4)]
        public void Rotate_ShouldReturnCorrectResult(double width, double height)
        {
            // Arrange
            var helper = new CoordinateHelper();
            var inputSize = new Size(width, height);
            var expectedSize = new Size(height, width);

            // Act
            var result = helper.Rotate(inputSize);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        #endregion

        #region GetCenterPoint tests

        [Theory]
        [InlineData(384, 264, 60, 72, 414, 300)]
        [InlineData(384, 96, 60, 72, 414, 132)]
        [InlineData(408, 264, 60, 72, 438, 300)]
        [InlineData(396, 264, 60, 72, 426, 300)]
        [InlineData(420, 264, 60, 72, 450, 300)]
        [InlineData(444, 264, 60, 72, 474, 300)]
        public void GetCenterPoint_ShouldReturnCorrectResult(double x, double y, double width, double height, double expectedX, double expectedY)
        {
            // Arrange
            var helper = new CoordinateHelper();
            var inputRect = new Rect(x, y, width, height);
            var expectedPoint = new Point(expectedX, expectedY);

            // Act
            var result = helper.GetCenterPoint(inputRect);

            // Assert
            Assert.Equal(expectedPoint, result);
        }

        #endregion

        #region ScreenToGrid tests

        [Theory]
        [InlineData(12, 20, 0.6)]
        [InlineData(11, 20, 0.55)]
        [InlineData(170, 20, 8.5)]
        [InlineData(-20, 20, -1)]
        [InlineData(16, 20, 0.8)]
        [InlineData(85, 20, 4.25)]
        [InlineData(-39, 20, -1.95)]
        [InlineData(-21, 21, -1)]
        [InlineData(3, 15, 0.2)]
        [InlineData(0, 15, 0)]
        [InlineData(-9, 15, -0.6)]
        [InlineData(9, 15, 0.6)]
        [InlineData(-12, 15, -0.8)]
        [InlineData(-89, 16, -5.5625)]
        [InlineData(-27, 18, -1.5)]
        [InlineData(-77, 22, -3.5)]
        [InlineData(-66, 22, -3)]
        [InlineData(7, 8, 0.875)]
        [InlineData(-5, 8, -0.625)]
        [InlineData(4, 8, 0.5)]
        [InlineData(49, 8, 6.125)]
        [InlineData(-37, 8, -4.625)]
        [InlineData(-9, 12, -0.75)]
        [InlineData(-3, 12, -0.25)]
        [InlineData(-12, 12, -1)]
        public void ScreenToGrid_Double_ShouldReturnCorrectResult(double screenLength, int gridStep, double expectedResult)
        {
            // Arrange
            var helper = new CoordinateHelper();

            // Act
            var result = helper.ScreenToGrid(screenLength, gridStep);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        #endregion

        #region GridToScreen tests

        [Theory]
        [InlineData(8, 20, 160)]
        [InlineData(-1, 20, -20)]
        [InlineData(2, 20, 40)]
        [InlineData(0, 20, 0)]
        [InlineData(20, 21, 420)]
        [InlineData(20, 19, 380)]
        [InlineData(20, 18, 360)]
        [InlineData(20, 17, 340)]
        [InlineData(20, 15, 300)]
        [InlineData(1, 15, 15)]
        [InlineData(0, 0, 0)]
        [InlineData(-1, -1, 1)]
        [InlineData(1, 1, 1)]
        public void GridToScreen_Double_ShouldReturnCorrectResult(double gridLength, int gridStep, double expectedResult)
        {
            // Arrange
            var helper = new CoordinateHelper();

            // Act
            var result = helper.GridToScreen(gridLength, gridStep);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(3, 4, 20, 60, 80)]
        [InlineData(1, 1, 20, 20, 20)]
        [InlineData(5, 5, 20, 100, 100)]
        [InlineData(3, 3, 20, 60, 60)]
        [InlineData(6, 5, 21, 126, 105)]
        [InlineData(5, 6, 21, 105, 126)]
        [InlineData(5, 6, 19, 95, 114)]
        [InlineData(5, 5, 19, 95, 95)]
        [InlineData(6, 5, 19, 114, 95)]
        [InlineData(5, 6, 18, 90, 108)]
        [InlineData(6, 5, 18, 108, 90)]
        [InlineData(6, 5, 22, 132, 110)]
        [InlineData(5, 6, 22, 110, 132)]
        [InlineData(5, 6, 8, 40, 48)]
        [InlineData(5, 5, 8, 40, 40)]
        public void GridToScreen_Size_ShouldReturnCorrectResult(double width, double height, int gridStep, double expectedWidth, double expectedHeight)
        {
            // Arrange
            var helper = new CoordinateHelper();
            var inputSize = new Size(width, height);
            var expectedSize = new Size(expectedWidth, expectedHeight);

            // Act
            var result = helper.GridToScreen(inputSize, gridStep);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        [Theory]
        [InlineData(7, 2, 20, 140, 40)]
        [InlineData(5, 13, 20, 100, 260)]
        [InlineData(8, 20, 20, 160, 400)]
        [InlineData(2, 26, 20, 40, 520)]
        [InlineData(18, 26, 20, 360, 520)]
        [InlineData(11, 14, 20, 220, 280)]
        [InlineData(25, 12, 21, 525, 252)]
        [InlineData(30, 10, 21, 630, 210)]
        [InlineData(19, 13, 21, 399, 273)]
        [InlineData(28, 21, 21, 588, 441)]
        [InlineData(19, 13, 15, 285, 195)]
        [InlineData(13, 19, 15, 195, 285)]
        [InlineData(30, 20, 15, 450, 300)]
        [InlineData(24, 14, 13, 312, 182)]
        [InlineData(19, 13, 13, 247, 169)]
        [InlineData(25, 8, 8, 200, 64)]
        [InlineData(19, 13, 8, 152, 104)]
        public void GridToScreen_Point_ShouldReturnCorrectResult(double x, double y, int gridStep, double expectedX, double expectedY)
        {
            // Arrange
            var helper = new CoordinateHelper();
            var inputPoint = new Point(x, y);
            var expectedPoint = new Point(expectedX, expectedY);

            // Act
            var result = helper.GridToScreen(inputPoint, gridStep);

            // Assert
            Assert.Equal(expectedPoint, result);
        }

        #endregion

        #region RoundScreenToGrid tests

        [Theory]
        [InlineData(465, 337, 20, 23, 17)]
        [InlineData(400, 356, 20, 20, 18)]
        [InlineData(746, 389, 20, 37, 19)]
        [InlineData(318, 200, 20, 16, 10)]
        [InlineData(361, 188, 20, 18, 9)]
        [InlineData(958.5, 372, 21, 46, 18)]
        [InlineData(923.5, 361, 21, 44, 17)]
        [InlineData(509, 239.5, 21, 24, 11)]
        [InlineData(262.5, 251, 15, 18, 17)]
        [InlineData(343.5, 301, 15, 23, 20)]
        public void RoundScreenToGrid_Point_ShouldReturnCorrectResult(double x, double y, int gridStep, double expectedX, double expectedY)
        {
            // Arrange
            var helper = new CoordinateHelper();
            var inputPoint = new Point(x, y);
            var expectedPoint = new Point(expectedX, expectedY);

            // Act
            var result = helper.RoundScreenToGrid(inputPoint, gridStep);

            // Assert
            Assert.Equal(expectedPoint, result);
        }

        #endregion
    }
}
