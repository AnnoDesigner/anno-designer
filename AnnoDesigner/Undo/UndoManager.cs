using System;
using System.Collections.Generic;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Undo.Operations;

namespace AnnoDesigner.Undo
{
    public class UndoManager : Notify, IUndoManager
    {
        internal Stack<IOperation> UndoStack { get; set; } = new Stack<IOperation>();
        internal Stack<IOperation> RedoStack { get; set; } = new Stack<IOperation>();
        internal CompositeOperation CurrentCompositeOperation { get; set; }

        private IOperation lastUndoableOperation;

        public bool IsDirty
        {
            get { return lastUndoableOperation != (UndoStack.Count > 0 ? UndoStack.Peek() : null); }
            set
            {
                if (value)
                {
                    throw new ArgumentException("You can only set IsDirty to false");
                }

                lastUndoableOperation = UndoStack.Count > 0 ? UndoStack.Peek() : null;
                OnPropertyChanged(nameof(IsDirty));
            }
        }

        private bool Undoing { get; set; }

        public void Undo()
        {
            if (UndoStack.Count > 0)
            {
                var wasUndoing = Undoing;
                Undoing = true;
                var operation = UndoStack.Pop();
                operation.Undo();
                RedoStack.Push(operation);
                OnPropertyChanged(nameof(IsDirty));
                Undoing = wasUndoing;
            }
        }

        public void Redo()
        {
            if (RedoStack.Count > 0)
            {
                var wasUndoing = Undoing;
                Undoing = true;
                var operation = RedoStack.Pop();
                operation.Redo();
                UndoStack.Push(operation);
                OnPropertyChanged(nameof(IsDirty));
                Undoing = wasUndoing;
            }
        }

        public void Clear()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            IsDirty = false;
        }

        public void RegisterOperation(IOperation operation)
        {
            if (Undoing) return;

            if (CurrentCompositeOperation != null)
            {
                CurrentCompositeOperation.Operations.Add(operation);
            }
            else
            {
                UndoStack.Push(operation);
                RedoStack.Clear();
                OnPropertyChanged(nameof(IsDirty));
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
