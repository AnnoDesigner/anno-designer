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
        public Key Key;
        public MouseAction MouseAction;
        public ModifierKeys Modifiers;
        public Type BindingType;
    }
}
