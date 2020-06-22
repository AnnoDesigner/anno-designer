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
    /// Acts as a wrapper for a named <see cref="InputBinding"/>, and allows updating if the currently wrapped <see cref="InputBinding"/> 
    /// is replaced with a fresh reference.
    /// </summary>
    public class Hotkey : Notify
    {
        private Hotkey() { }
        public Hotkey(string hotkeyId, InputBinding binding):this(hotkeyId, binding, null) { }
        public Hotkey(string hotkeyId, InputBinding binding, string description)
        {
            HotkeyId = hotkeyId;
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
        public string HotkeyId
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
            SynchronizeProperties(defaultKey, defaultMouseAction, defaultModifiers, defaultType);
        }

        private void SynchronizeProperties(Key key, MouseAction mouseAction, ModifierKeys modifiers, Type type)
        {
            var isKeyBinding = Binding is KeyBinding;
            var isCorrectType = Binding.GetType() == type;
            if (isKeyBinding && isCorrectType)
            {
                var keyBinding = Binding as KeyBinding;
                keyBinding.Key = key;
                keyBinding.Modifiers = modifiers;
            }
            else if (!isKeyBinding && isCorrectType)
            {
                var mouseBinding = Binding as MouseBinding;
                mouseBinding.MouseAction = mouseAction;
                (mouseBinding.Gesture as MouseGesture).Modifiers = modifiers;
            }
            else if (type == typeof(KeyBinding))
            {
                //we currently have a MouseBinding and need a KeyBinding
                Binding = new KeyBinding()
                {
                    Key = key,
                    Modifiers = modifiers,
                    Command = Binding.Command,
                    CommandParameter = Binding.CommandParameter
                };
            }
            else
            {
                //we currently have a KeyBinding and need a MouseBinding
                Binding = new MouseBinding()
                {
                    Gesture = new MouseGesture(mouseAction, modifiers),
                    Command = Binding.Command,
                    CommandParameter = Binding.CommandParameter
                };
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if the current <see cref="Key"/> or <see cref="MouseAction"/> mappings for this <see cref="Hotkey"/> 
        /// do not match the default mappings it was created with.
        /// </summary>
        /// <returns></returns>
        public bool IsRemapped()
        {
            var isChangedType = Binding.GetType() != defaultType;
            if (isChangedType)
            {
                return true;
            }
            else if (Binding is KeyBinding keyBinding)
            {
                return !(keyBinding.Key == defaultKey && keyBinding.Modifiers == defaultModifiers);
            }
            else if (Binding is MouseBinding mouseBinding && mouseBinding.Gesture is MouseGesture mouseGesture)
            {
                return !(mouseGesture.MouseAction == defaultMouseAction && mouseGesture.Modifiers == defaultModifiers);
            }
#if DEBUG
            throw new Exception($"Unrecognised InputBinding type {Binding.GetType()}.");
#else
            return false;
#endif
        }

        public HotkeyInformation GetHotkeyInformation()
        {
            var hotkeyInfo = new HotkeyInformation();
            if (Binding is KeyBinding keyBinding)
            {
                hotkeyInfo.Key = keyBinding.Key;
                hotkeyInfo.Modifiers = keyBinding.Modifiers;
                hotkeyInfo.BindingType = typeof(KeyBinding);
                return hotkeyInfo;
            }  
            else if (Binding is MouseBinding mouseBinding && mouseBinding.Gesture is MouseGesture mouseGesture) {
                hotkeyInfo.MouseAction = mouseGesture.MouseAction;
                hotkeyInfo.Modifiers = mouseGesture.Modifiers;
                hotkeyInfo.BindingType = typeof(MouseBinding);
                return hotkeyInfo;
            }
#if DEBUG
            throw new Exception($"Unrecognised InputBinding type {Binding.GetType()}.");
#else
            return new HotkeyInformation()
            {
                Key = defaultKey,
                MouseAction = defaultMouseAction,
                Modifiers = defaultModifiers,
                BindingType = defaultType
            };
#endif
        }

        /// <summary>
        /// Updates a hotkey and based on the given HotkeyInformation
        /// </summary>
        /// <param name="information"></param>
        public void UpdateHotkey(HotkeyInformation information)
        {
            SynchronizeProperties(information.Key, information.MouseAction, information.Modifiers, information.BindingType);
        }

        private readonly Key defaultKey = default;
        private readonly MouseAction defaultMouseAction = default;
        private readonly ModifierKeys defaultModifiers = default;
        private readonly Type defaultType = default;
    }
}
