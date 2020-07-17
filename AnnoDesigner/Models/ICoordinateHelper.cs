using System.Windows;

namespace AnnoDesigner.Models
{
    public interface ICoordinateHelper
    {
        Point GetCenterPoint(Rect rect);

        double GridToScreen(double gridLength, int gridStep);

        Point GridToScreen(Point gridPoint, int gridStep);

        Size GridToScreen(Size gridSize, int gridStep);

        Size Rotate(Size size);

        double RoundScreenToGrid(double screenLength, int gridStep);

        Point RoundScreenToGrid(Point screenPoint, int gridStep);

        double ScreenToGrid(double screenLength, int gridStep);

        Point ScreenToGrid(Point screenPoint, int gridStep);

        Point ScreenToPreciseGrid(Point screenPoint, int gridStep);
    }
}