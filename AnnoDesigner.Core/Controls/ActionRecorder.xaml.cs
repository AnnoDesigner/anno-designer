using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.CustomEventArgs;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Controls
{
    /// <summary>
    /// Interaction logic for ActionRecorder.xaml
    /// </summary>
    public partial class ActionRecorder : UserControl
    {
        public ActionRecorder()
        {
            InitializeComponent();
            Key = Key.None;
            Modifiers = ModifierKeys.None;
            MouseAction = ExtendedMouseAction.None;
        }

        public event EventHandler<ActionRecorderEventArgs> RecordingStarted;
        public event EventHandler<ActionRecorderEventArgs> RecordingFinished;


        public enum ActionType
        {
            None, KeyAction, MouseAction
        }

        // Using a DependencyProperty as the backing store for Key.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(Key), typeof(ActionRecorder), new FrameworkPropertyMetadata(Key.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // Using a DependencyProperty as the backing store for Modifiers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModifiersProperty =
            DependencyProperty.Register("Modifiers", typeof(ModifierKeys), typeof(ActionRecorder), new FrameworkPropertyMetadata(ModifierKeys.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // Using a DependencyProperty as the backing store for MouseAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseActionProperty =
            DependencyProperty.Register("MouseAction", typeof(ExtendedMouseAction), typeof(ActionRecorder), new FrameworkPropertyMetadata(ExtendedMouseAction.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // Using a DependencyProperty as the backing store for IsDisplayFrozen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDisplayFrozenProperty =
            DependencyProperty.Register("IsDisplayFrozen", typeof(bool), typeof(ActionRecorder), new FrameworkPropertyMetadata(false));

        // Using a DependencyProperty as the backing store for Result.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResultTypeProperty =
            DependencyProperty.Register("ResultType", typeof(ActionType), typeof(ActionRecorder), new FrameworkPropertyMetadata(ActionType.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public ActionType ResultType
        {
            get { return (ActionType)GetValue(ResultTypeProperty); }
            set { SetValue(ResultTypeProperty, value); }
        }

        public bool IsDisplayFrozen
        {
            get { return (bool)GetValue(IsDisplayFrozenProperty); }
            set { SetValue(IsDisplayFrozenProperty, value); }
        }

        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set
            {
                SetValue(KeyProperty, value);
                if (!IsDisplayFrozen)
                {
                    UpdateDisplay();
                }
            }
        }

        public ModifierKeys Modifiers
        {
            get { return (ModifierKeys)GetValue(ModifiersProperty); }
            set
            {
                SetValue(ModifiersProperty, value);
                if (!IsDisplayFrozen)
                {
                    UpdateDisplay();
                }
            }
        }

        public ExtendedMouseAction MouseAction
        {
            get { return (ExtendedMouseAction)GetValue(MouseActionProperty); }
            set
            {
                SetValue(MouseActionProperty, value);
                if (!IsDisplayFrozen)
                {
                    UpdateDisplay();
                }
            }
        }

        private static Key[] MODIFIER_KEYS { get; } = new[]
        {
            Key.LeftCtrl,
            Key.RightCtrl,
            Key.LeftAlt,
            Key.RightAlt,
            Key.LeftShift,
            Key.RightShift,
            Key.LWin,
            Key.RWin
        };

        private static ExtendedMouseAction[] UNSUPPORTED_MOUSE_ACTIONS { get; } = new[]
        {
            ExtendedMouseAction.XButton1Click,
            ExtendedMouseAction.XButton2Click
        };

        /// <summary>
        /// True if we are currently recording a key combination (optional modifier key(s) + key on keyboard)
        /// </summary>
        private bool recordingKeyCombination = false;
        /// <summary>
        /// True is we are currently recording a mouse combination (optional modifier key(s) + mouse button)
        /// </summary>
        private bool recordingMouseCombination = false;
        /// <summary>
        /// True if we should reset the current value stored in Modifiers to <see cref="ModifierKeys.None"/> at the start of the next recording.
        /// </summary>
        private bool startNewRecording = false;

        public void Reset()
        {
            Modifiers = ModifierKeys.None;
            Key = Key.None;
            MouseAction = ExtendedMouseAction.None;
            ResultType = ActionType.None;
        }

        private void UpdateDisplay()
        {
            var modifiers = Modifiers == ModifierKeys.None ? "" : Modifiers.ToString();
            var key = Key == Key.None ? "" : KeyboardInteropHelper.GetDisplayString(Key) ?? Key.ToString();
            var mouse = MouseAction == ExtendedMouseAction.None ? "" : MouseAction.ToString();

            string display;
            if (recordingKeyCombination)
            {

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(modifiers))
                {
                    display = modifiers + " + " + key;
                }
                else
                {
                    display = modifiers + key; //Don't add a '+' as one of them is empty.
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(mouse) && !string.IsNullOrEmpty(modifiers))
                {
                    display = modifiers + " + " + mouse;
                }
                else
                {
                    display = modifiers + mouse; //Don't add a '+' as one of them is empty.
                }
            }
            RecordedInput.Content = display;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Focus();
            if (startNewRecording)
            {
                StartNewRecording();
            }

            if (!recordingKeyCombination)
            {
                //Do not record MouseActions that are not supported.
                var action = PolyGesture.GetExtendedMouseAction(e);
                if (!UNSUPPORTED_MOUSE_ACTIONS.Contains(action))
                {
                    if (!recordingMouseCombination)
                    {
                        recordingMouseCombination = true;
                    }
                    MouseAction = action;
                    EnsureCorrectResultType();
                }

            }
        }


        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None) //If no modifier keys are pressed, then reset the recording
            {
                EndCurrentRecording();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (startNewRecording)
            {
                StartNewRecording();
            }

            if (e.Key == Key.System)
            {
                //key is being processed by the system, so should not be set in a keybinding.
                return;
            }

            if (MODIFIER_KEYS.Contains(e.Key))
            {
                Modifiers = Keyboard.Modifiers;
                return;
            }

            if (!recordingMouseCombination)
            {
                //Only set this after checking for modifier keys (above). We only know if we are starting a new key or mouse recording at this point
                //as the user is likely to press a modifier key first before clicking or pressing a single key.
                if (!recordingKeyCombination)
                {
                    recordingKeyCombination = true;
                }

                if (e.Key != Key)
                {
                    Key = e.Key;
                    e.Handled = true;
                }
            }
        }

        private void EndCurrentRecording()
        {
            if (!startNewRecording)
            {
                EnsureCorrectResultType();

                recordingKeyCombination = false;
                recordingMouseCombination = false;
                startNewRecording = true; //Save the current state, but if we start recording again, remove the current saved Modifiers
                RecordingFinished?.Invoke(this, new ActionRecorderEventArgs(Key, MouseAction, Modifiers, ResultType));
            }
        }

        private void EnsureCorrectResultType()
        {
            if (recordingKeyCombination)
            {
                ResultType = ActionType.KeyAction;
            }
            else if (recordingMouseCombination)
            {
                ResultType = ActionType.MouseAction;
            }
            else
            {
                ResultType = ActionType.None;
            }
        }

        private void StartNewRecording()
        {
            startNewRecording = false;
            Reset();
            RecordingStarted?.Invoke(this, ActionRecorderEventArgs.Empty);
        }
    }
}
