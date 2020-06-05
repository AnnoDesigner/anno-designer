﻿using System;
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
            MouseAction = MouseAction.None;
        }

        public event EventHandler<ActionRecorderEventArgs> RecordingStarted;
        public event EventHandler<ActionRecorderEventArgs> RecordingFinished;

        public void Reset()
        {
            Modifiers = ModifierKeys.None;
            Key = Key.None;
            MouseAction = MouseAction.None;
            Result = ActionType.None;
        }

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
            DependencyProperty.Register("MouseAction", typeof(MouseAction), typeof(ActionRecorder), new FrameworkPropertyMetadata(MouseAction.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // Using a DependencyProperty as the backing store for IsDisplayFrozen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDisplayFrozenProperty =
            DependencyProperty.Register("IsDisplayFrozen", typeof(bool), typeof(ActionRecorder), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // Using a DependencyProperty as the backing store for Result.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(ActionType), typeof(ActionRecorder), new FrameworkPropertyMetadata(ActionType.None, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public ActionType Result
        {
            get { return (ActionType)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
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

        public MouseAction MouseAction
        {
            get { return (MouseAction)GetValue(MouseActionProperty); }
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

        private void UpdateDisplay()
        {
            var modifiers = Modifiers == ModifierKeys.None ? "" : Modifiers.ToString();
            var key = Key == Key.None ? "" : KeyboardInteropHelper.GetDisplayString(Key) ?? Key.ToString();
            var mouse = MouseAction == MouseAction.None ? "" : MouseAction.ToString();

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
            //RecordedInput.Text = display;
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
                if (!recordingMouseCombination)
                {
                    recordingMouseCombination = true;
                }
                if (e.ClickCount > 1)
                {
                    switch (e.ChangedButton)
                    {
                        case MouseButton.Left:
                            MouseAction = MouseAction.LeftDoubleClick;
                            break;
                        case MouseButton.Middle:
                            MouseAction = MouseAction.MiddleDoubleClick;
                            break;
                        case MouseButton.Right:
                            MouseAction = MouseAction.RightDoubleClick;
                            break;
                        default:
                            return;
                    }
                }
                else
                {
                    switch (e.ChangedButton)
                    {
                        case MouseButton.Left:
                            MouseAction = MouseAction.LeftClick;
                            break;
                        case MouseButton.Middle:
                            MouseAction = MouseAction.MiddleClick;
                            break;
                        case MouseButton.Right:
                            MouseAction = MouseAction.RightClick;
                            break;
                        default:
                            return;
                    }
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

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            //OnKeyDown(e);
            //base.OnPreviewKeyDown(e);
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
            if (recordingKeyCombination)
            {
                Result = ActionType.KeyAction;
            }
            else if (recordingMouseCombination)
            {
                Result = ActionType.MouseAction;
            }
            else
            {
                Result = ActionType.None;
            }

            recordingKeyCombination = false;
            recordingMouseCombination = false;
            startNewRecording = true; //Save the current state, but if we start recording again, remove the current saved Modifiers
            RecordingFinished?.Invoke(this, new ActionRecorderEventArgs(Key, MouseAction, Modifiers, Result));
        }

        private void StartNewRecording()
        {
            startNewRecording = false;
            Reset();
            RecordingStarted?.Invoke(this, ActionRecorderEventArgs.Empty);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Debug.WriteLine("GotFocus");
            base.OnGotFocus(e);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Debug.WriteLine("GotKeyboardFocus");
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            Debug.WriteLine("LostFocus");
            base.OnLostFocus(e);
        }

    }
}
