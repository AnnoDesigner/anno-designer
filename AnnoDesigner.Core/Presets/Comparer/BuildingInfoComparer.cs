using System;
using System.Collections.Generic;
using System.Linq;
using AnnoDesigner.Core.Presets.Models;

namespace AnnoDesigner.Core.Presets.Comparer
{
    /// <summary>
    /// Comparer used to check if two BuildingInfo groups match
    /// </summary>
    public class BuildingInfoComparer : IEqualityComparer<IBuildingInfo>
    {
        public bool Equals(IBuildingInfo x, IBuildingInfo y)
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
            return string.Equals(x.Group, y.Group, StringComparison.OrdinalIgnoreCase);
            //&& string.Equals(x.Identifier, y.Identifier, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(IBuildingInfo obj)
        {
            if (obj == null)
            {
                return -1;
            }

            unchecked
            {
                var hashCode = obj.Group != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Group) : 0;
                //hashCode = (hashCode * 397) ^ (obj.Identifier != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Identifier) : 0);

                return hashCode;
            }
        }
    }
}