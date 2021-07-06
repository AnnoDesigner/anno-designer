using System.Collections.Generic;
using System.Linq;

namespace AnnoDesigner.Undo.Operations
{
    public class CompositeOperation : BaseOperation
    {
        public ICollection<IOperation> Operations { get; set; } = new List<IOperation>();

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
