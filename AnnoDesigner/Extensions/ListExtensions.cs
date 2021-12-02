using System.Collections.Generic;
using System.Linq;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Models;

namespace AnnoDesigner.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Removes objects from a given <see cref="List{LayoutObject}"/> based on some criteria.
        /// </summary>
        /// <param name="objects">The <see cref="List{LayoutObject}"/> to filter.</param>
        /// <returns>The filtered <see cref="List{LayoutObject}"/>.</returns>
        public static List<LayoutObject> WithoutIgnoredObjects(this List<LayoutObject> objects)
        {
            if (objects is null)
            {
                return null;
            }

            var annoObjectsWithoutIgnoredObjects = objects.Select(x => x.WrappedAnnoObject).WithoutIgnoredObjects();

            return objects.Where(x => annoObjectsWithoutIgnoredObjects.Contains(x.WrappedAnnoObject)).ToList();
        }
    }
}
