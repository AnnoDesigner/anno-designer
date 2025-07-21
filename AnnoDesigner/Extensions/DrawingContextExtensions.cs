using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AnnoDesigner.Extensions
{
    public static class DrawingContextExtensions
    {
        public static void DrawRectangleRotated(this DrawingContext drawingContext, Brush brush, Pen pen, Rect rect, float rotationDegrees)
        {
            drawingContext.PushTransform(new RotateTransform(rotationDegrees, rect.Left + (rect.Width / 2), rect.Top + (rect.Height / 2f)));
            drawingContext.DrawRectangle(brush, pen, rect);
            drawingContext.Pop();
        }
    }
}
