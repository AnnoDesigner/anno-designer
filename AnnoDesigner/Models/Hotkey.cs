using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Models
{
    /// <summary>
    /// Acts as a wrapper for a named <see cref="InputBinding"/>, and allows updating if the current Binding is replaced with a fresh reference.
    /// </summary>
    public class Hotkey : Notify
    {
        private Hotkey() { }
        public Hotkey(string hotkeyName, InputBinding binding):this(hotkeyName, binding, null) { }
        public Hotkey(string hotkeyName, InputBinding binding, string description)
        {
            Name = hotkeyName;
            Binding = binding;
            Description = description;

            if (binding is KeyBinding keyBinding)
            {
                defaultKey = keyBinding.Key;
                defaultModifiers = keyBinding.Modifiers;
                defaultType = typeof(KeyBinding);
            }
            else
            {
                var mouseBinding = binding as MouseBinding;
                defaultMouseAction = mouseBinding.MouseAction;
                defaultModifiers = (mouseBinding.Gesture as MouseGesture).Modifiers;
                defaultType = typeof(MouseBinding);
            }
        }

        private InputBinding _binding;
        public InputBinding Binding
        {
            get { return _binding; }
            set { UpdateProperty(ref _binding, value); }
        }

        private string _name;
        /// <summary>
        /// An identifier for the <see cref="Hotkey"/>, usually required to be unique.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { UpdateProperty(ref _name, value); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { UpdateProperty(ref _description, value); }
        }

        /// <summary>
        /// Resets a hotkey to its defaults.
        /// </summary>
        public void Reset()
        {
            var isKeyBinding = Binding is KeyBinding;
            var isCorrectType = Binding.GetType() == defaultType;
            if (isKeyBinding && isCorrectType)
            {
                var keyBinding = Binding as KeyBinding;
                keyBinding.Key = defaultKey;
                keyBinding.Modifiers = defaultModifiers;
            }
            else if (!isKeyBinding && isCorrectType)
            {
                var mouseBinding = Binding as MouseBinding;
                mouseBinding.MouseAction = defaultMouseAction;
                (mouseBinding.Gesture as MouseGesture).Modifiers = defaultModifiers;
            }
            else if (defaultType == typeof(KeyBinding))
            {
                //we currently have a MouseBinding and need a KeyBinding
                Binding = new KeyBinding()
                {
                    Key = defaultKey,
                    Modifiers = defaultModifiers,
                    Command = Binding.Command,
                    CommandParameter = Binding.CommandParameter
                };
            }
            else
            {
                //we currently have a KeyBinding and need a MouseBinding
                Binding = new MouseBinding()
                {
                    Gesture = new MouseGesture(defaultMouseAction, defaultModifiers),
                    Command = Binding.Command,
                    CommandParameter = Binding.CommandParameter
                };
            }
        }

        private readonly Key defaultKey = default;
        private readonly MouseAction defaultMouseAction = default;
        private readonly ModifierKeys defaultModifiers = default;
        private readonly Type defaultType = default;
    }
}
