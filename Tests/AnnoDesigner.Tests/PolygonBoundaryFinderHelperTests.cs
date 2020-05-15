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
            var insidePoints = new bool[][]
            {
                new bool[2],
                new bool[2] { false, true }
            };

            // Act
            var boundary = PolygonBoundaryFinderHelper.GetBoundaryPoints(insidePoints);

            // Assert
            Assert.Equal(boundary, new (int, int)[]
            {
                (1, 1),
                (1, 2),
                (2, 2),
                (2, 1)
            });
        }

        [Fact]
        public void GetBoundaryPoints_ComplexShape()
        {
            // Arrange
            var insidePoints = new bool[][]
            {
                new bool[4],
                new bool[4] { false, false, true, false },
                new bool[4] { false, true, true, true },
                new bool[4] { false, false, true, false }
            };

            // Act
            var boundary = PolygonBoundaryFinderHelper.GetBoundaryPoints(insidePoints);

            // Assert
            Assert.Equal(boundary, new (int, int)[]
            {
                (1, 2),
                (1, 3),
                (2, 3),
                (2, 4),
                (3, 4),
                (3, 3),
                (4, 3),
                (4, 2),
                (3, 2),
                (3, 1),
                (2, 1),
                (2, 2)
            });
        }

        [Fact]
        public void GetBoundaryPoints_ContinuousSidesAreMerged()
        {
            // Arrange
            var insidePoints = new bool[][]
            {
                new bool[3],
                new bool[3] { false, true, true },
                new bool[3] { false, true, true }
            };

            // Act
            var boundary = PolygonBoundaryFinderHelper.GetBoundaryPoints(insidePoints);

            // Assert
            Assert.Equal(boundary, new (int, int)[]
            {
                (1, 1),
                (1, 3),
                (3, 3),
                (3, 1)
            });
        }
    }
}
