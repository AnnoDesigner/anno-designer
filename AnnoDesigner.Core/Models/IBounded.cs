using System.Windows;

namespace AnnoDesigner.Core.Models
{
    public interface IBounded
    {
        Point Position { get; set; }

        Size Size { get; set; }
    }

    public static class Extensions
    {
        public static Rect GetBounds(this IBounded item)
        {
            return new Rect(item.Position, item.Size);
        }
    }
}
