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
        public class Quadrant
        {
            /// <summary>
            /// Indicates if current cached data (<see cref="count"/> and <see cref="itemCache"/>) is up to date.
            /// Returns <see langword="true"/> when cached data is up to date.
            /// </summary>
            bool isDirty;

            Quadrant topRight;
            Quadrant topLeft;
            Quadrant bottomRight;
            Quadrant bottomLeft;

            readonly Rect topRightBounds;
            readonly Rect topLeftBounds;
            readonly Rect bottomRightBounds;
            readonly Rect bottomLeftBounds;

            /// <summary>
            /// A reference to the parent <see cref="Quadrant"/> for this Quadrant.
            /// </summary>
            public Quadrant Parent { get; set; }

            /// <summary>
            /// A count of all items in this Quadrant and under it.
            /// </summary>
            int count;
            /// <summary>
            /// A list of all the items in the Quadrant and under it.
            /// </summary>
            IEnumerable<T> itemCache;

            /// <summary>
            /// Holds a list of all the items in this quadrant.
            /// </summary>
            /// <remarks>
            /// Stored as a list as items as any items that overlap multiple child quadrants will be stored here.
            /// </remarks>
            public List<(T item, Rect bounds)> Items { get; }

            public Rect Extent { get; set; }

            public Quadrant(Rect extent)
            {
                Extent = extent;
                Items = new List<(T, Rect)>();
                itemCache = new List<T>();
                isDirty = false;

                var w = Extent.Width / 2;
                var h = Extent.Width / 2;

                topRightBounds = new Rect(Extent.Left + w, Extent.Top, w, h);
                topLeftBounds = new Rect(Extent.Left, Extent.Top, w, h);
                bottomRightBounds = new Rect(Extent.Left + w, Extent.Top + h, w, h);
                bottomLeftBounds = new Rect(Extent.Left, Extent.Top + h, w, h);
            }

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
            public IEnumerable<T> GetItemsIntersecting(Rect bounds)
            {
                //TODO: PR: Currently bugged when object rects span more than the width of the parent.
                //Fix: Build up the items list recursively upwards from the lowest quadrant level, until
                //we reach a quadrant that has no intersecting items.

                if (topRight != null && topRight.Extent.Contains(bounds))
                {
                    return topRight.GetItemsIntersecting(bounds);
                }
                else if (topLeft != null && topLeft.Extent.Contains(bounds))
                {
                    return topLeft.GetItemsIntersecting(bounds);
                }
                else if (bottomRight != null && bottomRight.Extent.Contains(bounds))
                {
                    return bottomRight.GetItemsIntersecting(bounds);
                }
                else if (bottomLeft != null && bottomLeft.Extent.Contains(bounds))
                {
                    return bottomLeft.GetItemsIntersecting(bounds);
                }
                else
                {
                    //Only add all items if the bounds are not completely within a sub quadrant
                    var items = All();
                    //also add parent items if they also intersect with the current bounds, as there could be items that span several quadrants.
                    items = items.Concat(Parent?.Items?.Where(_ => _.bounds.IntersectsWith(bounds))?.Select(_ => _.item) ?? new List<T>(0));
                    return items;
                }
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
                //TODO: PR: Make this lazy - then we can effectively keep a specific framerate and and keep track of how much we rendered last frame
                //Might not make it into this PR - could just be a goal for a future PR.
                if (isDirty)
                {
                    UpdateCachedData();
                }
                return itemCache;
            }

            /// <summary>
            /// Updates the cahced data for this quadrant.
            /// </summary>
            private void UpdateCachedData()
            {
                //initialise with the outdated count value
                var newItems = new List<T>(count);
                var empty = new List<T>(0);
                newItems.AddRange(topLeft?.All() ?? empty);
                newItems.AddRange(topRight?.All() ?? empty);
                newItems.AddRange(bottomRight?.All() ?? empty);
                newItems.AddRange(bottomLeft?.All() ?? empty);
                newItems.AddRange(Items.Select(obj => obj.item));
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
        Quadrant root;
        Rect _extent;

        /// <summary>
        /// Get the bounds of this QuadTree;
        /// </summary>
        public Rect Extent
        {
            get => _extent;
            set
            {
                _extent = value;
                //TODO: PR: perform reindex
            }
        }

        /// <summary>
        /// Create a <see cref="QuadTree{T}"/>
        /// </summary>
        /// <param name="extent">The bounds of the <see cref="QuadTree{T}"/></param>
        public QuadTree(Rect extent)
        {
            Extent = extent;
            root = new Quadrant(extent);
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
            return root.GetItemsIntersecting(bounds) ?? new List<T>();
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
            //TODO: PR: See if this is necessary.
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
            var rects = new List<Rect>(20);
            rects.Add(root.Extent);
            root.GetQuadrantRects(rects);
            return rects;
        }
#endif
    }
}
