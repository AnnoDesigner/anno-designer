using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

        /// <summary>
        /// Generates sequence of double values from <paramref name="from"/> to <paramref name="to"/>.
        /// Can generate sequences in descending order.
        /// Generated sequence always have <paramref name="from"/>. By default <paramref name="to"/> is excluded from sequence unless <paramref name="inclusiveTo"/> is set.
        /// </summary>
        /// <param name="from">First value of the sequence.</param>
        /// <param name="to">Last value of the sequence.</param>
        /// <param name="step">Increment between iterations.</param>
        /// <param name="inclusiveTo">Returns value after last iteration if set to true.</param>
        /// <returns>Sequence of doubles between provided bounds.</returns>
        public static IEnumerable<double> Range(double from, double to, double step = 1, bool inclusiveTo = false)
        {
            if (step <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(step), "Value must be positive");
            }

            double i;
            if (to > from)
            {
                for (i = from; i < to; i += step)
                {
                    yield return i;
                }
            }
            else
            {
                for (i = from; i > to; i -= step)
                {
                    yield return i;
                }
            }
            if (inclusiveTo)
            {
                yield return i;
            }
        }

        /// <summary>
        /// Generates sequence of grid points in rectangle specified by <paramref name="start"/> and <paramref name="inclusiveEnd"/>.
        /// Uses <see cref="Range(double, double, double, bool)"/> to generate X and Y coordinates.
        /// </summary>
        /// <param name="start">Point where rect should start at.</param>
        /// <param name="end">Point where rect should end at.</param>
        /// <param name="step">Steps in horizontal and vertical directions between points. Defaults to (1, 1).</param>
        /// <param name="inclusiveEnd">Includes one point after the <paramref name="end"/>.</param>
        /// <returns>Sequence of grid points in specified rectangle.</returns>
        public static IEnumerable<Point> GeneratePointsInsideRect(Point start, Point end, Size? step = null, bool inclusiveEnd = false)
        {
            step ??= new Size(1, 1);
            foreach (var i in Range(start.X, end.X, step.Value.Width, inclusiveEnd))
            {
                foreach (var j in Range(start.Y, end.Y, step.Value.Height, inclusiveEnd))
                {
                    yield return new Point(i, j);
                }
            }
        }
    }
}
