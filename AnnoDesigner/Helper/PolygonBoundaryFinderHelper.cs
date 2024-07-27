using System.Collections.Generic;
using System.Linq;

namespace AnnoDesigner.Helper
{
    public static class PolygonBoundaryFinderHelper
    {
        public enum Direction
        {
            None = 0,
            Up = 1 << 0,
            Left = 1 << 1,
            Down = 1 << 2,
            Right = 1 << 3
        }

        /// <summary>
        /// Gets the starting point (corner of pixel) based on pixel position and side where the search should start.
        /// 
        /// Returns the top left corner (X) in the given direction (the entire box is one pixel).
        /// X---
        /// | ^ |
        /// | | |
        /// |   |
        ///  ---
        /// </summary>
        public static (int x, int y) GetStartPoint((int x, int y) pixel, Direction side)
        {
            return side switch
            {
                Direction.Up => pixel,
                Direction.Left => (pixel.x, pixel.y + 1),
                Direction.Down => (pixel.x + 1, pixel.y + 1),
                _ => (pixel.x + 1, pixel.y),
            };
        }

        /// <summary>
        /// Gets the start direction from the side where the search will start.
        /// </summary>
        public static Direction GetStartDirection(Direction side)
        {
            return RotateCounterClockwise(side);
        }

        private static Direction RotateClockwise(Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Right,
                Direction.Left => Direction.Up,
                Direction.Down => Direction.Left,
                _ => Direction.Down,
            };
        }

        private static Direction RotateCounterClockwise(Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Left,
                Direction.Left => Direction.Down,
                Direction.Down => Direction.Right,
                _ => Direction.Up,
            };
        }

        /// <summary>
        /// Returns coordinate of L cell from the supplied direction based on following illustration.
        /// 
        /// If input <paramref name="direction"/> is "Up" (rotate accordingly), then:
        /// 
        /// F | R
        ///   |
        /// --X--
        ///   ^
        /// L |
        /// 
        /// X = point
        /// </summary>
        private static (int x, int y) GetLeftCell((int X, int Y) point, Direction direction)
        {
            return direction switch
            {
                Direction.Up => (point.X - 1, point.Y),
                Direction.Left => point,
                Direction.Down => (point.X, point.Y - 1),
                _ => (point.X - 1, point.Y - 1),
            };
        }


        /// <summary>
        /// Returns coordinate of F cell from the supplied direction based on following illustration.
        /// 
        /// If input <paramref name="direction"/> is "Up" (rotate accordingly), then:
        /// 
        /// F | R
        ///   |
        /// --X--
        ///   ^
        /// L |
        /// 
        /// X = point
        /// </summary>
        private static (int x, int y) GetForwardCell((int X, int Y) point, Direction direction)
        {
            return direction switch
            {
                Direction.Up => (point.X - 1, point.Y - 1),
                Direction.Left => (point.X - 1, point.Y),
                Direction.Down => point,
                _ => (point.X, point.Y - 1),
            };
        }


        /// <summary>
        /// Returns coordinate of R cell from the supplied direction based on following illustration.
        /// 
        /// If input <paramref name="direction"/> is "Up" (rotate accordingly), then:
        /// 
        /// F | R
        ///   |
        /// --X--
        ///   ^
        /// L |
        /// 
        /// X = point
        /// </summary>
        private static (int x, int y) GetRightCell((int X, int Y) point, Direction direction)
        {
            return direction switch
            {
                Direction.Up => (point.X, point.Y - 1),
                Direction.Left => (point.X - 1, point.Y - 1),
                Direction.Down => (point.X - 1, point.Y),
                _ => point,
            };
        }

        private static (int x, int y) MoveForward((int X, int Y) point, Direction direction)
        {
            return direction switch
            {
                Direction.Up => (point.X, point.Y - 1),
                Direction.Left => (point.X - 1, point.Y),
                Direction.Down => (point.X, point.Y + 1),
                _ => (point.X + 1, point.Y),
            };
        }

        /// <summary>
        /// Finds the starting position and direction of the boundary search.
        /// If <paramref name="boundarySides"/> is supplied, it will be searched for first cell which has some boundary.
        /// Else first encountered pixel will be returned.
        /// </summary>
        private static ((int x, int y), Direction) FindStart(bool[][] insidePoints, Direction[][] boundarySides)
        {
            if (boundarySides != null)
            {
                for (var x = 0; x < boundarySides.Length; x++)
                {
                    for (var y = 0; y < boundarySides[x].Length; y++)
                    {
                        if (boundarySides[x][y] != Direction.None)
                        {
                            var side = Direction.Up;
                            for (var i = 0; i < 4; i++)
                            {
                                if (boundarySides[x][y].HasFlag(side))
                                {
                                    return (GetStartPoint((x, y), side), GetStartDirection(side));
                                }

                                side = RotateClockwise(side);
                            }
                        }
                    }
                }
            }
            else
            {
                for (var x = 0; x < insidePoints.Length; x++)
                {
                    for (var y = 0; y < insidePoints[x].Length; y++)
                    {
                        if (insidePoints[x][y])
                        {
                            return ((x, y), Direction.Left);
                        }
                    }
                }
            }

            return ((0, 0), Direction.None);
        }

        /// <summary>
        /// Finds bounding polygon of input set of cells.
        /// Traversal is being done "between" cells.
        /// Boundaries will be removed from <paramref name="boundarySides"/> if it is supplied during the traversal.
        /// 
        /// At every step the cells to the right and left of the current direction are checked.
        /// If there not cell to the left, direction rotates to the left and stores current point.
        /// If there is cell to the right but not to the left, direction is not changed and current point is not stored.
        ///   - no need to store the point, because would be on line between previously stored point and next stored point
        /// If there is a cell to the left and right, direction rotates to the right and stores current point.
        /// In any case algorithm then steps 1 cell forward (after direction might have been changed).
        /// 
        /// Traversal stops when algorithm enters starting point.
        /// This will cause find bounding polygon of set of points, in counter-clockwise direction.
        /// 
        /// Holes inside set of points are not found (if <paramref name="boundarySides"/> is not supplied), only outer most polygon is found.
        /// </summary>
        public static IReadOnlyList<(int x, int y)> GetBoundaryPoints(bool[][] insidePoints, Direction[][] boundarySides = null)
        {
            var result = new List<(int, int)>();

            if (insidePoints.Sum(column => column.Length) == 0)
            {
                return result;
            }

            var maxX = insidePoints.Length;
            var maxY = insidePoints[0].Length;
            var (startPoint, startDirection) = FindStart(insidePoints, boundarySides);
            var point = startPoint;
            var direction = startDirection;

            if (startDirection == Direction.None)
            {
                return result;
            }

            do
            {
                var (forwardX, forwardY) = GetForwardCell(point, direction);
                var (rightX, rightY) = GetRightCell(point, direction);

                if (forwardX >= 0 && forwardX < maxX && forwardY >= 0 && forwardY < maxY && insidePoints[forwardX][forwardY])
                {
                    if (rightX >= 0 && rightX < maxX && rightY >= 0 && rightY < maxY && insidePoints[rightX][rightY]) // turn right
                    {
                        result.Add(point);

                        direction = RotateClockwise(direction);

                        if (boundarySides != null)
                        {
                            boundarySides[rightX][rightY] &= ~RotateClockwise(direction);
                        }
                    }
                    else // go straight
                    {
                        // don't record point

                        // direction doesn't change

                        if (boundarySides != null)
                        {
                            boundarySides[forwardX][forwardY] &= ~RotateClockwise(direction);
                        }
                    }
                }
                else // turn left
                {
                    if (boundarySides != null)
                    {
                        var (leftX, leftY) = GetLeftCell(point, direction);
                        if (leftX >= 0 && leftX < maxX && leftY >= 0 && leftY < maxY)
                        {
                            boundarySides[leftX][leftY] &= ~direction;
                        }
                    }

                    result.Add(point);

                    direction = RotateCounterClockwise(direction);
                }

                point = MoveForward(point, direction);
            }
            while (point != startPoint || direction != startDirection);

            return result;
        }

        /// <summary>
        /// Finds bounding polygons of input set of cells.
        /// Traversal is being done "between" cells.
        /// Also returns holes in polygon.
        /// 
        /// Builds boundary map and then uses <see cref="GetBoundarySides(bool[][])"/> to return all polygons.
        /// Returns the outer boundary first, then holes from left to right (primary sort) and then top to bottom (secondary sort).
        /// </summary>
        public static IEnumerable<IReadOnlyList<(int x, int y)>> GetBoundaryPointsWithHoles(bool[][] insidePoints)
        {
            var boundarySides = GetBoundarySides(insidePoints);

            IReadOnlyList<(int x, int y)> points;
            while (true)
            {
                points = GetBoundaryPoints(insidePoints, boundarySides);
                if (points.Count == 0) yield break;

                yield return points;
            }
        }

        /// <summary>
        /// Builds boundary map of input cells.
        /// For every inside cell checks adjacent cells and if cell not also inside, the boundary is added.
        /// </summary>
        /// <returns>
        /// 2D array of what boundaries specific cell has.
        /// </returns>
        private static Direction[][] GetBoundarySides(bool[][] insidePoints)
        {
            var boundarySides = insidePoints.Select(x => x.Select(y => Direction.None).ToArray()).ToArray();

            for (var x = 0; x < insidePoints.Length; x++)
            {
                for (var y = 0; y < insidePoints[x].Length; y++)
                {
                    if (insidePoints[x][y])
                    {
                        // up pixel
                        if (y == 0 || !insidePoints[x][y - 1])
                        {
                            boundarySides[x][y] |= Direction.Up;
                        }

                        // left pixel
                        if (x == 0 || !insidePoints[x - 1][y])
                        {
                            boundarySides[x][y] |= Direction.Left;
                        }

                        // down pixel
                        if (y + 1 == insidePoints[x].Length || !insidePoints[x][y + 1])
                        {
                            boundarySides[x][y] |= Direction.Down;
                        }

                        // right side
                        if (x + 1 == insidePoints.Length || !insidePoints[x + 1][y])
                        {
                            boundarySides[x][y] |= Direction.Right;
                        }
                    }
                }
            }

            return boundarySides;
        }
    }
}
