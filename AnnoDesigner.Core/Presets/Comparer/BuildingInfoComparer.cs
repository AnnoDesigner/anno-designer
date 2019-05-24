using System;
using System.Collections.Generic;
using System.Linq;
using AnnoDesigner.Core.Presets.Models;

namespace AnnoDesigner.Core.Presets.Comparer
{
    /// <summary>
    /// Comparer used to check if two BuildingInfo groups match
    /// </summary>
    public class BuildingInfoComparer : IEqualityComparer<BuildingInfo>
    {
        public bool Equals(BuildingInfo x, BuildingInfo y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            //Check whether the compared objects reference the same data.
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            //Check whether the buildingInfo group properties are equal
            return x.Group == y.Group;
        }

        public int GetHashCode(BuildingInfo obj)
        {
            if (obj == null)
            {
                return -1;
            }

            return base.GetHashCode();
        }
    }
}