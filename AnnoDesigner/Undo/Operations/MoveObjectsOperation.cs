using System.Collections.Generic;
using System.Windows;
using AnnoDesigner.Core.DataStructures;
using AnnoDesigner.Models;

namespace AnnoDesigner.Undo.Operations
{
    public class MoveObjectsOperation : IOperation
    {
        public IEnumerable<(LayoutObject obj, Point startPosition, Point endPosition)> ObjectPositions { get; set; }

        public QuadTree<LayoutObject> Collection { get; set; }

        public void Undo()
        {
            foreach (var (obj, startPosition, _) in ObjectPositions)
            {
                Collection.Remove(obj, obj.GridRect);
                obj.Position = startPosition;
                Collection.Insert(obj, obj.GridRect);
            }
        }

        public void Redo()
        {
            foreach (var (obj, _, endPosition) in ObjectPositions)
            {
                Collection.Remove(obj, obj.GridRect);
                obj.Position = endPosition;
                Collection.Insert(obj, obj.GridRect);
            }
        }
    }
}
