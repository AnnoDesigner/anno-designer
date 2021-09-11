using System.Windows;

namespace AnnoDesigner.Core.Models
{
    public interface IBounded
    {
        Point Position { get; set; }

        Size Size { get; set; }

        Rect Bounds { get; set; }
    }
}
