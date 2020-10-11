using System;

namespace AnnoDesigner.Core.Models.Undoable
{
  [Flags]
  public enum UndoableCollectionActions
  {
    None = 0,
    Add = 1 << 0,
    Remove = 1 << 1,
    /// <summary>
    /// Propagate undoable events of collection items
    /// </summary>
    Items = 1 << 2,
    AddRemove = Add | Remove,
    All = ~None
  }
}
