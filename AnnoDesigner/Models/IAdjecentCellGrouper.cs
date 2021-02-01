using System.Collections.Generic;

namespace AnnoDesigner.Models
{
    public interface IAdjacentCellGrouper
    {
        IEnumerable<CellGroup<T>> GroupAdjacentCells<T>(T[][] cells, bool returnSingleCells = false);
    }
}
