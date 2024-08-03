using AnnoDesigner.Undo.Operations;
using System;

namespace AnnoDesigner.Undo;

public interface IUndoManager
{
    bool IsDirty { get; set; }

    void Undo();

    void Redo();

    void Clear();

    void RegisterOperation(IOperation operation);

    void AsSingleUndoableOperation(Action action);
}
