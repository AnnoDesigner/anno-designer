using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AnnoDesigner
{
    /// <summary>
    /// Encapsulates properties about the current viewport, and provides methods to convert to and from origin and viewport coordinates.
    /// </summary>
    public class Viewport
    {
        /// <summary>
        /// The top offset of the viewport.
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// The Left offset of the viewport.
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// The Width of the viewport.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// The Height of the viewport.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Retrieve a <see cref="Rect"/> that represents the current viewport, relative to the origin.
        /// </summary>
        public Rect Relative
        {
            //the viewport-relative rect needs to reflect the new position of 0,0 in relation to the Top and Left properties. 
            //e.g with no translation, the top left corner of the viewport is 0,0.
            //if we've shifted everything 5 units in the X direction and 5 units in the y direction, the top left corner of the viewport
            //is -5, -5.
            get => new Rect(-Left, -Top, Width, Height);
        }
        
        /// <summary>
        /// Retrieve a <see cref="Rect"/> that represents the current viewport.
        /// </summary>
        public Rect Absolute
        {
            get => new Rect(Left, Top, Width, Height);
        }

        /// <summary>
        /// Converts a <see cref="System.Windows.Rect"/> that is relative to the origin to one that is relative to the viewport.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public Rect OriginToViewport(Rect rect)
        {
            return new Rect(rect.X - Left, rect.Y - Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// Converts a <see cref="Point"/> that is relative to the origin to one that is relative to the viewport.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public Point OriginToViewport(Point point)
        {
            return new Point(point.X - Left, point.Y - Top);
        }
    }
}
