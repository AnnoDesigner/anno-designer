using System.Collections.Generic;
using System.Linq;

namespace AnnoDesigner.Undo.Operations
{
    public class CompositeOperation : IOperation
    {
        public ICollection<IOperation> Operations { get; set; } = new List<IOperation>();

        public void Redo()
        {
            foreach (var curOperation in Operations)
            {
                curOperation.Redo();
            }
        }

        public void Undo()
        {
            foreach (var curOperation in Operations.Reverse())
            {
                curOperation.Undo();
            }
        }
    }
}
