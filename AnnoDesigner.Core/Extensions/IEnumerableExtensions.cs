using System;
using System.Collections.Generic;
using System.Linq;

namespace AnnoDesigner.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        public static Dictionary<TKey, TSource> ToDictionaryWithCapacity<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            int capacity;

            if (source is ICollection<TSource> collection)
            {
                capacity = collection.Count;
            }
            else if (source is IReadOnlyCollection<TSource> readonlyCollection)
            {
                capacity = readonlyCollection.Count;
            }
            else
            {
                capacity = source.Count();
            }

            var result = new Dictionary<TKey, TSource>(capacity);

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
        public static TSource[] ToArrayWithCapacity<TSource>(this IEnumerable<TSource> source, int capacity)
        {
            var result = new TSource[capacity];

            int i = 0;
            foreach (var item in source)
            {
                result[i++] = item;
            }

            return result;
        }

        /// <summary>
        /// Creates a list from the elements in the source sequence. Unlike <see cref="Enumerable.ToList{TSource}"/>,
        /// this method takes the number of elements as a parameter, so that it can allocate a list of the right size
        /// from the start, hence suppressing the need for subsequent allocations and improving performance.
        /// </summary>
        public static List<TSource> ToListWithCapacity<TSource>(this IEnumerable<TSource> source, int capacity)
        {
            var list = new List<TSource>(capacity);

            list.AddRange(source);

            return list;
        }

        /// <summary>
        /// Iterates over each item in <paramref name="source"/>.
        /// Use on lazily evaluated iterator to materialize each item.
        /// </summary>
        public static void Consume<T>(this IEnumerable<T> source)
        {
            foreach (var _ in source) { }
        }
    }
}
