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
    public class QuadTree<T> : IEnumerable<T>
    {
        public class Quadrant
        {
            /// <summary>
            /// Represents if a new item has been added between updating metadata.
            /// If false and metadata is accessed, the metadata needs to be refreshed.
            /// </summary>
            bool isDirty = false;

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
            /// A count of all items in this Quadrant
            /// </summary>
            int count;
            /// <summary>
            /// A list of all the items in the Quadrant and under it.
            /// </summary>
            IEnumerable<T> itemCache;

            /// <summary>
            /// Stored as a list of items as we could have multiple items that overlap child quadrants
            /// </summary>
            readonly List<(T item, Rect bounds)> items;

            public Rect Extent { get; set; }

            public Quadrant(Rect extent)
            {
                Extent = extent;
                items = new List<(T, Rect)>();
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
                    items.Add((item, bounds));
                }
                else
                {
                    childQuadrant.Parent = this;
                    childQuadrant.Insert(item, bounds);
                }

                isDirty = true;
            }

            public void MarkAsDirty()
            {
                if (Parent != null)
                {
                    Parent.MarkAsDirty();
                }
                isDirty = true;
            }

            public IEnumerable<T> GetItemsIntersecting(Rect bounds)
            {
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
                    return All();
                }
            }

            public int Count()
            {
                if (isDirty)
                {
                    UpdateMetadata();
                }
                return count;
            }

            public IEnumerable<T> All()
            {
                if (isDirty)
                {
                    UpdateMetadata();
                }
                return itemCache;
            }

            public void AddRange(IEnumerable<(T item, Rect bounds)> collection)
            {
                foreach (var item in collection)
                {
                    Insert(item.item, item.bounds);
                }
            }

            public void UpdateMetadata()
            {
                //initialise with the outdated count value
                var items = new List<T>(count);
                var empty = new List<T>(0);
                    items.AddRange(topLeft?.All() ?? empty);
                    items.AddRange(topRight?.All() ?? empty);
                    items.AddRange(bottomRight?.All() ?? empty);
                    items.AddRange(bottomLeft?.All() ?? empty);
                items.AddRange(this.items.Select(obj => obj.item));
                count = items.Count;
                itemCache = items;
                isDirty = false;
            }
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

        public void Insert(T item, Rect bounds)
        {
            if (!root.Extent.Contains(bounds))
            {
                throw new ArgumentException($"{nameof(bounds)} of {nameof(item)} is greater than the extent of the quadtree.");
            }
            root.Insert(item, bounds);
        }

        public void Remove(T item)
        {
            //TODO: Implement this
            //no-op
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
    }
}
