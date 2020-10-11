using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.DataStructures
{
    /// <summary>
    /// Creates a new <see cref="QuadTree{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QuadTree<T> : ICollection<T>
        where T : IBounded
    {
        public enum ResizeDirection
        {
            TopRight,
            TopLeft,
            BottomRight,
            BottomLeft
        }

        private class Quadrant : ICollection<T>
        {
            private readonly Rect topRightBounds;
            private readonly Rect topLeftBounds;
            private readonly Rect bottomRightBounds;
            private readonly Rect bottomLeftBounds;

            public Quadrant TopRight { get; set; }
            public Quadrant TopLeft { get; set; }
            public Quadrant BottomRight { get; set; }
            public Quadrant BottomLeft { get; set; }

            /// <summary>
            /// A count of all items in this Quadrant and under it.
            /// </summary>
            public int Count => ItemsInQuadrant.Count + (TopLeft?.Count ?? 0)    + (TopRight?.Count ?? 0)
                                                      + (BottomLeft?.Count ?? 0) + (BottomRight?.Count ?? 0);

            /// <summary>
            /// Holds a list of all the items in this quadrant.
            /// </summary>
            /// <remarks>
            /// Stored as a list of items as any item that overlaps multiple child quadrants will be stored here.
            /// This list differs to the itemCache, as this only contains items that do not fit completely into a child quadrant.
            /// </remarks>
            public List<T> ItemsInQuadrant { get; }

            public Rect Extent { get; set; }

            public bool IsReadOnly => false;

            /// <summary>
            /// Is this quadrant smaller or equal to 2x2 square.
            /// </summary>
            public bool LastLevelQuadrant => Extent.Width <= 2 && Extent.Height <= 2;

            public Quadrant(Rect extent)
            {
                Extent = extent;
                ItemsInQuadrant = new List<T>(4);

                var w = Extent.Width / 2;
                var h = Extent.Height / 2;

                topRightBounds = new Rect(Extent.Left + w, Extent.Top, w, h);
                topLeftBounds = new Rect(Extent.Left, Extent.Top, w, h);
                bottomRightBounds = new Rect(Extent.Left + w, Extent.Top + h, w, h);
                bottomLeftBounds = new Rect(Extent.Left, Extent.Top + h, w, h);
            }

            public void Add(T item) => Add(item, item.GetBounds());

            /// <summary>
            /// Adds a new item into the Quadrant.
            /// </summary>
            public void Add(T item, Rect bounds)
            {
                if (!LastLevelQuadrant)
                {
                    if (topRightBounds.Contains(bounds))
                    {
                        TopRight ??= new Quadrant(topRightBounds);
                        TopRight.Add(item, bounds);
                    }
                    else if (topLeftBounds.Contains(bounds))
                    {
                        TopLeft ??= new Quadrant(topLeftBounds);
                        TopLeft.Add(item, bounds);
                    }
                    else if (bottomRightBounds.Contains(bounds))
                    {
                        BottomRight ??= new Quadrant(bottomRightBounds);
                        BottomRight.Add(item, bounds);
                    }
                    else if (bottomLeftBounds.Contains(bounds))
                    {
                        BottomLeft ??= new Quadrant(bottomLeftBounds);
                        BottomLeft.Add(item, bounds);
                    }
                    else
                    {
                        ItemsInQuadrant.Add(item);
                    }
                }
                else
                {
                    ItemsInQuadrant.Add(item);
                }
            }

            public bool Remove(T item) => Remove(item, item.GetBounds());

            /// <summary>
            /// Removes an item from this Quadrant.
            /// Returns true if item was found and removed, false otherwise.
            /// </summary>
            public bool Remove(T item, Rect bounds)
            {
                bool removed;

                if (TopRight != null && topRightBounds.Contains(bounds))
                    removed = TopRight.Remove(item, bounds);
                else if (TopLeft != null && topLeftBounds.Contains(bounds))
                    removed = TopLeft.Remove(item, bounds);
                else if (BottomRight != null && bottomRightBounds.Contains(bounds))
                    removed = BottomRight.Remove(item, bounds);
                else if (BottomLeft != null && bottomLeftBounds.Contains(bounds))
                    removed = BottomLeft.Remove(item, bounds);
                else
                    removed = ItemsInQuadrant.Remove(item);

                return removed;
            }

            /// <summary>
            /// Clears this collection.
            /// </summary>
            public void Clear()
            {
                TopRight = null;
                TopLeft = null;
                BottomRight = null;
                BottomLeft = null;
                ItemsInQuadrant.Clear();
            }

            /// <summary>
            /// Checks if specified item is inside this collection.
            /// </summary>
            public bool Contains(T item)
            {
                var bounds = item.GetBounds();
                return TopRight != null && TopRight.Extent.IntersectsWith(bounds) && TopRight.Contains(item) ||
                       TopLeft != null && TopLeft.Extent.IntersectsWith(bounds) && TopLeft.Contains(item) ||
                       BottomRight != null && BottomRight.Extent.IntersectsWith(bounds) && BottomRight.Contains(item) ||
                       BottomLeft != null && BottomLeft.Extent.IntersectsWith(bounds) && BottomLeft.Contains(item) ||
                       ItemsInQuadrant.Contains(item);
            }

            /// <summary>
            /// Copies items from this collection to target array.
            /// </summary>
            public void CopyTo(T[] array, int arrayIndex)
            {
                foreach (var item in this)
                {
                    array[arrayIndex++] = item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// Returns enumerator for iterating all items in this collection.
            /// </summary>
            public IEnumerator<T> GetEnumerator()
            {
                if (TopRight != null)
                    foreach (var item in TopRight)
                        yield return item;

                if (TopLeft != null)
                    foreach (var item in TopLeft)
                        yield return item;

                if (BottomRight != null)
                    foreach (var item in BottomRight)
                        yield return item;

                if (BottomLeft != null)
                    foreach (var item in BottomLeft)
                        yield return item;

                //add all the items in this quadrant that intersect the given bounds
                foreach (var item in ItemsInQuadrant)
                    yield return item;
            }

            /// <summary>
            /// Retrieves the items that intersect with the given bounds.
            /// </summary>
            public IEnumerable<T> GetItemsIntersecting(Rect bounds)
            {
                if (TopRight?.Extent.IntersectsWith(bounds) ?? false)
                    foreach (var item in TopRight.GetItemsIntersecting(bounds))
                        yield return item;

                if (TopLeft?.Extent.IntersectsWith(bounds) ?? false)
                    foreach (var item in TopLeft.GetItemsIntersecting(bounds))
                        yield return item;

                if (BottomRight?.Extent.IntersectsWith(bounds) ?? false)
                    foreach (var item in BottomRight.GetItemsIntersecting(bounds))
                        yield return item;

                if (BottomLeft?.Extent.IntersectsWith(bounds) ?? false)
                    foreach (var item in BottomLeft.GetItemsIntersecting(bounds))
                        yield return item;

                //add all the items in this quadrant that intersect the given bounds
                foreach (var item in ItemsInQuadrant.Where(item => item.GetBounds().IntersectsWith(bounds)))
                    yield return item;
            }

            /// <summary>
            /// Retrieves the <see cref="Quadrant"/> containing the given bounds
            /// </summary>
            public Quadrant GetContainingQuadrant(Rect bounds)
            {
                if (TopRight?.Extent.Contains(bounds) ?? false)
                {
                    return TopRight.GetContainingQuadrant(bounds);
                }
                else if (TopLeft?.Extent.Contains(bounds) ?? false)
                {
                    return TopLeft.GetContainingQuadrant(bounds);
                }
                else if (BottomRight?.Extent.Contains(bounds) ?? false)
                {
                    return BottomRight.GetContainingQuadrant(bounds);
                }
                else if (BottomLeft?.Extent.Contains(bounds) ?? false)
                {
                    return BottomLeft.GetContainingQuadrant(bounds);
                }
                else if (Extent.Contains(bounds))
                {
                    return this;
                }
                return null;
            }

#if DEBUG
            /// <summary>
            /// Retrieves a list of Rects that make up this quadrant and the quadrants beneath it.
            /// Used when debugging to draw quadrants to a canvas.
            /// </summary>
            /// <param name="rects"></param>
            public IEnumerable<Rect> GetQuadrantRects()
            {
                yield return Extent;

                if (TopLeft != null)
                    foreach (var rect in TopLeft.GetQuadrantRects())
                        yield return rect;

                if (TopRight != null)
                    foreach (var rect in TopRight.GetQuadrantRects())
                        yield return rect;

                if (BottomLeft != null)
                    foreach (var rect in BottomLeft.GetQuadrantRects())
                        yield return rect;

                if (BottomRight != null)
                    foreach (var rect in BottomRight.GetQuadrantRects())
                        yield return rect;
            }
#endif
        }

        /// <summary>
        /// The root of the <see cref="QuadTree{T}"/>
        /// </summary>
        private Quadrant root;

        /// <summary>
        /// Get the bounds of this QuadTree;
        /// </summary>
        public Rect Extent => root.Extent;

        public int Count => root.Count;

        public bool IsReadOnly => false;

        /// <summary>
        /// Create a <see cref="QuadTree{T}"/>
        /// </summary>
        /// <param name="extent">The bounds of the <see cref="QuadTree{T}"/></param>
        public QuadTree(Rect extent)
        {
            root = new Quadrant(extent);
        }

        /// <summary>
        /// Insert a <typeparamref name="T"/> item into the QuadTree.
        /// </summary>
        public void Add(T item)
        {
            var bounds = item.GetBounds();
            EnsureBounds(bounds);

            root.Add(item, bounds);

            if (item is INotifyPropertyChangedWithValue notify)
            {
                notify.PropertyChanged += Item_PropertyChanged;
            }
        }

        /// <summary>
        /// Remove a <typeparamref name="T"/> item from the QuadTree.
        /// </summary>
        public bool Remove(T item)
        {
            var removed = root.Remove(item);

            if (removed)
            {
                if (item is INotifyPropertyChangedWithValue notify)
                {
                    notify.PropertyChanged -= Item_PropertyChanged;
                }
            }
            return removed;
        }

        /// <summary>
        /// Removes all items from the QuadTree.
        /// </summary>
        public void Clear()
        {
            foreach (var item in this.ToList())
            {
                Remove(item);
            }
        }

        public bool Contains(T item) => root.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => root.CopyTo(array, arrayIndex);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => root.GetEnumerator();

        /// <summary>
        /// Ensures specified bound is in Extent of this QuadTree.
        /// </summary>
        public void EnsureBounds(Rect bounds)
        {
            while (!Extent.Contains(bounds))
            {
                var newExtent = Extent;
                newExtent.Union(bounds);
                var top = Extent.Top - newExtent.Top > 0;
                var bottom = newExtent.Bottom - Extent.Bottom > 0;
                var left = Extent.Left - newExtent.Left > 0;

                if (top && left) Inflate(ResizeDirection.TopLeft);
                else if (top) Inflate(ResizeDirection.TopRight);
                else if (bottom && left) Inflate(ResizeDirection.BottomLeft);
                else if (bottom) Inflate(ResizeDirection.BottomRight);
                else if (left) Inflate(ResizeDirection.TopLeft);
                else Inflate(ResizeDirection.BottomRight);
            }
        }

        /// <summary>
        /// Doubles the size of QuadTree in specific direction.
        /// </summary>
        public void Inflate(ResizeDirection direction)
        {
            var oldRoot = root;
            var oldWidth = new Vector(Extent.Width, 0);
            var oldHeight = new Vector(0, Extent.Height);
            var newSize = new Size(Extent.Width * 2, Extent.Height * 2);

            switch (direction)
            {
                case ResizeDirection.TopRight:
                    root = new Quadrant(new Rect(Extent.TopLeft - oldHeight, newSize));
                    root.BottomLeft = oldRoot;
                    break;
                case ResizeDirection.TopLeft:
                    root = new Quadrant(new Rect(Extent.TopLeft - oldHeight - oldWidth, newSize));
                    root.BottomRight = oldRoot;
                    break;
                case ResizeDirection.BottomRight:
                    root = new Quadrant(new Rect(Extent.TopLeft, newSize));
                    root.TopLeft = oldRoot;
                    break;
                case ResizeDirection.BottomLeft:
                    root = new Quadrant(new Rect(Extent.TopLeft - oldWidth, newSize));
                    root.TopRight = oldRoot;
                    break;
            }
        }

        /// <summary>
        /// Handles changes in items.
        /// Reindexes item if its position or size changed.
        /// </summary>
        private void Item_PropertyChanged(object sender, PropertyChangedWithValueEventArgs e)
        {
            if (sender is T item)
            {
                switch (e.PropertyName)
                {
                    case nameof(IBounded.Position):
                        Reindex(item, new Rect((Point)e.OldValue, item.Size));
                        break;
                    case nameof(IBounded.Size):
                        Reindex(item, new Rect(item.Position, (Point)e.OldValue));
                        break;
                }
            }
        }

        /// <summary>
        /// Reindexes item.
        /// </summary>
        public void Reindex(T item, Rect oldBounds)
        {
            if (root.GetContainingQuadrant(item.GetBounds()) != root.GetContainingQuadrant(oldBounds))
            {
                root.Remove(item, oldBounds);
                root.Add(item);
            }
        }

        /// <summary>
        /// Retrieves a sequence of items that intersect the given bounds.
        /// </summary>
        /// <param name="bounds">The bounds to check</param>
        /// <returns></returns>
        public IEnumerable<T> GetItemsIntersecting(Rect bounds)
        {
            return root.GetItemsIntersecting(bounds);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                Add(item);
            }
        }

#if DEBUG
        /// <summary>
        /// Retrieves a list of Rects that make up the Quadrants in this QuadTree.
        /// Used when debugging to draw the QuadTree quadrants to a canvas.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Rect> GetQuadrantRects()
        {
            return root.GetQuadrantRects();
        }
#endif
    }
}
