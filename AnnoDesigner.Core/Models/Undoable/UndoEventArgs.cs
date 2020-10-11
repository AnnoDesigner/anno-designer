using System;
using System.Linq;
using System.Collections.Generic;

namespace AnnoDesigner.Core.Models.Undoable
{
    public class UndoEventArgs : EventArgs
    {
        public List<UndoEventArgs> Chain { get; set; }

        public void AddToChain(UndoEventArgs args)
        {
            Chain ??= new List<UndoEventArgs>();
            Chain.Add(args);
        }

        public virtual void Undo()
        {
            if (Chain != null)
                foreach (var item in Enumerable.Reverse(Chain))
                    item.Undo();
        }

        public virtual void Redo()
        {
            if (Chain != null)
                foreach (var item in Chain)
                    item.Redo();
        }
    }

    public class PropertyUndoEventArgs : UndoEventArgs
    {
        public IUndoable RefObject { get; set; }

        public string PropertyName { get; set; }

        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public override void Undo()
        {
            base.Undo();

            var undoing = RefObject.Undoing;
            RefObject.Undoing = true;
            if (PropertyName != null)
            {
                var property = RefObject.GetType().GetProperty(PropertyName);
                if (property != null)
                {
                    property.SetValue(RefObject, OldValue);
                }
            }
            RefObject.Undoing = undoing;
        }

        public override void Redo()
        {
            var undoing = RefObject.Undoing;
            RefObject.Undoing = true;
            if (PropertyName != null)
            {
                var property = RefObject.GetType().GetProperty(PropertyName);
                if (property != null)
                {
                    property.SetValue(RefObject, NewValue);
                }
            }
            RefObject.Undoing = undoing;

            base.Redo();
        }

        public override string ToString()
        {
            return $"Undo {RefObject} {PropertyName} from {OldValue ?? "null"} to {NewValue ?? "null"} and other {Chain?.Count ?? 0} actions";
        }
    }

    public class CollectionUndoEventArgs<T> : UndoEventArgs
    {
        public IUndoableCollection<T> RefObject { get; set; }

        public T Item { get; set; }

        public bool ItemWasAdded { get; set; }

        public override void Undo()
        {
            base.Undo();

            var undoing = RefObject.Undoing;
            RefObject.Undoing = true;
            if (ItemWasAdded)
            {
                RefObject.Remove(Item);
            }
            else
            {
                RefObject.Add(Item);
            }
            RefObject.Undoing = undoing;
        }

        public override void Redo()
        {
            var undoing = RefObject.Undoing;
            RefObject.Undoing = true;
            if (ItemWasAdded)
            {
                RefObject.Add(Item);
            }
            else
            {
                RefObject.Remove(Item);
            }
            RefObject.Undoing = undoing;

            base.Redo();
        }

        public override string ToString()
        {
            if (ItemWasAdded)
                return $"Undo adding {Item} into {RefObject} and other {Chain?.Count ?? 0} actions";
            return $"Undo removing {Item} from {RefObject} and other {Chain?.Count ?? 0} actions";
        }
    }
}
