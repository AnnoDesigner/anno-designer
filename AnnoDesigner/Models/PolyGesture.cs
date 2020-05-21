using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Policy;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Themes;

namespace AnnoDesigner.Models
{
    /// <summary>
    /// InputGesture implementation supporting either a mouse or keypress gesture. To bind to a command, use the 
    /// <see cref="PolyBinding"/> class, which is an implementation of the <see cref="InputBinding"/> <see langword="abstract"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// WPF <see cref="KeyBinding"/> and <see cref="KeyGesture"/> classes do not support single key presses. Within Anno Designer, 
    /// we want to allow single key press key bindings, as well as mouse clicks. Allowing this functionality to be handled
    /// by a single class simplifies binding and means we don't need to instatiate a different object if the user  decides to 
    /// bind an action to a mouse click (for example, binding a rotate command to the X1 Button on the mouse). And should simlify
    /// binding to a single source.
    /// </para>
    /// <para>
    /// To bind this to a command, use the <see cref="PolyBinding"/> class. I'd have loved to use an existing implmentation of 
    /// <see cref="InputBinding"/> (switching between <see cref="MouseBinding"/> and <see cref="KeyBinding"/>), but even though 
    /// <see cref="KeyBinding"/> has an <see cref="InputGesture"/> <c>Gesture</c> property, its artificially limited to only 
    /// accept types of <see cref="KeyGesture"/> and throws an error if I try to use it with <see cref="PolyGesture"/>
    /// </para>
    /// </remarks>
    ///
    public class PolyGesture : InputGesture, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private Key _key;
        private ModifierKeys _modifierKeys;
        private MouseButton _mouseButton;

        /// <summary>
        /// Get whether or not this <see cref="PolyGesture"/> is a key gesture. This property does not support databinding.
        /// </summary>
        public bool IsKeyGesture { get; private set; }
        /// <summary>
        /// Get whether or not this <see cref="PolyGesture"/> is a mouse gesture. This property does not support databinding.
        /// </summary>
        public bool IsMouseGesture { get; private set; }
        public Key Key
        {
            get { return _key; }
            private set 
            {
                if (!EqualityComparer<Key>.Default.Equals(_key, value))
                {
                    _key = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Key))); 
                }
            }
        }
        public ModifierKeys ModifierKeys
        {
            get { return _modifierKeys; }
            private set
            {
                if (!EqualityComparer<ModifierKeys>.Default.Equals(_modifierKeys, value))
                {
                    _modifierKeys = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModifierKeys)));
                }
            }
        }
        public MouseButton MouseButton
        {
            get { return _mouseButton; }
            private set
            {

                if (EqualityComparer<MouseButton>.Default.Equals(_mouseButton, value))
                {
                    _mouseButton = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MouseButton)));
                }
            }
        }

        public PolyGesture() { }
        public PolyGesture(Key key) : this(key, ModifierKeys.None) { }
        public PolyGesture(Key key, ModifierKeys modifierKeys)
        {
            IsKeyGesture = true;
            IsMouseGesture = false;
            Key = key;
            ModifierKeys = modifierKeys;
        }

        public PolyGesture(MouseButton mouseButton)
        {
            IsKeyGesture = false;
            IsMouseGesture = true;
            MouseButton = mouseButton;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            Debug.Assert(IsMouseGesture || IsKeyGesture, "One of IsMouseGesture or IsKeyGesture must always be true.");
            Debug.Assert(!(IsMouseGesture && IsKeyGesture), "IsMouseGesture and IsKeyGesture should never both be true.");
            if (IsKeyGesture)
            {
                var keyEventArgs = inputEventArgs as KeyEventArgs;
                Debug.Assert(keyEventArgs != null, "Object state has become corrupted - IsKeyGesture should not be true if the InputEventArgs we are receiving are not of type KeyEventArgs.");
                return Keyboard.Modifiers.HasFlag(ModifierKeys) && Key == keyEventArgs.Key;
            }

            if (IsMouseGesture)
            {
                var mouseButtonEventArgs = inputEventArgs as MouseButtonEventArgs;
                Debug.Assert(mouseButtonEventArgs != null, "Object state has become corrupted - IsMouseGesture should not be true if the InputEventArgs we are receiving are not of type MouseButtonEventArgs.");
                return mouseButtonEventArgs.ButtonState == MouseButtonState.Pressed && mouseButtonEventArgs.ChangedButton == MouseButton;
            }
            return false;
        }

        /// <summary>
        /// If this gesture is currently a mouse gesture, it will be set to a key gesture with this method, and the MouseButton set will no longer function
        /// </summary>
        public void SetKey(Key key)
        {
            SetKey(key, ModifierKeys.None);
        }

        /// <summary>
        /// If this gesture is currently a mouse gesture, it will be set to a key gesture with this method, and the MouseButton set will no longer function
        /// </summary>
        public void SetKey(Key key, ModifierKeys modifierKeys)
        {
            if (IsMouseGesture)
            {
                IsMouseGesture = false;
                MouseButton = default;
            }
            IsKeyGesture = true;

            Key = key;
            ModifierKeys = modifierKeys;
        }

        /// <summary>
        /// If this gesture is currently a key gesture, it will be set to a mouse gesture with this method, and the Key and ModifierKeys set will no longer function
        /// </summary>
        public void SetMouseButton(MouseButton mouseButton)
        {
            if (IsKeyGesture)
            {
                IsKeyGesture = false;
                Key = default;
                MouseButton = default;
            }
            IsMouseGesture = true;

            MouseButton = mouseButton;
        }
    }
}
