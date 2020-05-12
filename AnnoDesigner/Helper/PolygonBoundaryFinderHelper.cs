using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace AnnoDesigner.Helper
{
    public static class PolygonBoundaryFinderHelper
    {
        private enum Direction
        {
            Up,
            Left,
            Down,
            Right
        }

        private static Point GetLeftCell(Point point, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return new Point(point.X - 1, point.Y - 1);
                case Direction.Left:
                    return new Point(point.X - 1, point.Y);
                case Direction.Down:
                    return point;
                default:
                    return new Point(point.X, point.Y - 1);
            }
        }

        private static Point GetRightCell(Point point, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return new Point(point.X, point.Y - 1);
                case Direction.Left:
                    return new Point(point.X - 1, point.Y - 1);
                case Direction.Down:
                    return new Point(point.X - 1, point.Y);
                default:
                    return point;
            }
        }

        private static Point MoveForward(Point point, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return new Point(point.X, point.Y - 1);
                case Direction.Left:
                    return new Point(point.X - 1, point.Y);
                case Direction.Down:
                    return new Point(point.X, point.Y + 1);
                default:
                    return new Point(point.X + 1, point.Y);
            }
        }

        /// <summary>
        /// Finds bounding polygon of input set of points.
        /// Starts at min point of input set of points (most top left point) and starts moving down.
        /// Traversal is being done "between" cells.
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
        /// Holes inside set of points are not found (not needed for drawing influence ranges), only outer most polygon is found.
        /// HINT:
        /// In order to draw holes inside, the map of boundary edges would have to be constructed and then cleared during the traversal.
        /// Then traversal would restart from any remaining boundary edge until there are non left. Returning list of list of points.
        /// </summary>
        public static IList<Point> GetBoundaryPoints(ISet<Point> insidePoints)
        {
            var result = new List<Point>();

            if (insidePoints.Count == 0)
                return result;

            var minPoint = insidePoints.Min(p => (p.X, p.Y));
            var startPoint = new Point(minPoint.X, minPoint.Y);
            var point = startPoint;
            var direction = Direction.Down;
            result.Add(point);

            do
            {
                var left = GetLeftCell(point, direction);
                var right = GetRightCell(point, direction);

                if (insidePoints.Contains(left))
                {
                    if (insidePoints.Contains(right))// turn right
                    {
                        result.Add(point);

                        direction = (Direction)(((int)direction + 3) % 4);
                    }
                    // else keep moving forward
                }
                else// turn left
                {
                    result.Add(point);

                    direction = (Direction)(((int)direction + 1) % 4);
                }

                point = MoveForward(point, direction);
            }
            while (point != startPoint);

            return result;
        }
    }
}
