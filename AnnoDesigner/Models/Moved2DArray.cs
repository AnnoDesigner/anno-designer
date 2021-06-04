using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnnoDesigner.Models
{
    /// <summary>
    /// Holds 2D array of objects which with extra information about how offset the array is from something.
    /// Used to hold objects on canvas in array even if their position might be negative.
    /// </summary>
    public class Moved2DArray<T> : IReadOnlyList<T[]>
    {
        public T[][] Array { get; set; }

        public (int x, int y) Offset { get; set; }

        public int Count => Array.Length;

        public T[] this[int index] => Array[index];

        public IEnumerator<T[]> GetEnumerator()
        {
            return Array.Cast<T[]>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Array.GetEnumerator();
        }
    }
}
