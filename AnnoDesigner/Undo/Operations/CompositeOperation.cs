using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnnoDesigner.Undo.Operations
{
    public class CompositeOperation : BaseOperation, ICollection<IOperation>
    {
        private ICollection<IOperation> operations = new List<IOperation>();

        public IEnumerable<IOperation> Operations => operations;

        public int Count => operations.Count;

        public bool IsReadOnly => operations.IsReadOnly;

        public CompositeOperation()
        {
            operations = new List<IOperation>();
        }

        public CompositeOperation(IEnumerable<IOperation> ops)
        {
            operations = new List<IOperation>(ops);
        }

        public void Add(IOperation item)
        {
            operations.Add(item);
        }

        public void Clear()
        {
            operations.Clear();
        }

        public bool Contains(IOperation item)
        {
            return operations.Contains(item);
        }

        public void CopyTo(IOperation[] array, int arrayIndex)
        {
            operations.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IOperation> GetEnumerator()
        {
            return operations.GetEnumerator();
        }

        public bool Remove(IOperation item)
        {
            return operations.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)operations).GetEnumerator();
        }

        protected override void RedoOperation()
        {
            foreach (var curOperation in Operations)
            {
                curOperation.Redo();
            }
        }

        protected override void UndoOperation()
        {
            foreach (var curOperation in Operations.Reverse())
            {
                curOperation.Undo();
            }
        }
    }
}
