using System.Windows.Media;

namespace AnnoDesigner.Models;

public interface IBrushCache
{
    SolidColorBrush GetSolidBrush(Color color);
}
