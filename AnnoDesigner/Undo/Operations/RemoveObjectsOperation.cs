using System.Collections.Generic;
using AnnoDesigner.Core.DataStructures;
using AnnoDesigner.Models;

namespace AnnoDesigner.Undo.Operations
{
    public class RemoveObjectsOperation : IOperation
    {
        public IEnumerable<LayoutObject> Objects { get; set; }

        public QuadTree<LayoutObject> Collection { get; set; }

        public void Undo()
        {
            OperationHelper.AddObjects(Collection, Objects);
        }

        public void Redo()
        {
            OperationHelper.RemoveObjects(Collection, Objects);
        }
    }
}
