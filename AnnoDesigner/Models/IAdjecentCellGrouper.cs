using System.Collections.Generic;
using System.Windows;

namespace AnnoDesigner.Models
{
    public class CellGroup<T>
    {
        public Rect Bounds { get; set; }
        public ISet<T> Items { get; set; }

        public CellGroup(Rect bounds, ISet<T> items)
        {
            Bounds = bounds;
            Items = items;
        }
    }

    public interface IAdjacentCellGrouper
    {
        IEnumerable<CellGroup<T>> GroupAdjacentCells<T>(T[][] cells, bool returnSingleCells = false);
    }
}
