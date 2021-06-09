using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        public static Dictionary<TKey, TSource> ToDictionaryWithCapacity<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            int capacitiy;

            if (source is ICollection<TSource> collection)
            {
                capacitiy = collection.Count;
            }
            else if (source is IReadOnlyCollection<TSource> readonlyCollection)
            {
                capacitiy = readonlyCollection.Count;
            }
            else
            {
                capacitiy = source.Count();
            }

            var result = new Dictionary<TKey, TSource>(capacitiy);

            foreach (var current in source)
            {
                result.Add(keySelector(current), current);
            }

            return result;
        }

        /// <summary>
        /// Creates an array from the elements in the source sequence. Unlike <see cref="Enumerable.ToArray{TSource}"/>,
        /// this method takes the number of elements as a parameter, so that it can allocate an array of the right size
        /// from the start, hence suppressing the need for subsequent allocations and improving performance.
        /// </summary>
        public static TSource[] ToArrayWithCapacity<TSource>(this IEnumerable<TSource> source, int capacitiy)
        {
            var result = new TSource[capacitiy];

            int i = 0;
            foreach (var item in source)
            {
                result[i++] = item;
            }

            return result;
        }
    }
}
