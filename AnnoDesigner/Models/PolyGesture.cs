using System.Diagnostics;
using System.Windows.Input;

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
    /// bind an action to a mouse click (for example, binding a rotate command to the X1 Button on the mouse).
    /// </para>
    /// <para>
    /// To bind this to a command, use the Polybinding class. I'd have loved to use an existing implmentation of 
    /// <see cref="InputBinding"/> (switching between <see cref="MouseBinding"/> and <see cref="KeyBinding"/>, but even though 
    /// <see cref="KeyBinding"/> has an <see cref="InputGesture"/> <c>Gesture</c> property, its artificially limited to only 
    /// accept types of <see cref=" KeyGesture"/>
    /// </para>
    /// </remarks>
    ///
    public class PolyGesture : InputGesture
    {
        private Key key;
        private ModifierKeys modifierKeys;
        private MouseButton mouseButton;

        public bool IsKeyGesture { get; private set; }
        public bool IsMouseGesture { get; private set; }
        public Key Key { get => key; set => key = value; }
        public ModifierKeys ModifierKeys { get => modifierKeys; set => modifierKeys = value; }
        public MouseButton MouseButton { get => mouseButton; set => mouseButton = value; }

        public PolyGesture()
        {

        }

        public PolyGesture(Key key) : this(key, ModifierKeys.None) { }
        public PolyGesture(Key key, ModifierKeys modifierKeys)
        {
            IsKeyGesture = true;
            IsMouseGesture = false;
            this.Key = key;
            this.ModifierKeys = modifierKeys;
        }

        public PolyGesture(MouseButton mouseButton)
        {
            IsKeyGesture = false;
            IsMouseGesture = true;
            this.MouseButton = mouseButton;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            var keyEventArgs = inputEventArgs as KeyEventArgs;
            if (keyEventArgs == null)
            {
                var mouseEventArgs = inputEventArgs as MouseButtonEventArgs;
                Debug.WriteLine("Make sure this if statement is removed before you accept the PR");
                if (mouseEventArgs == null)
                {
                    Debugger.Break();
                }
                return mouseEventArgs.ButtonState == MouseButtonState.Pressed && mouseEventArgs.ChangedButton == MouseButton;
            }
            else
            {
                return Keyboard.Modifiers.HasFlag(ModifierKeys) && Key == keyEventArgs.Key;
            }

        }
    }
}
