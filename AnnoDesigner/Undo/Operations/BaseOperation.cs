using System;

namespace AnnoDesigner.Undo.Operations
{
    public abstract class BaseOperation : IOperation
    {
        public Action AfterAction { get; set; }

        public Action AfterUndoAction { get; set; }

        public Action AfterRedoAction { get; set; }

        public void Undo()
        {
            UndoOperation();

            AfterUndoAction?.Invoke();
            AfterAction?.Invoke();
        }

        protected abstract void UndoOperation();

        public void Redo()
        {
            RedoOperation();

            AfterRedoAction?.Invoke();
            AfterAction?.Invoke();
        }

        protected abstract void RedoOperation();
    }
}
