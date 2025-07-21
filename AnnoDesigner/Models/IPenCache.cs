using System.Windows.Media;

namespace AnnoDesigner.Models;

public interface IPenCache
{
    Pen GetPen(Brush brush, double thickness);
}
