using System;
using AnnoDesigner.Undo.Operations;

namespace AnnoDesigner.Undo
{
    public interface IUndoManager
    {
        void Undo();

        void Redo();

        void Clear();

        void RegisterOperation(IOperation operation);

        void AsSingleUndoableOperation(Action action);
    }
}
