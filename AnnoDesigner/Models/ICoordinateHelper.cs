using AnnoDesigner.Core.Models;
using System.Windows;

namespace AnnoDesigner.Models;

public interface ICoordinateHelper
{
    Point GetCenterPoint(Rect rect);

    double GridToScreen(double gridLength, int gridStep);

    Point GridToScreen(Point gridPoint, int gridStep);

    Size GridToScreen(Size gridSize, int gridStep);

    Size Rotate(Size size);

    Rect Rotate(Rect rect);

    GridDirection Rotate(GridDirection direction);

    double RoundScreenToGrid(double screenLength, int gridStep);

    Point RoundScreenToGrid(Point screenPoint, int gridStep);

    double ScreenToGrid(double screenLength, int gridStep);

    Point ScreenToGrid(Point screenPoint, int gridStep);

    Point ScreenToFractionalGrid(Point screenPoint, int gridStep);

    public Rect ScreenToGrid(Rect rect, int gridStep);

    public Rect GridToScreen(Rect rect, int gridStep);
}