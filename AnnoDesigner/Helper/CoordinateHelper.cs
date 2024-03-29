﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;

namespace AnnoDesigner.Helper
{
    public class CoordinateHelper : ICoordinateHelper
    {
        /// <summary>
        /// Convert a screen coordinate to a grid coordinate by determining in which grid cell the point is contained.
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public Point ScreenToGrid(Point screenPoint, int gridStep)
        {
            return new Point(Math.Floor(screenPoint.X / gridStep), Math.Floor(screenPoint.Y / gridStep));
        }

        /// <summary>
        /// Convert a screen coordinate to a grid coordinate without flooring to the nearest full coordinate
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public Point ScreenToFractionalGrid(Point screenPoint, int gridStep)
        {
            return new Point(screenPoint.X / gridStep, screenPoint.Y / gridStep);
        }

        /// <summary>
        /// Converts a screen coordinate to a grid coordinate by determining which grid cell is nearest.
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public Point RoundScreenToGrid(Point screenPoint, int gridStep)
        {
            return new Point(Math.Round(screenPoint.X / gridStep), Math.Round(screenPoint.Y / gridStep));
        }

        /// <summary>
        /// Converts a length given in (pixel-)units to grid coordinate by determining which grid edge is nearest.
        /// </summary>
        /// <param name="screenLength"></param>
        /// <returns></returns>
        public double RoundScreenToGrid(double screenLength, int gridStep)
        {
            return Math.Round(screenLength / gridStep);
        }

        /// <summary>
        /// Converts a length given in (pixel-)units to a length given in grid cells.
        /// </summary>
        /// <param name="screenLength"></param>
        /// <returns></returns>
        public double ScreenToGrid(double screenLength, int gridStep)
        {
            return screenLength / gridStep;
        }

        /// <summary>
        /// Converts a length given in (pixel-)units to a length given in grid cells.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="gridStep"></param>
        public Rect ScreenToGrid(Rect rect, int gridStep)
        {
            return new Rect(rect.Location.X / gridStep, rect.Location.Y / gridStep, rect.Width / gridStep, rect.Height / gridStep);
        }

        /// <summary>
        /// Convert a grid coordinate to a screen coordinate.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="gridStep"></param>
        public Rect GridToScreen(Rect rect, int gridStep)
        {
            return new Rect(rect.Location.X * gridStep, rect.Location.Y * gridStep, rect.Width * gridStep, rect.Height * gridStep);
        }

        /// <summary>
        /// Convert a grid coordinate to a screen coordinate.
        /// </summary>
        /// <param name="gridPoint"></param>
        /// <returns></returns>
        public Point GridToScreen(Point gridPoint, int gridStep)
        {
            return new Point(gridPoint.X * gridStep, gridPoint.Y * gridStep);
        }

        /// <summary>
        /// Converts a size given in grid cells to a size given in (pixel-)units.
        /// </summary>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public Size GridToScreen(Size gridSize, int gridStep)
        {
            return new Size(gridSize.Width * gridStep, gridSize.Height * gridStep);
        }

        /// <summary>
        /// Converts a length given in grid cells to a length given in (pixel-)units.
        /// </summary>
        /// <param name="gridLength"></param>
        /// <returns></returns>
        public double GridToScreen(double gridLength, int gridStep)
        {
            return gridLength * gridStep;
        }

        /// <summary>
        /// Calculates the exact center point of a given rect
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public Point GetCenterPoint(Rect rect)
        {
            var pos = rect.Location;
            var size = rect.Size;
            pos.X += size.Width / 2;
            pos.Y += size.Height / 2;
            return pos;
        }

        /// <summary>
        /// Rotates the given Size object, i.e. switches width and height.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public Size Rotate(Size size)
        {
            return new Size(size.Height, size.Width);
        }

        /// <summary>
        /// Rotates the given Rect object 90 degrees clockwise around point (0, 0).
        /// </summary>
        public Rect Rotate(Rect rect)
        {
            var position = rect.TopLeft;
            //Full formula left in for explanation
            //var xPrime = x * Math.Cos(angle) - y * Math.Sin(angle);
            //var yPrime = x * Math.Sin(angle) - y * Math.Cos(angle);

            //Cos 90 = 0, sin 90 = 1
            //Therefore, the below is equivalent
            var xPrime = 0 - position.Y;
            var yPrime = position.X;

            //When the building is rotated, the xPrime and yPrime values no 
            //longer represent the top left corner, they will represent the 
            //top-right corner instead. We need to account for this, by 
            //moving the xPrime position (still in grid coordinates).
            xPrime -= rect.Size.Height;

            return new Rect(new Point(xPrime, yPrime), Rotate(rect.Size));
        }

        /// <summary>
        /// Rotates the given GridDirection object 90 degrees clockwise.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public GridDirection Rotate(GridDirection direction)
        {
            return (GridDirection)((int)(direction + 1) % 4);
        }
    }
}
