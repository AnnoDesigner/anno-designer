using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AnnoDesigner.Core.DataStructures;
using AnnoDesigner.Models;

namespace AnnoDesigner.Undo.Operations
{
    public static class OperationHelper
    {
        public static void AddObjects(QuadTree<LayoutObject> collection, IEnumerable<LayoutObject> objects)
        {
            collection.AddRange(objects.Select(o => (o, o.GridRect)));
        }

        public static void RemoveObjects(QuadTree<LayoutObject> collection, IEnumerable<LayoutObject> objects)
        {
            foreach (var obj in objects)
            {
                collection.Remove(obj, obj.GridRect);
            }
        }

        public static void RotateClockwise(QuadTree<LayoutObject> collection, IEnumerable<LayoutObject> objects)
        {
            foreach (var obj in objects)
            {
                collection.Remove(obj, obj.GridRect);
                obj.Position = new Point(-obj.Position.Y - obj.Size.Height, obj.Position.X);
                obj.Size = new Size(obj.Size.Height, obj.Size.Width);
                collection.Insert(obj, obj.GridRect);
            }
        }

        public static void RotateCounterClockwise(QuadTree<LayoutObject> collection, IEnumerable<LayoutObject> objects)
        {
            foreach (var obj in objects)
            {
                collection.Remove(obj, obj.GridRect);
                obj.Position = new Point(obj.Position.Y, -obj.Position.X - obj.Size.Width);
                obj.Size = new Size(obj.Size.Height, obj.Size.Width);
                collection.Insert(obj, obj.GridRect);
            }
        }
    }
}
