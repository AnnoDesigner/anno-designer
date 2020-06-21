using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using AnnoDesigner.Core.Controls;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;

namespace AnnoDesigner.ViewModels
{
    public class ManageKeybindingsViewModel : Notify
    {
        /// <summary>
        /// These keys match values in the Localization dictionary
        /// </summary>
        private const string REBIND = "Rebind";
        private const string RECORDING = "Recording";
        private const string RESET_ALL = "ResetAll";
        private const string RESET_ALL_CONFIRMATION_MESSAGE = "ResetAllConfirmationMessage";
        

        public ManageKeybindingsViewModel(HotkeyCommandManager hotkeyCommandManager)
        {
            HotkeyCommandManager = hotkeyCommandManager;
            RebindCommand = new RelayCommand<Hotkey>(ExecuteRebind);
            ResetHotkeysCommand = new RelayCommand(ExecuteResetHotkeys);
            Commons.Instance.SelectedLanguageChanged += Instance_SelectedLanguageChanged;

            UpdateRebindButtonText();
        }

        private void Instance_SelectedLanguageChanged(object sender, EventArgs e)
        {
            UpdateRebindButtonText();
        }

        private HotkeyCommandManager _manager;
        private ICommand _rebindCommand;
        private ICommand _resetHotkeysCommand;
        private string _rebindButtonText;

        public HotkeyCommandManager HotkeyCommandManager
        {
            get { return _manager; }
            set { UpdateProperty(ref _manager, value); }
        }

        public ICommand RebindCommand
        {
            get { return _rebindCommand; }
            set { UpdateProperty(ref _rebindCommand, value); }
        }
        
        public ICommand ResetHotkeysCommand
        {
            get { return _resetHotkeysCommand; }
            set { UpdateProperty(ref _resetHotkeysCommand, value); }
        }

        public string RebindButtonText
        {
            get { return _rebindButtonText; }
            set { UpdateProperty(ref _rebindButtonText, value); }
        }

        public string RebindButtonCurrentTextKey { get; set; } = REBIND;

        private void ExecuteRebind(Hotkey hotkey)
        {
            RebindButtonCurrentTextKey = RECORDING;
            UpdateRebindButtonText();

            var window = new HotkeyRecorderWindow();

#pragma warning disable IDE0007 // Use implicit type //Intent is much clearer
            (Key key, ModifierKeys modifiers, MouseAction action, ActionRecorder.ActionType actionType, bool userCancelled) = window.RecordNewAction(); 
#pragma warning restore IDE0007 // Use implicit type

            //Only set new hotkeys if the user didn't click cancel, and they didn't close the window without a key bound
            if (!userCancelled)
            {
                Debug.WriteLine($"Recieved the following binding: {modifiers} + {key} + {action} + {actionType}");
                if (actionType == ActionRecorder.ActionType.KeyAction)
                {
                    if (hotkey.Binding is KeyBinding keyBinding)
                    {
                        keyBinding.Key = key;
                        keyBinding.Modifiers = modifiers;
                    }
                    else
                    {
                        hotkey.Binding = GetKeyBinding(hotkey.Binding as MouseBinding, key, modifiers);
                    }
                }
                else
                {
                    if (hotkey.Binding is MouseBinding mouseBinding)
                    {
                        hotkey.Binding = UpdateMouseBinding(mouseBinding, action, modifiers);
                    }
                    else
                    {
                        hotkey.Binding = GetMouseBinding(hotkey.Binding as KeyBinding, action, modifiers);
                    }
                }
            }
            RebindButtonCurrentTextKey = REBIND;
            UpdateRebindButtonText();
        }

        /// <summary>
        /// Creates a <see cref="KeyBinding"/> from a given <see cref="MouseBinding"/>, copying over the Command, CommandParameter and CommandTarget properties
        /// </summary>
        /// <param name="mouseBinding"></param>
        /// <param name="key"></param>
        /// <param name="modifierKeys"></param>
        /// <returns></returns>
        private KeyBinding GetKeyBinding(MouseBinding mouseBinding, Key key, ModifierKeys modifierKeys)
        {
            //This is not an exact copy, hence why this method is private and should only be used internally by the class.
            //Properties such as IsFrozen, IsSealed are not copied, and could lead to inconsistencies if used
            //in a wider scope.
            var keyBinding = new KeyBinding
            {
                Command = mouseBinding.Command,
                CommandParameter = mouseBinding.CommandParameter,
                CommandTarget = mouseBinding.CommandTarget,
                Key = key,
                Modifiers = modifierKeys
            };
            return keyBinding;
        }

        /// <summary>
        /// Creates a <see cref="MouseBinding"/> from a given <see cref="KeyBinding"/>, copying over the Command, CommandParameter and CommandTarget properties
        /// </summary>
        /// <param name="keyBinding"></param>
        /// <param name="action"></param>
        /// <param name="modifierKeys"></param>
        /// <returns></returns>
        private MouseBinding GetMouseBinding(KeyBinding keyBinding, MouseAction action, ModifierKeys modifierKeys)
        {
            //This is not an exact copy, hence why this method is private. It should only be used internally.
            //Properties such as IsFrozen, IsSealed are not copied, and could lead to inconsistencies if used
            //in a wider scope.
            //We have to create the MouseGesture separately, as we can't access the modifiers property from the MouseBinding
            var mouseGesture = new MouseGesture(action, modifierKeys);
            var mouseBinding = new MouseBinding
            {
                Command = keyBinding.Command,
                CommandParameter = keyBinding.CommandParameter,
                CommandTarget = keyBinding.CommandTarget,
                Gesture = mouseGesture 
            };
            return mouseBinding;
        }

        /// <summary>
        /// Updates a <see cref="MouseBinding"/> with a new <see cref="MouseGesture"/>
        /// </summary>
        /// <param name="mouseBinding"></param>
        /// <param name="action"></param>
        /// <param name="modifierKeys"></param>
        /// <returns></returns>
        private MouseBinding UpdateMouseBinding(MouseBinding mouseBinding, MouseAction action, ModifierKeys modifierKeys)
        {
            //This is not an exact copy, hence why this method is private. It should only be used internally.
            //Properties such as IsFrozen, IsSealed are not copied, and could lead to inconsistencies if used
            //in a wider scope.
            var mouseGesture = new MouseGesture(action, modifierKeys);
            mouseBinding.Gesture = mouseGesture;
            return mouseBinding;
        }

        private void ExecuteResetHotkeys(object param)
        {
            if (Xceed.Wpf.Toolkit.MessageBox.Show(
                Localization.Localization.Translations[RESET_ALL_CONFIRMATION_MESSAGE], 
                Localization.Localization.Translations[RESET_ALL], 
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                HotkeyCommandManager.ResetHotkeys();
            }
        }

        private void UpdateRebindButtonText()
        {
            RebindButtonText = Localization.Localization.Translations[RebindButtonCurrentTextKey];
        }
    }
}
