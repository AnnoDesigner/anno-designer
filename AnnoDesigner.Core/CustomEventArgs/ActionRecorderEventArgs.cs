using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AnnoDesigner.Core.Controls;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.CustomEventArgs;

public class ActionRecorderEventArgs : EventArgs
{
    public static readonly new ActionRecorderEventArgs Empty = new ActionRecorderEventArgs(Key.None, ExtendedMouseAction.None, ModifierKeys.None, ActionRecorder.ActionType.None);

    public Key Key { get; }
    public ExtendedMouseAction Action { get; }
    public ModifierKeys Modifiers { get; }
    public ActionRecorder.ActionType ResultType { get; }

    private ActionRecorderEventArgs() { }
    public ActionRecorderEventArgs(Key key, ExtendedMouseAction action, ModifierKeys modifiers, ActionRecorder.ActionType resultType)
    {
        Key = key;
        Action = action;
        Modifiers = modifiers;
        ResultType = resultType;
    }
}
