using System.Collections.Generic;
using System.Linq;

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

        private static (int x, int y) GetLeftPixel((int x, int y) point, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return (point.x - 1, point.y - 1);
                case Direction.Left:
                    return (point.x - 1, point.y);
                case Direction.Down:
                    return point;
                default:
                    return (point.x, point.y - 1);
            }
        }

        private static (int x, int y) GetRightPixel((int x, int y) point, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return (point.x, point.y - 1);
                case Direction.Left:
                    return (point.x - 1, point.y - 1);
                case Direction.Down:
                    return (point.x - 1, point.y);
                default:
                    return point;
            }
        }

        private static (int x, int y) MoveForward((int x, int y) point, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return (point.x, point.y - 1);
                case Direction.Left:
                    return (point.x - 1, point.y);
                case Direction.Down:
                    return (point.x, point.y + 1);
                default:
                    return (point.x + 1, point.y);
            }
        }

        public static IList<(int x, int y)> GetBoundaryPoints(ISet<(int x, int y)> insidePoints)
        {
            var points = new List<(int, int)>();

            if (insidePoints.Count == 0)
                return points;

            var startPoint = insidePoints.Min();
            var point = startPoint;
            var direction = Direction.Down;
            points.Add(point);

            do
            {
                var left = GetLeftPixel(point, direction);
                var right = GetRightPixel(point, direction);

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
