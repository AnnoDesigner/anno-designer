using System;
using System.Collections.Generic;
using AnnoDesigner.Undo.Operations;

namespace AnnoDesigner.Undo
{
    public class UndoManager : IUndoManager
    {
        internal Stack<IOperation> UndoStack { get; set; } = new Stack<IOperation>();
        internal Stack<IOperation> RedoStack { get; set; } = new Stack<IOperation>();
        internal CompositeOperation CurrentCompositeOperation { get; set; }

        public void Undo()
        {
            if (UndoStack.Count > 0)
            {
                var operation = UndoStack.Pop();
                operation.Undo();
                RedoStack.Push(operation);
            }
        }

        public void Redo()
        {
            if (RedoStack.Count > 0)
            {
                var operation = RedoStack.Pop();
                operation.Redo();
                UndoStack.Push(operation);
            }
        }

        public void Clear()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }

        public void RegisterOperation(IOperation operation)
        {
            if (CurrentCompositeOperation != null)
            {
                CurrentCompositeOperation.Operations.Add(operation);
            }
            else
            {
                UndoStack.Push(operation);
                RedoStack.Clear();
            }
        }

        public void AsSingleUndoableOperation(Action action)
        {
            CurrentCompositeOperation = new CompositeOperation();
            CompositeOperation operation = null;
            try
            {
                action();
                operation = CurrentCompositeOperation;
            }
            finally
            {
                CurrentCompositeOperation = null;
                if (operation != null && operation.Operations.Count > 0)
                {
                    RegisterOperation(operation);
                }
            }
        }
    }
}
