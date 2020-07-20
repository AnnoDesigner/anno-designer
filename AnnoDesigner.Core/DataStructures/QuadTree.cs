using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoDesigner.Core.DataStructures
{
    public class QuadTree<T>
    {
        private class Quadrant
        {

            bool hasSubdivisions;

            Quadrant topRight;
            Quadrant topLeft;
            Quadrant bottomRight;
            Quadrant bottomLeft;

            /// <summary>
            /// Stored as a list of items as we could have multiple items that overlap child quadrants
            /// </summary>
            readonly List<(T item, Rect bounds)> items;

            public Rect Extent { get; set; }

            public Quadrant(Rect extent)
            {
                Extent = extent;
                hasSubdivisions = false;
                items = new List<(T, Rect)>();
            }

            public void Subdivide()
            {
                var w = Extent.Width / 2;
                var h = Extent.Height / 2;
                
                var topRight = new Rect(Extent.Left + w, Extent.Top, w, h);
                var topLeft = new Rect(Extent.Left, Extent.Top, w, h);
                var bottomRight = new Rect(Extent.Left + w, Extent.Top + h, w, h);
                var bottomLeft = new Rect(Extent.Left, Extent.Top + h, w, h);

                this.topRight = new Quadrant(topRight);
                this.topLeft = new Quadrant(topLeft);
                this.bottomRight = new Quadrant(bottomRight);
                this.bottomLeft = new Quadrant(bottomLeft);

                hasSubdivisions = true;
            }

            public void Insert(T item, Rect bounds)
            {
                if (!Extent.IntersectsWith(bounds))
                {
                    return; //item does not belong in quadrant
                }

                //Otherwise, subdivide
                if (!hasSubdivisions)
                {
                    Subdivide();
                }

                var inserted = false;
                if (topRight.Extent.Contains(bounds))
                {
                    topRight.Insert(item, bounds);
                    inserted = true;
                }
                else if (topLeft.Extent.Contains(bounds))
                {
                    topLeft.Insert(item, bounds);
                    inserted = true;
                }
                else if (bottomRight.Extent.Contains(bounds))
                {
                    bottomRight.Insert(item, bounds);
                    inserted = true;
                }
                else if (bottomLeft.Extent.Contains(bounds))
                {
                    bottomLeft.Insert(item, bounds);
                    inserted = true;
                }

                if (!inserted)
                {
                    //if not inserted into a sub-quadrant add the item here as it intersects several quadrants
                    items.Add((item, bounds));
                }
            }

            public IEnumerable<T> GetItemsIntersecting(Rect bounds)
            {
                var items = new List<T>();
                if (bounds.IntersectsWith(Extent))
                {
                    if (hasSubdivisions)
                    {
                        items.AddRange(topRight.GetItemsIntersecting(bounds));
                        items.AddRange(topLeft.GetItemsIntersecting(bounds));
                        items.AddRange(bottomRight.GetItemsIntersecting(bounds));
                        items.AddRange(bottomLeft.GetItemsIntersecting(bounds));
                    }
                    items.AddRange(this.items.Select(_ => _.item));
                }
                return items;
            }
        }

        /// <summary>
        /// The root of the <see cref="QuadTree{T}"/>
        /// </summary>
        readonly Quadrant root;
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
                //perform reindex
            }
        }

        /// <summary>
        /// Create a <see cref="QuadTree{T}"/>
        /// </summary>
        /// <param name="extent">The size of the <see cref="QuadTree{T}"/></param>
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

        public IEnumerable<T> GetItemsIntersecting(Rect bounds)
        {
            return root.GetItemsIntersecting(bounds);
        }
    }
}
