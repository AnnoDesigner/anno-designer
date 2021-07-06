using System.Windows;
using AnnoDesigner.Core.DataStructures;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Undo.Operations
{
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
            foreach (var (obj, _, newRect) in ObjectPropertyValues)
            {
                QuadTree.ReIndex(obj, newRect);
            }
        }

        private void AfterRedo()
        {
            foreach (var (obj, oldRect, _) in ObjectPropertyValues)
            {
                QuadTree.ReIndex(obj, oldRect);
            }
        }
    }
}
