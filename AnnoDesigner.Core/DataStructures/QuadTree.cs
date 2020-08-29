using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace AnnoDesigner.Core.DataStructures
{
    /// <summary>
    /// Creates a new <see cref="QuadTree{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QuadTree<T> : IEnumerable<T>
    {
        private class Quadrant
        {
            /// <summary>
            /// Indicates if current cached data (<see cref="count"/> and <see cref="itemCache"/>) is up to date.
            /// Returns <see langword="true"/> when cached data is up to date.
            /// </summary>
            private bool isDirty;

            private Quadrant topRight;
            private Quadrant topLeft;
            private Quadrant bottomRight;
            private Quadrant bottomLeft;

            private readonly Rect topRightBounds;
            private readonly Rect topLeftBounds;
            private readonly Rect bottomRightBounds;
            private readonly Rect bottomLeftBounds;

            /// <summary>
            /// A reference to the parent <see cref="Quadrant"/> for this Quadrant.
            /// </summary>
            public Quadrant Parent { get; set; }

            /// <summary>
            /// A count of all items in this Quadrant and under it.
            /// </summary>
            private int count;

            /// <summary>
            /// A list of all the items in the Quadrant and the items under it.
            /// </summary>
            private IEnumerable<(T Item, Rect Bounds)> itemCache;

            /// <summary>
            /// Holds a list of all the items in this quadrant.
            /// </summary>
            /// <remarks>
            /// Stored as a list of items as any item that overlaps multiple child quadrants will be stored here.
            /// This list differs to the itemCache, as this only contains items that do not fit completely into a child quadrant.
            /// </remarks>
            public List<(T Item, Rect Bounds)> Items { get; }

            public Rect Extent { get; set; }

            public Quadrant(Rect extent)
            {
                Extent = extent;
                Items = new List<(T, Rect)>(4);
                itemCache = new List<(T Items, Rect Bounds)>();
                isDirty = false;

                var w = Extent.Width / 2;
                var h = Extent.Width / 2;

                topRightBounds = new Rect(Extent.Left + w, Extent.Top, w, h);
                topLeftBounds = new Rect(Extent.Left, Extent.Top, w, h);
                bottomRightBounds = new Rect(Extent.Left + w, Extent.Top + h, w, h);
                bottomLeftBounds = new Rect(Extent.Left, Extent.Top + h, w, h);
            }

            /// <summary>
            /// Insert a new item into the Quad Tree.
            /// </summary>
            /// <param name="item">The item to insert</param>
            /// <param name="bounds">The bounds of the item</param>
            public void Insert(T item, Rect bounds)
            {
                if (!Extent.IntersectsWith(bounds))
                {
                    return; //item does not belong in quadrant
                }

                Quadrant childQuadrant = null;
                if (topRightBounds.Contains(bounds))
                {
                    topRight ??= new Quadrant(topRightBounds);
                    childQuadrant = topRight;
                }
                else if (topLeftBounds.Contains(bounds))
                {
                    topLeft ??= new Quadrant(topLeftBounds);
                    childQuadrant = topLeft;
                }
                else if (bottomRightBounds.Contains(bounds))
                {
                    bottomRight ??= new Quadrant(bottomRightBounds);
                    childQuadrant = bottomRight;
                }
                else if (bottomLeftBounds.Contains(bounds))
                {
                    bottomLeft ??= new Quadrant(bottomLeftBounds);
                    childQuadrant = bottomLeft;
                }

                if (childQuadrant is null)
                {
                    Items.Add((item, bounds));
                }
                else
                {
                    childQuadrant.Parent = this;
                    childQuadrant.Insert(item, bounds);
                }

                isDirty = true;
            }

            /// <summary>
            /// Removes an item from this Quadrant
            /// </summary>
            /// <param name="item"></param>
            internal void Remove((T item, Rect bounds) item)
            {
                Items.Remove(item);
                MarkAncestorsAsDirty(); //make sure all ancestors are now marked as requiring an update.
            }

            /// <summary>
            /// Marks this quadrant and all parent quadrants as dirty.
            /// </summary>
            internal void MarkAncestorsAsDirty()
            {
                if (Parent != null)
                {
                    Parent.MarkAncestorsAsDirty();
                }
                isDirty = true;
            }

            /// <summary>
            /// Retrieves the items that intersect with the given bounds.
            /// </summary>
            /// <param name="bounds"></param>
            /// <returns></returns>
            public void GetItemsIntersecting(List<T> items, Rect bounds)
            {
                if (topRight != null && topRight.Extent.IntersectsWith(bounds))
                {
                    topRight.GetItemsIntersecting(items, bounds);
                }
                if (topLeft != null && topLeft.Extent.IntersectsWith(bounds))
                {
                    topLeft.GetItemsIntersecting(items, bounds);
                }
                if (bottomRight != null && bottomRight.Extent.IntersectsWith(bounds))
                {
                    bottomRight.GetItemsIntersecting(items, bounds);
                }
                if (bottomLeft != null && bottomLeft.Extent.IntersectsWith(bounds))
                {
                    bottomLeft.GetItemsIntersecting(items, bounds);
                }
                //add all the items in this quadrant that intersect the given bounds
                items.AddRange(Items.Where(_ => _.Bounds.IntersectsWith(bounds)).Select(_ => _.Item));
            }

            /// <summary>
            /// Retrieves the <see cref="Quadrant"/> containing the given bounds
            /// </summary>
            /// <param name="bounds"></param>
            /// <returns></returns>
            internal Quadrant GetContainingQuadrant(Rect bounds)
            {
                if (topRight != null && topRight.Extent.Contains(bounds))
                {
                    return topRight.GetContainingQuadrant(bounds);
                }
                else if (topLeft != null && topLeft.Extent.Contains(bounds))
                {
                    return topLeft.GetContainingQuadrant(bounds);
                }
                else if (bottomRight != null && bottomRight.Extent.Contains(bounds))
                {
                    return bottomRight.GetContainingQuadrant(bounds);
                }
                else if (bottomLeft != null && bottomLeft.Extent.Contains(bounds))
                {
                    return bottomLeft.GetContainingQuadrant(bounds);
                }
                else
                {
                    return this;
                }
            }

            /// <summary>
            /// Retrieves the count of all items in this quadrant and beneath it.
            /// </summary>
            /// <returns></returns>
            public int Count()
            {
                if (isDirty)
                {
                    UpdateCachedData();
                }
                return count;
            }

            /// <summary>
            /// Retrieves a list of all items in this quadrant and beneath it.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<T> All()
            {
                if (isDirty)
                {
                    UpdateCachedData();
                }
                return itemCache.Select(_ => _.Item);
            }

            /// <summary>
            /// Returns all the items in this quadrant and beneath it, including the bounds of the item.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<(T Item, Rect Bounds)> AllWithBounds()
            {
                var items = new List<(T Item, Rect Bounds)>(count);
                var empty = new List<(T Item, Rect Bounds)>(0);
                items.AddRange(topLeft?.AllWithBounds() ?? empty);
                items.AddRange(topRight?.AllWithBounds() ?? empty);
                items.AddRange(bottomRight?.AllWithBounds() ?? empty);
                items.AddRange(bottomLeft?.AllWithBounds() ?? empty);
                items.AddRange(Items);
                return items;
            }

            /// <summary>
            /// Updates the cahced data for this quadrant.
            /// </summary>
            private void UpdateCachedData()
            {
                //initialise with the outdated count value
                var newItems = new List<(T Item, Rect Bounds)>(count);
                var empty = new List<(T Item, Rect Bounds)>(0);
                newItems.AddRange(topLeft?.AllWithBounds() ?? empty);
                newItems.AddRange(topRight?.AllWithBounds() ?? empty);
                newItems.AddRange(bottomRight?.AllWithBounds() ?? empty);
                newItems.AddRange(bottomLeft?.AllWithBounds() ?? empty);
                newItems.AddRange(Items);
                count = newItems.Count;
                itemCache = newItems;
                isDirty = false;
            }

#if DEBUG
            /// <summary>
            /// Retrieves a list of Rects that make up this quadrant and the quadrants beneath it.
            /// Used when debugging to draw quadrants to a canvas.
            /// </summary>
            /// <param name="rects"></param>
            internal void GetQuadrantRects(List<Rect> rects)
            {
                if (topLeft != null)
                {
                    rects.Add(topLeftBounds);
                    topLeft.GetQuadrantRects(rects);
                }
                if (topRight != null)
                {
                    rects.Add(topRightBounds);
                    topRight.GetQuadrantRects(rects);
                }
                if (bottomLeft != null)
                {
                    rects.Add(bottomLeftBounds);
                    bottomLeft.GetQuadrantRects(rects);
                }
                if (bottomRight != null)
                {
                    rects.Add(bottomRightBounds);
                    bottomRight.GetQuadrantRects(rects);
                }
            }
#endif
        }

        /// <summary>
        /// The root of the <see cref="QuadTree{T}"/>
        /// </summary>
        private Quadrant root;

        /// <summary>
        /// A value representing the size of the list returned from methods on this QuadTree.
        /// Used in a list initialisation to pre-allocate a certain amount of values.
        /// </summary>
        private int previousCount = 32;

        /// <summary>
        /// Get the bounds of this QuadTree;
        /// </summary>
        public Rect Extent
        {
            get => root.Extent;
            set
            {
                root.Extent = value;
                ReIndex();
            }
        }

        /// <summary>
        /// Reindexes the entire quadtree. Very expensive operation.
        /// </summary>
        public void ReIndex()
        {
                var oldRoot = root;
                root = new Quadrant(Extent);
                AddRange(oldRoot.AllWithBounds());
        }

        /// <summary>
        /// Create a <see cref="QuadTree{T}"/>
        /// </summary>
        /// <param name="extent">The bounds of the <see cref="QuadTree{T}"/></param>
        public QuadTree(Rect extent)
        {
            root = new Quadrant(extent);
            Extent = extent;
        }

        /// <summary>
        /// Insert a <typeparamref name="T"/> item into the QuadTree.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="bounds"></param>
        public void Insert(T item, Rect bounds)
        {
            if (!root.Extent.Contains(bounds))
            {
                throw new ArgumentException($"{nameof(bounds)} of {nameof(item)} is greater than the extent of the quadtree.");
            }
            root.Insert(item, bounds);
        }

        /// <summary>
        /// Remove a <typeparamref name="T"/> item from the QuadTree.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="bounds"></param>
        public void Remove(T item, Rect bounds)
        {
            var quadrant = root.GetContainingQuadrant(bounds);
            quadrant.Remove((item, bounds));
        }

        /// <summary>
        /// Retrieves a sequence of items that intersect the given bounds.
        /// </summary>
        /// <param name="bounds">The bounds to check</param>
        /// <returns></returns>
        public IEnumerable<T> GetItemsIntersecting(Rect bounds)
        {
            var items = new List<T>(previousCount);
            root.GetItemsIntersecting(items, bounds);
            previousCount = items.Count; 
            return items;
        }

        /// <summary>
        /// Retrieves all the items from the <see cref="QuadTree{T}"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> All()
        {
            return root.All();
        }

        public IEnumerable<T> FindAll(Predicate<T> match)
        {
            if (match is null)
            {
                throw new ArgumentNullException($"{nameof(match)}");
            }
            var list = new List<T>();
            foreach (var item in this)
            {
                if (match(item))
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public T Find(Predicate<T> match)
        {
            if (match is null)
            {
                throw new ArgumentNullException($"{nameof(match)}");
            }
            foreach (var item in this)
            {
                if (match(item))
                {
                    return item;
                }
            }
            return default;
        }

        public bool Exists(Predicate<T> match)
        {
            if (match is null)
            {
                throw new ArgumentNullException($"{nameof(match)}");
            }
            foreach (var item in this)
            {
                if (match(item))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddRange(IEnumerable<(T item, Rect bounds)> collection)
        {
            foreach (var item in collection)
            {
                Insert(item.item, item.bounds);
            }
        }

        /// <summary>
        /// Removes all items from the QuadTree.
        /// </summary>
        public void Clear()
        {
            root = new Quadrant(Extent);
            GC.Collect();
        }

        /// <summary>
        /// Retrieves the number of items in the QuadTree.
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return root.Count();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return root.All().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return root.All().GetEnumerator();
        }
#if DEBUG
        /// <summary>
        /// Retrieves a list of Rects that make up the Quadrants in this QuadTree.
        /// Used when debugging to draw the QuadTree quadrants to a canvas.
        /// </summary>
        /// <returns></returns>
        public List<Rect> GetQuadrantRects()
        {
            var rects = new List<Rect>(20)
            {
                root.Extent
            };
            root.GetQuadrantRects(rects);
            return rects;
        }
#endif
    }
}
