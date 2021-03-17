using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AnnoDesigner.Helper;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class PolygonBoundaryFinderHelperTests
    {
        /// <summary>
        /// Returns grid representation of string lines.
        /// Character "X" is used to represent grid cell being occupied. Any other character means empty cell.
        /// Start of coordinates is in top left corner, width is horizontal and height is vertical.
        /// </summary>
        /// <example>
        /// "XX  ",
        /// "XX  ",
        /// "XXXX",
        /// "XXXX"
        /// </example>
        public static bool[][] ParseGrid(params string[] gridLines)
        {
            var preTranspose = gridLines.Select(line => line.Select(c => c == 'X').ToArray()).ToArray();
            var postTranspose = Enumerable.Range(0, gridLines.Max(i => i.Length)).Select(i => new bool[gridLines.Length]).ToArray();

            for (var i = 0; i < gridLines.Length; i++)
                for (var j = 0; j < gridLines[i].Length; j++)
                    postTranspose[j][i] = preTranspose[i][j];

            return postTranspose;
        }

        [Fact]
        public void GetBoundaryPoints_SimpleShape()
        {
            // Arrange
            var insidePoints = ParseGrid(
                "  ",
                " X");

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
            var insidePoints = ParseGrid(
                "    ",
                "  X ",
                " XXX",
                "  X ");

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
            var insidePoints = ParseGrid(
                "   ",
                " XX",
                " XX");

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

        [Fact]
        public void GetBoundaryPointsWithHoles_HolesAreFound()
        {
            // Arrange
            var insidePoints = ParseGrid(
                "XXXXXX",
                "X X  X",
                "X XXXX",
                "X X  X",
                "X X  X",
                "XXXXXX");

            // Act
            var boundaries = PolygonBoundaryFinderHelper.GetBoundaryPointsWithHoles(insidePoints);

            // Assert
            Assert.Equal(boundaries, new (int, int)[][]
            {
                new (int, int)[] // outer boundary
                {
                    (0, 0),
                    (0, 6),
                    (6, 6),
                    (6, 0)
                },
                new (int, int)[] // left hole
                {
                    (1, 1),
                    (2, 1),
                    (2, 5),
                    (1, 5)
                },
                new (int, int)[] // top right hole
                {
                    (3, 1),
                    (5, 1),
                    (5, 2),
                    (3, 2)
                },
                new (int, int)[] // bottom right hole
                {
                    (3, 3),
                    (5, 3),
                    (5, 5),
                    (3, 5)
                }
            });
        }

        [Fact]
        public void GetBoundaryPointsWithHoles_ComplexHoleIsFound()
        {
            // Arrange
            var insidePoints = ParseGrid(
                "XXXXXX",
                "X    X",
                "X XX X",
                "X X  X",
                "X X XX",
                "XXXXXX");

            // Act
            var boundaries = PolygonBoundaryFinderHelper.GetBoundaryPointsWithHoles(insidePoints);

            // Assert
            Assert.Equal(boundaries, new (int, int)[][]
            {
                new (int, int)[] // outer boundary
                {
                    (0, 0),
                    (0, 6),
                    (6, 6),
                    (6, 0)
                },
                new (int, int)[] // hole
                {
                    (1, 1),
                    (5, 1),
                    (5, 4),
                    (4, 4),
                    (4, 5),
                    (3, 5),
                    (3, 3),
                    (4, 3),
                    (4, 2),
                    (2, 2),
                    (2, 5),
                    (1, 5)
                }
            });
        }
    }
}
