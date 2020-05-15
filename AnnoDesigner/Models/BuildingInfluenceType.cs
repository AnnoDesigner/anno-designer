using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Models
{
    /// <summary>
    /// Holds the influence type of a building - not stored in the buildingInfo object itself, this is used in MainWindow.
    /// </summary>
    [Flags]
    public enum BuildingInfluenceType
    {
        None = 0,
        Radius = 1 << 0,
        Distance = 1 << 1,
        Both = ~None//https://stackoverflow.com/a/8488314
    }
}
