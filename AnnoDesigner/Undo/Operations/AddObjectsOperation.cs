using System.Collections.Generic;

namespace AnnoDesigner.Undo.Operations;

public class AddObjectsOperation<T> : BaseOperation
{
    public IEnumerable<T> Objects { get; set; }

    public ICollection<T> Collection { get; set; }

    protected override void UndoOperation()
    {
        foreach (T obj in Objects)
        {
            _ = Collection.Remove(obj);
        }
    }

    protected override void RedoOperation()
    {
        foreach (T obj in Objects)
        {
            Collection.Add(obj);
        }
    }
}
