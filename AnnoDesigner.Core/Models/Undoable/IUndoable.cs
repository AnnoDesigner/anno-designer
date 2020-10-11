using System.Collections.Generic;

namespace AnnoDesigner.Core.Models.Undoable
{
    public delegate void UndoEventHandler(UndoEventArgs e);

    public interface IUndoable
    {
        bool Undoing { get; set; }

        event UndoEventHandler UndoableAction;
    }

    public interface IUndoableCollection<T> : ICollection<T>, IUndoable
    {
        UndoableCollectionActions ReportedActions { get; set; }
    }
}
