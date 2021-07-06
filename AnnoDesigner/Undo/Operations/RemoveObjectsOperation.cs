using System.Collections.Generic;

namespace AnnoDesigner.Undo.Operations
{
    public class RemoveObjectsOperation<T> : BaseOperation
    {
        public IEnumerable<T> Objects { get; set; }

        public ICollection<T> Collection { get; set; }

        protected override void UndoOperation()
        {
            foreach (var obj in Objects)
            {
                Collection.Add(obj);
            }
        }

        protected override void RedoOperation()
        {
            foreach (var obj in Objects)
            {
                Collection.Remove(obj);
            }
        }
    }
}
