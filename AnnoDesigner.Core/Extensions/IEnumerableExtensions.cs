using System;
using System.Collections.Generic;
using System.Linq;
using AnnoDesigner.Core.Models;

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
        /// Creates a list from the elements in the source sequence. Unlike <see cref="Enumerable.ToList{TSource}"/>,
        /// this method takes the number of elements as a parameter, so that it can allocate a list of the right size
        /// from the start, hence suppressing the need for subsequent allocations and improving performance.
        /// </summary>
        public static List<TSource> ToListWithCapacity<TSource>(this ICollection<TSource> source)
        {
            var list = new List<TSource>(source.Count);

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

        /// <summary>
        /// Removes objects from a given <see cref="IEnumerable{AnnoObject}"/> based on some criteria.
        /// </summary>
        /// <param name="objects">The <see cref="IEnumerable{AnnoObject}"/> to filter.</param>
        /// <returns>The filtered <see cref="IEnumerable{AnnoObject}"/>.</returns>
        /// <remarks>Currently the logic is based only on a single "Template", but can be extended to other criteria in the future.</remarks>
        public static IEnumerable<AnnoObject> WithoutIgnoredObjects(this IEnumerable<AnnoObject> objects)
        {
            if (objects is null)
            {
                return null;
            }

            return objects.Where(x => !x.IsIgnoredObject());
        }

        /// <summary>
        /// Checks if input object should be excluded from certain rendering actions.
        /// </summary>
        /// <remarks>
        /// Currently the logic is based only on a single "Template", but can be extended to other criteria in the future.
        /// </remarks>
        public static bool IsIgnoredObject(this AnnoObject annoObject)
        {
            return string.Equals(annoObject.Template, "Blocker", StringComparison.OrdinalIgnoreCase);
        }

        public static T MinOrDefault<T>(this IEnumerable<T> enumerable, T @default = default)
        {
            if (!enumerable.Any()) return @default;

            return enumerable.Min();
        }

        public static TResult MinOrDefault<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult> selector, TResult @default = default)
        {
            if (!enumerable.Any()) return @default;

            return enumerable.Min(selector);
        }

        public static T MaxOrDefault<T>(this IEnumerable<T> enumerable, T @default = default)
        {
            if (!enumerable.Any()) return @default;

            return enumerable.Max();
        }

        public static TResult MaxOrDefault<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult> selector, TResult @default = default)
        {
            if (!enumerable.Any()) return @default;

            return enumerable.Max(selector);
        }
    }
}
