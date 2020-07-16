using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoDesigner.Core.Models
{
    public enum GestureType
    {
        MouseGesture,
        KeyGesture
    }

    public class PolyGesture : InputGesture, INotifyPropertyChanged
    {
        public PolyGesture() { }
        public PolyGesture(Key key) : this(key, default) { }
        public PolyGesture(ExtendedMouseAction mouseAction) : this(mouseAction, default) { }
        public PolyGesture(Key key, ModifierKeys modifierKeys) : this(key, default, modifierKeys, GestureType.KeyGesture) { }
        public PolyGesture(ExtendedMouseAction mouseAction, ModifierKeys modifierKeys) : this(default, mouseAction, modifierKeys, GestureType.MouseGesture) { }
        private PolyGesture(Key key, ExtendedMouseAction mouseAction, ModifierKeys modifierKeys, GestureType type)
        {
            Key = key;
            MouseAction = mouseAction;
            ModifierKeys = modifierKeys;
            Type = type;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Key. This property is ignored if <see cref="GestureType"/> is set to <see cref="GestureType.MouseGesture"/>.
        /// </summary>
        public Key Key
        {
            get => _key;
            set => UpdateProperty(ref _key, value);
        }

        /// <summary>
        /// Modifier keys.
        /// </summary>
        public ModifierKeys ModifierKeys
        {
            get => _modifierKeys;
            set => UpdateProperty(ref _modifierKeys, value);
        }

        /// <summary>
        /// Extended mouse action. This property is ignored if <see cref="GestureType"/> is set to <see cref="GestureType.KeyGesture"/>.
        /// </summary>
        public ExtendedMouseAction MouseAction
        {
            get => _mouseAction;
            set => UpdateProperty(ref _mouseAction, value);
        }

        /// <summary>
        /// A <see cref="GestureType"/> representing the type of gesture that should be matched. Either <see cref="GestureType.MouseGesture"/> or <see cref="GestureType.KeyGesture"/>.
        /// </summary>
        public GestureType Type
        {
            get => _gestureType;
            set
            {
                if (IsDefinedGestureType(value))
                {
                    UpdateProperty(ref _gestureType, value);
                }
                else
                {
                    throw new ArgumentException($"Value provided is not valid for enum {nameof(GestureType)}", nameof(Type));
                }
            }
        }

        /// <summary>
        /// Matches InputEventArgs to the current `PolyGesture`.
        /// </summary>
        /// <param name="targetElement"></param>
        /// <param name="inputEventArgs"></param>
        /// <returns>True if the gesture matches, false otherwise</returns>
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (_gestureType == GestureType.MouseGesture)
            {
                return MatchMouseGesture(inputEventArgs);
            }
            else
            {
                return MatchKeyGesture(inputEventArgs);
            }
        }

        /// <summary>
        /// Used for matching when <see cref="Type"/> is <see cref="GestureType.MouseGesture"/>.
        /// </summary>
        /// <param name="inputEventArgs"></param>
        /// <returns></returns>
        private bool MatchMouseGesture(InputEventArgs inputEventArgs)
        {
            var mouseAction = GetExtendedMouseAction(inputEventArgs);
            if (mouseAction != ExtendedMouseAction.None)
            {
                return MouseAction == mouseAction && ModifierKeys == Keyboard.Modifiers;
            }
            return false;
        }

        /// <summary>
        /// Used for matching when <see cref="Type"/> is <see cref="GestureType.KeyGesture"/>.
        /// </summary>
        /// <param name="inputEventArgs"></param>
        /// <returns></returns>
        private bool MatchKeyGesture(InputEventArgs inputEventArgs)
        {
            //never match Key.None
            if (inputEventArgs is KeyEventArgs eventArgs && eventArgs.Key != Key.None)
            {
                return Key == eventArgs.Key && ModifierKeys == Keyboard.Modifiers;
            }
            return false;
        }

        private Key _key;
        private ModifierKeys _modifierKeys;
        private ExtendedMouseAction _mouseAction;
        private GestureType _gestureType;

        /// <summary>
        /// Extracts an <see cref="ExtendedMouseAction"/> from <see cref="InputEventArgs"/>.
        /// </summary>
        /// <param name="inputArgs"></param>
        /// <returns></returns>
        public static ExtendedMouseAction GetExtendedMouseAction(InputEventArgs inputArgs)
        {
            if (inputArgs != null)
            {
                if (inputArgs is MouseWheelEventArgs)
                {
                    return ExtendedMouseAction.WheelClick;
                }
                else
                {
                    var args = inputArgs as MouseButtonEventArgs;
                    return (args.ClickCount, args.ChangedButton) switch
                    {
                        (1, MouseButton.Left) => ExtendedMouseAction.LeftClick,
                        (1, MouseButton.Right) => ExtendedMouseAction.RightClick,
                        (1, MouseButton.Middle) => ExtendedMouseAction.MiddleClick,
                        (1, MouseButton.XButton1) => ExtendedMouseAction.XButton1Click,
                        (1, MouseButton.XButton2) => ExtendedMouseAction.XButton2Click,
                        (2, MouseButton.Left) => ExtendedMouseAction.LeftDoubleClick,
                        (2, MouseButton.Right) => ExtendedMouseAction.RightDoubleClick,
                        (2, MouseButton.Middle) => ExtendedMouseAction.MiddleDoubleClick,
                        _ => ExtendedMouseAction.None,
                    };
                }
            }
            return ExtendedMouseAction.None;
        }

        /// <summary>
        /// Checks if a given <see cref="GestureType"/> is valid.
        /// </summary>
        /// <param name="gestureType"></param>
        /// <returns>True if the given gestureType is valid, false otherwise</returns>
        public static bool IsDefinedGestureType(GestureType gestureType)
        {
            return gestureType >= 0 && (int)gestureType < 2;
        }

        //Copied from AnnoDesigner.Core.Models.Notify as we need to inherit from InputGesture
        #region OnPropertyChanged helper methods

        protected bool UpdateProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(name);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            //Invoke event if not null
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
