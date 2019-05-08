using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner
{
    public class CanvasAction
    {
        public List<AnnoObject> Objects { get; set; }
        public ActionType Action { get; set; }

        public CanvasAction(List<AnnoObject> objects, ActionType action)
        {
            Objects = objects;
            Action = action;
        }

        public CanvasAction(AnnoObject obj, ActionType action)
        {
            Objects = new List<AnnoObject>() { obj };
            Action = action;
        }
    }

    public enum ActionType
    {
        Add, Remove, Move, AddGroup, RemoveGroup, MoveGroup
    }


}
