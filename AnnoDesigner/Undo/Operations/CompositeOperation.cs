using System.Collections.Generic;
using System.Linq;

namespace AnnoDesigner.Undo.Operations
{
    public class CompositeOperation : IOperation
    {
        public ICollection<IOperation> Operations { get; set; } = new List<IOperation>();

        public void Redo()
        {
            foreach (var reducer in Operations)
            {
                reducer.Redo();
            }
        }

        public void Undo()
        {
            foreach (var reducer in Operations.Reverse())
            {
                reducer.Undo();
            }
        }
    }
}
