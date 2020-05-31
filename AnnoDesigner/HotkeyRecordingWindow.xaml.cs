using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
        private HotkeyRecordingWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            WindowStyle = WindowStyle.ToolWindow;
            InitializeComponent();
            KeyDown += HotkeyRecordingWindow_KeyDown;
            KeyUp += HotkeyRecordingWindow_KeyUp;
            Closing += HotkeyRecordingWindow_Closing;
            Key = Key.None;
            Modifiers = ModifierKeys.None;
            recordingKeyCombination = false;
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

        private void UpdateDisplay()
        {
            var modifiers = Modifiers == ModifierKeys.None ? "" : Modifiers.ToString();
            var key = Key == Key.None ? "" : Key.ToString();

            string display;
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(modifiers))
            {
                display = modifiers + " + " + key;
            }
            else
            {
                display = modifiers + key; //Don't add a '+' as one of them is empty.
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

        private bool recordingKeyCombination = false;

        private void HotkeyRecordingWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None) //If no modifier keys are pressed, then reset the recording
            {
                recordingKeyCombination = false;
            }
        }
        
        private void HotkeyRecordingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KeyDown -= HotkeyRecordingWindow_KeyDown;
            KeyUp -= HotkeyRecordingWindow_KeyUp;
            Closing -= HotkeyRecordingWindow_Closing;
            if (!DialogResult.HasValue)
            {
                DialogResult = false;
            }
        }

        public static (Key Key, ModifierKeys Modifiers, bool userCancelled) RecordNewBinding()
        {
            var window = new HotkeyRecordingWindow();
            window.ShowDialog();
            return (window.Key, window.Modifiers, !window.DialogResult.Value);
        }

        private void HotkeyRecordingWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System)
            {
                //key is being processed by the system, so should not be set in a keybinding.
                return;
            }

            //TODO: Remove this debug comment before PR
            //Debug.WriteLine($"SystemKey: {e.SystemKey}, Key: {e.Key}, ImeProcessedKey: {e.ImeProcessedKey}");

            if (!recordingKeyCombination)
            {
                Modifiers = ModifierKeys.None;
                Key = Key.None;
                recordingKeyCombination = true;
            }
            if (MODIFIER_KEYS.Contains(e.Key))
            {
                Modifiers = Keyboard.Modifiers;
                return;
            }
            if (e.Key != Key)
            {
                Key = e.Key;
            }
        }

        private void SetBindingClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
