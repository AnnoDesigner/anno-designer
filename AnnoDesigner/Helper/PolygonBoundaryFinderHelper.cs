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

        public static IList<Point> GetBoundaryPoints(ISet<Point> insidePoints)
        {
            var points = new List<Point>();

            if (insidePoints.Count == 0)
                return points;

            var minPoint = insidePoints.Min(p => (p.X, p.Y));
            var startPoint = new Point(minPoint.X, minPoint.Y);
            var point = startPoint;
            var direction = Direction.Down;
            points.Add(point);

            do
            {
                var left = GetLeftCell(point, direction);
                var right = GetRightCell(point, direction);

                if (insidePoints.Contains(left))
                {
                    if (insidePoints.Contains(right))// turn right
                    {
                        points.Add(point);

                        direction = (Direction)(((int)direction + 3) % 4);
                    }
                    // else keep moving forward
                }
                else// turn left
                {
                    points.Add(point);

                    direction = (Direction)(((int)direction + 1) % 4);
                }

                point = MoveForward(point, direction);
            }
            while (point != startPoint);

            return points;
        }
    }
}
