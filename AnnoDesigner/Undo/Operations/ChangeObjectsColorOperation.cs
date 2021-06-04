using System;
using System.Collections.Generic;
using System.Windows.Media;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;

namespace AnnoDesigner.Undo.Operations
{
    public class ChangeObjectsColorOperation : IOperation
    {
        public IEnumerable<(LayoutObject obj, SerializableColor oldColor, Color? newColor)> ObjectColors { get; set; }

        public Action RedrawAction { get; set; }

        public void Undo()
        {
            foreach (var (obj, oldColor, _) in ObjectColors)
            {
                obj.Color = oldColor;
            }

            RedrawAction?.Invoke();
        }

        public void Redo()
        {
            foreach (var (obj, _, newColor) in ObjectColors)
            {
                obj.Color = newColor.Value;
            }

            RedrawAction?.Invoke();
        }
    }
}
