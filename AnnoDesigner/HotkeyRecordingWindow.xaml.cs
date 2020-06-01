using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32.SafeHandles;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for KeyRecorderWindow.xaml
    /// </summary>
    public partial class HotkeyRecordingWindow : Window
    {
        public enum HotkeyRecordingWindowResult
        {
            None, KeyAction, MouseAction
        }
        //TODO: localize title and button. 
        //TODO: Move most of this to a UserControl before PR.
        private HotkeyRecordingWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            WindowStyle = WindowStyle.ToolWindow;

            InitializeComponent();

            KeyDown += HotkeyRecordingWindow_KeyDown;
            KeyUp += HotkeyRecordingWindow_KeyUp;
            MouseDown += HotkeyRecordingWindow_MouseDown;
            Closing += HotkeyRecordingWindow_Closing;

            Key = Key.None;
            Modifiers = ModifierKeys.None;
            MouseAction = MouseAction.None;
        }


        private Key _key;
        private Key Key
        {
            get { return _key; }
            set
            {
                _key = value;
                UpdateDisplay();
            }
        }

        private ModifierKeys _modifiers;
        private ModifierKeys Modifiers
        {
            get => _modifiers;
            set
            {
                _modifiers = value;
                UpdateDisplay();
            }
        }

        private MouseAction _mouseAction;
        private MouseAction MouseAction
        {
            get => _mouseAction;
            set
            {
                _mouseAction = value;
                UpdateDisplay();
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
        private bool resetKeysOnNextRecording = false;

        private HotkeyRecordingWindowResult result = HotkeyRecordingWindowResult.None;

        public static (Key Key, ModifierKeys Modifiers, MouseAction action, HotkeyRecordingWindowResult result, bool userCancelled) RecordNewBinding()
        {
            var window = new HotkeyRecordingWindow();
            window.ShowDialog();
            return (window.Key, window.Modifiers, window.MouseAction, window.result, !window.DialogResult.Value);
        }

        private void UpdateDisplay()
        {
            var modifiers = Modifiers == ModifierKeys.None ? "" : Modifiers.ToString();
            var key = Key == Key.None ? "" : Key.ToString();
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
            //TODO: Add a proper key mapping utility before PR. Needs to work for different keyboards and cultures,
            //and should fall back to Key enum names for unprintable or whitespace only characters, e.g Shift, Space, Alt.
            //Might need to handle differences between Windows 7 and Windows 10, as some API behaviour changed at Windows 8
            //References
            //https://docs.microsoft.com/en-gb/windows/win32/api/winuser/
            //https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-mapvirtualkeya
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.keyinterop.virtualkeyfromkey
            //https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-tounicode
            //https://stackoverflow.com/questions/5825820/how-to-capture-the-character-on-different-locale-keyboards-in-wpf-c/5826175#5826175
        }

        private void HotkeyRecordingWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (resetKeysOnNextRecording)
            {
                resetKeysOnNextRecording = false;
                Modifiers = ModifierKeys.None;
                Key = Key.None;
                MouseAction = MouseAction.None;
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


        private void HotkeyRecordingWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None) //If no modifier keys are pressed, then reset the recording
            {
                EndCurrentRecording();
            }
            //else
            //{
            //    Debug.WriteLine("Reset modifiers");
            //    //We must still have a modifier key pressed.
            //    Modifiers = Keyboard.Modifiers;
            //}
        }

        private void HotkeyRecordingWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (resetKeysOnNextRecording)
            {
                resetKeysOnNextRecording = false;
                Key = Key.None;
                MouseAction = MouseAction.None;
            }

            if (e.Key == Key.System)
            {
                //key is being processed by the system, so should not be set in a keybinding.
                return;
            }

            //TODO: Remove this debug comment before PR
            //Debug.WriteLine($"SystemKey: {e.SystemKey}, Key: {e.Key}, ImeProcessedKey: {e.ImeProcessedKey}");

            if (MODIFIER_KEYS.Contains(e.Key))
            {
                Modifiers = Keyboard.Modifiers;
                //TODO: Remove this debug comment before PR
                //Debug.WriteLine($"Set Modifiers: {Modifiers}");
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
                }
            }
        }


        private void HotkeyRecordingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KeyDown -= HotkeyRecordingWindow_KeyDown;
            KeyUp -= HotkeyRecordingWindow_KeyUp;
            Closing -= HotkeyRecordingWindow_Closing;
            MouseDown -= HotkeyRecordingWindow_MouseDown;
            if (!DialogResult.HasValue)
            {
                DialogResult = false;
            }

            result = (Key != Key.None) ? HotkeyRecordingWindowResult.KeyAction : HotkeyRecordingWindowResult.MouseAction;
        }

        private void EndCurrentRecording()
        {
            recordingKeyCombination = false;
            recordingMouseCombination = false;
            resetKeysOnNextRecording = true; //Save the current state, but if we start recording again, remove the current saved Modifiers
        }

        private void SaveRecordingClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            e.Handled = true; //prevent click from bubbling up to window.
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            e.Handled = true; //prevent click from bubbling up to window.
            Close();
        }
    }
}
