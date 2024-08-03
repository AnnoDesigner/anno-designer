using AnnoDesigner.Core.DataStructures;
using AnnoDesigner.Core.Models;
using System.Windows;

namespace AnnoDesigner.Undo.Operations;

public class MoveObjectsOperation<T> : ModifyObjectPropertiesOperation<T, Rect>
    where T : IBounded
{
    public IQuadTree<T> QuadTree { get; set; }

    public MoveObjectsOperation()
    {
        PropertyName = nameof(IBounded.Bounds);
        AfterUndoAction = AfterUndo;
        AfterRedoAction = AfterRedo;
    }

    private void AfterUndo()
    {
        foreach ((T obj, Rect _, Rect newRect) in ObjectPropertyValues)
        {
            QuadTree.ReIndex(obj, newRect);
        }
    }

    private void AfterRedo()
    {
        foreach ((T obj, Rect oldRect, Rect _) in ObjectPropertyValues)
        {
            QuadTree.ReIndex(obj, oldRect);
        }
    }
}
