using System.Collections.Generic;
using System.Linq;

namespace AnnoDesigner.Undo.Operations;

public class CompositeOperation : BaseOperation
{
    public ICollection<IOperation> Operations { get; set; } = [];

    protected override void RedoOperation()
    {
        foreach (IOperation curOperation in Operations)
        {
            curOperation.Redo();
        }
    }

    protected override void UndoOperation()
    {
        foreach (IOperation curOperation in Operations.Reverse())
        {
            curOperation.Undo();
        }
    }
}
