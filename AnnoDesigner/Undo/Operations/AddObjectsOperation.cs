using System.Collections.Generic;
using AnnoDesigner.Core.DataStructures;
using AnnoDesigner.Models;

namespace AnnoDesigner.Undo.Operations
{
    public class AddObjectsOperation : IOperation
    {
        public IEnumerable<LayoutObject> Objects { get; set; }

        public QuadTree<LayoutObject> Collection { get; set; }

        public void Undo()
        {
            OperationHelper.RemoveObjects(Collection, Objects);
        }

        public void Redo()
        {
            OperationHelper.AddObjects(Collection, Objects);
        }
    }
}
