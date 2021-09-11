using System.Collections.Generic;
using System.Windows;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.DataStructures
{
    public interface IQuadTree<T> : ICollection<T>
        where T : IBounded
    {
        Rect Extent { get; }

        void EnsureBounds(Rect bounds);

        void Inflate(ResizeDirection direction);

        void ReIndex(T item, Rect oldBounds);

        void Move(T item, Rect newBounds);

        void Move(T item, Vector offset);

        IEnumerable<T> GetItemsIntersecting(Rect bounds);

        void AddRange(IEnumerable<T> collection);

#if DEBUG
        IEnumerable<Rect> GetQuadrantRects();
#endif
    }
}
