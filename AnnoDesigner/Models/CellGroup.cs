using System.Collections.Generic;
using System.Windows;

namespace AnnoDesigner.Models
{
    public class CellGroup<T>
    {
        public CellGroup(Rect bounds, ISet<T> items)
        {
            Bounds = bounds;
            Items = items;
        }

        public Rect Bounds { get; set; }
        public ISet<T> Items { get; set; }
    }
}
