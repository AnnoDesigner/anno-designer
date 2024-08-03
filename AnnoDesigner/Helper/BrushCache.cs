using AnnoDesigner.Models;
using System.Collections.Generic;
using System.Windows.Media;

namespace AnnoDesigner.Helper;

public class BrushCache : IBrushCache
{
    private static readonly Dictionary<Color, SolidColorBrush> _cachedBrushes;

    static BrushCache()
    {
        _cachedBrushes = [];
    }

    public SolidColorBrush GetSolidBrush(Color color)
    {
        if (!_cachedBrushes.TryGetValue(color, out SolidColorBrush foundBrush))
        {
            foundBrush = new SolidColorBrush(color);
            if (foundBrush.CanFreeze)
            {
                foundBrush.Freeze();
            }

            _cachedBrushes.Add(color, foundBrush);
        }

        return foundBrush;
    }
}
