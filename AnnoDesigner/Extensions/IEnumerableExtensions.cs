using System.Collections.Generic;
using System.Linq;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Models;

namespace AnnoDesigner.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Removes objects from a given <see cref="IEnumerable{LayoutObject}"/> based on some criteria.
        /// </summary>
        /// <param name="objects">The <see cref="IEnumerable{LayoutObject}"/> to filter.</param>
        /// <returns>The filtered <see cref="IEnumerable{LayoutObject}"/>.</returns>
        public static IEnumerable<LayoutObject> WithoutIgnoredObjects(this IEnumerable<LayoutObject> objects)
        {
            if (objects is null)
            {
                return null;
            }

            var annoObjectsWithoutIgnoredObjects = objects.Select(x => x.WrappedAnnoObject).WithoutIgnoredObjects();

            return objects.Where(x => annoObjectsWithoutIgnoredObjects.Contains(x.WrappedAnnoObject));
        }
    }
}
