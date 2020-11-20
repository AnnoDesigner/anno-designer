using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AnnoDesigner.Models;

namespace AnnoDesigner.Helper
{
    public class AdjacentCellGrouper : IAdjacentCellGrouper
    {
        /// <summary>
        /// Searches input 2D generic array for clusters of adjacent cells.
        /// Cells with non-default value are considered to be occupied.
        /// Cells have to have common edge to be considered adjacent.
        /// Largest groups are returned first.
        /// For every rectangle returned the entire array will be searched.
        /// Input array WILL be modified in the process (to not take into account one cell multiple times).
        /// </summary>
        /// <param name="returnSingleCells">Set to true to also return single cells at the end of the search.</param>
        /// <returns>Lazily evaluated integer rectangle of grouped cells and set of grouped cells.</returns>
        public IEnumerable<CellGroup<T>> GroupAdjacentCells<T>(T[][] cells, bool returnSingleCells = false)
        {
            var items = new HashSet<T>();
            while (true)
            {
                var largest = FindLargestGroup(cells);
                if (largest.Width * largest.Height > 1)
                {
                    for (var x = 0; x < largest.Width; x++)
                        for (var y = 0; y < largest.Height; y++)
                        {
                            items.Add(cells[(int)largest.X + x][(int)largest.Y + y]);
                            cells[(int)largest.X + x][(int)largest.Y + y] = default;
                        }

                    yield return new CellGroup<T>(largest, items);
                }
                else if (returnSingleCells && largest.Width * largest.Height > 0)
                {
                    for (var x = 0; x < cells.Length; x++)
                        for (var y = 0; y < cells[x].Length; y++)
                            if (!EqualityComparer<T>.Default.Equals(cells[x][y], default))
                            {
                                items.Add(cells[x][y]);
                                cells[x][y] = default;
                                yield return new CellGroup<T>(new Rect(x, y, 1, 1), items);
                            }

                    yield break;
                }
                else
                {
                    yield break;
                }
                items = new HashSet<T>();
            }
        }

        /// <summary>
        /// Goes through entire input array and returns largest rectangle found.
        /// Array is scanned column by column, histogram of longest continuous rows is kept.
        /// Every histogram is then scanned for largest area.
        /// </summary>
        /// <returns>Largest rectangle found, or (0, 0, 0, 0) if none was found.</returns>
        private Rect FindLargestGroup<T>(T[][] cells)
        {
            var largest = new Rect();
            
            var column = new int[cells.Max(c => c.Length)];
            for (var x = 0; x < cells.Length; x++)
            {
                for (var y = 0; y < cells[x].Length; y++)
                    column[y] = !EqualityComparer<T>.Default.Equals(cells[x][y], default) ? column[y] + 1 : 0;

                var area = FindLargestAreaUnderHistogram(column);

                if (largest.Width * largest.Height < area.width * area.height)
                    largest = new Rect(x - area.width + 1, area.y, area.width, area.height);
            }

            return largest;
        }

        /// <summary>
        /// Calculates largest area under histogram.
        /// Taken from https://www.geeksforgeeks.org/largest-rectangle-under-histogram/ with some small modifications.
        /// Kind of black magic but I'll try to explain it.
        /// Keep in mind that this is histogram of a column (bars would be horizontal),so "width" is histogram's height.
        ///   - Goes through the column and while it keeps finding incrementing histogram size it stores indexes of
        ///     these items on stack for further processing.
        ///   - If it encounters item which is smaller than item with index from top of the stack.
        ///     It means the algorithm has encountered a bottleneck which the histogram must try to fit under.
        ///     - Calculates the area before this bottleneck.
        ///   - Once entire column was processed (either by adding index to stack or processing the bottleneck),
        ///     process rest of the items left on the stack like they would be the bottleneck.
        /// </summary>
        /// <remarks>
        /// Probably the best histogram which shows how this algorithm works is:
        /// |
        /// |    XXX
        /// |   XXXXX
        /// |  XXXXXXX
        /// | XXXXXXXXX
        /// |XXXXXXXXXXX
        /// |------------
        /// </remarks>
        /// <returns>Tuple (start index of largest area under histogram, max width, max height)</returns>
        private (int y, int width, int height) FindLargestAreaUnderHistogram(int[] column)
        {
            var maxY = 0;
            var maxWidth = 0;
            var maxHeight = 0;

            var stack = new Stack<int>(column.Length);
            var y = 0;

            void CalculateAreaAndUpdate()
            {
                var peak = stack.Pop();

                var width = column[peak];
                var height = stack.Count > 0 ? y - stack.Peek() - 1 : y;

                if (maxWidth * maxHeight < width * height)
                {
                    maxY = y - height;
                    maxWidth = width;
                    maxHeight = height;
                }
            }

            while (y < column.Length)
            {
                if (stack.Count == 0 || column[stack.Peek()] <= column[y])
                {
                    stack.Push(y++);
                }
                else
                {
                    CalculateAreaAndUpdate();
                }
            }

            while (stack.Count > 0)
            {
                CalculateAreaAndUpdate();
            }

            return (maxY, maxWidth, maxHeight);
        }
    }
}
