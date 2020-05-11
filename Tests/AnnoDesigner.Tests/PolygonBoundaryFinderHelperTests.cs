using System.Collections.Generic;
using System.Windows;
using AnnoDesigner.Helper;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class PolygonBoundaryFinderHelperTests
    {
        [Fact]
        public void GetBoundaryPoints_SimpleShape()
        {
            // Arrange
            var insidePoints = new HashSet<Point>()
            {
                new Point(1, 1)
            };

            // Act
            var boundary = PolygonBoundaryFinderHelper.GetBoundaryPoints(insidePoints);

            // Assert
            Assert.Equal(boundary, new Point[]
            {
                new Point(1, 1),
                new Point(1, 2),
                new Point(2, 2),
                new Point(2, 1)
            });
        }

        [Fact]
        public void GetBoundaryPoints_ComplexShape()
        {
            // Arrange
            var insidePoints = new HashSet<Point>()
            {
                new Point(2, 1),
                new Point(1, 2),
                new Point(2, 2),
                new Point(3, 2),
                new Point(2, 3)
            };

            // Act
            var boundary = PolygonBoundaryFinderHelper.GetBoundaryPoints(insidePoints);

            // Assert
            Assert.Equal(boundary, new Point[]
            {
                new Point(1, 2),
                new Point(1, 3),
                new Point(2, 3),
                new Point(2, 4),
                new Point(3, 4),
                new Point(3, 3),
                new Point(4, 3),
                new Point(4, 2),
                new Point(3, 2),
                new Point(3, 1),
                new Point(2, 1),
                new Point(2, 2)
            });
        }

        [Fact]
        public void GetBoundaryPoints_ContinuousSidesAreMerged()
        {
            // Arrange
            var insidePoints = new HashSet<Point>()
            {
                new Point(1, 1),
                new Point(2, 1),
                new Point(1, 2),
                new Point(2, 2)
            };

            // Act
            var boundary = PolygonBoundaryFinderHelper.GetBoundaryPoints(insidePoints);

            // Assert
            Assert.Equal(boundary, new Point[]
            {
                new Point(1, 1),
                new Point(1, 3),
                new Point(3, 3),
                new Point(3, 1)
            });
        }
    }
}
