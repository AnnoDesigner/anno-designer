using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoDesigner.Models
{
    /// <summary>
    /// Holds a <see cref="Key"/> or <see cref="MouseAction"/> mapping for a hotkey.
    /// </summary>
    [Serializable]
    public class HotkeyInformation
    {
        public HotkeyInformation() { }
        public HotkeyInformation(Key key) : this (key, default, ModifierKeys.None, GestureType.KeyGesture) { }
        public HotkeyInformation(ExtendedMouseAction mouseAction) : this (default, mouseAction, ModifierKeys.None, GestureType.MouseGesture) { }
        public HotkeyInformation(Key key, ModifierKeys modifiers) : this (key, default, modifiers, GestureType.KeyGesture) { }
        public HotkeyInformation(ExtendedMouseAction mouseAction, ModifierKeys modifiers) : this (default, mouseAction, modifiers, GestureType.MouseGesture) { }
        public HotkeyInformation(Key key, ExtendedMouseAction mouseAction, ModifierKeys modifiers, GestureType gestureType)
        {
            Key = key;
            MouseAction = mouseAction;
            Modifiers = modifiers;
            Type = gestureType;
        }

        public Key Key { get; set; }
        public ExtendedMouseAction MouseAction { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public GestureType Type { get; set; }
    }

}
