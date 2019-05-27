using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.model
{
    /// <summary>
    /// Holds the influence type of a building - not stored in the buildingInfo object itself, this is used in MainWindow.
    /// </summary>
    public enum BuildingInfluenceType
    {
        None,
        Radius,
        Distance,
        Both = Radius | Distance,
    }
}
