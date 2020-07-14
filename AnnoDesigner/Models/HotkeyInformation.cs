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
        public HotkeyInformation(Key key) : this (key, default, ModifierKeys.None, typeof(KeyBinding)) { }
        public HotkeyInformation(MouseAction mouseAction) : this (default, mouseAction, ModifierKeys.None, typeof(MouseBinding)) { }
        public HotkeyInformation(Key key, ModifierKeys modifiers) : this (key, default, modifiers, typeof(KeyBinding)) { }
        public HotkeyInformation(MouseAction mouseAction, ModifierKeys modifiers) : this (default, mouseAction, modifiers, typeof(MouseBinding)) { }
        public HotkeyInformation(Key key, MouseAction mouseAction, ModifierKeys modifiers, Type bindingType)
        {
            Key = key;
            MouseAction = mouseAction;
            Modifiers = modifiers;
            BindingType = bindingType;
        }

        public Key Key { get; set; }
        public MouseAction MouseAction { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public Type BindingType { get; set; }
    }

}
