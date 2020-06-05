using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AnnoDesigner.Core.Controls;

namespace AnnoDesigner.Core.CustomEventArgs
{
    public class ActionRecorderEventArgs : EventArgs
    {
        public static readonly new ActionRecorderEventArgs Empty = new ActionRecorderEventArgs(Key.None, MouseAction.None, ModifierKeys.None, ActionRecorder.ActionRecorderResult.None);

        public Key Key { get; }
        public MouseAction Action { get; }
        public ModifierKeys Modifiers { get; }
        public ActionRecorder.ActionRecorderResult Result { get; }

        private ActionRecorderEventArgs() { }
        public ActionRecorderEventArgs(Key key, MouseAction action, ModifierKeys modifiers, ActionRecorder.ActionRecorderResult result)
        {
            Key = key;
            Action = action;
            Modifiers = modifiers;
            Result = result;
        }
    }
}
