using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser.WikiText
{
    public class WikiTextTableEntryComparer : IEqualityComparer<WikiTextTableEntry>
    {
        public bool Equals(WikiTextTableEntry x, WikiTextTableEntry y)
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
            return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) &&
                   x.Region.Equals(y.Region);
        }

        public int GetHashCode(WikiTextTableEntry obj)
        {
            if (obj == null)
            {
                return -1;
            }

            unchecked
            {
                //name cannot be null
                //var hashCode = obj.Name != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name) : 0;
                var hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);

                hashCode = (hashCode * 397) ^ obj.Region.GetHashCode();

                return hashCode;
            }
        }
    }
}
