using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoDesigner.Models
{
    public enum GestureType
    {
        MouseGesture,
        KeyGesture
    }

    public class PolyGesture : InputGesture, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Key Key
        {
            get => _key;
            set => UpdateProperty(ref _key, value);
        }

        public ModifierKeys ModifierKeys
        {
            get => _modifierKeys;
            set => UpdateProperty(ref _modifierKeys, value);
        }

        public ExtendedMouseAction MouseAction
        {
            get => _mouseAction;
            set => UpdateProperty(ref _mouseAction, value);
        }

        public GestureType Type
        {
            get => _gestureType;
            set => UpdateProperty(ref _gestureType, value);
        }

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

        private bool MatchMouseGesture(InputEventArgs inputEventArgs)
        {
            var mouseAction = GetExtendedMouseAction(inputEventArgs);
            if (mouseAction != ExtendedMouseAction.None)
            {
                return MouseAction == mouseAction && ModifierKeys == Keyboard.Modifiers;
            }
            return false;
        }

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
        /// Extracts an ExtendedMouseAction from InputEventArgs
        /// </summary>
        /// <param name="inputArgs"></param>
        /// <returns></returns>
        private ExtendedMouseAction GetExtendedMouseAction(InputEventArgs inputArgs)
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
                        (2, MouseButton.XButton1) => ExtendedMouseAction.XButton1DoubleClick,
                        (2, MouseButton.XButton2) => ExtendedMouseAction.XButton2DoubleClick,
                        _ => ExtendedMouseAction.None,
                    };
                }
            }
            return ExtendedMouseAction.None;
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
