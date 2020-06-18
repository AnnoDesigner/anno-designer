using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Xml;
using AnnoDesigner.Core.Controls;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;

namespace AnnoDesigner.ViewModels
{
    public class ManageKeybindingsViewModel : Notify
    {
        private const string REBIND_KEY = "Rebind";
        private const string RECORDING_KEY = "Recording";

        public ManageKeybindingsViewModel(HotkeyCommandManager hotkeyCommandManager)
        {
            HotkeyCommandManager = hotkeyCommandManager;
            RebindCommand = new RelayCommand<Hotkey>(ExecuteRebind);
            Commons.Instance.SelectedLanguageChanged += Commons_SelectedLanguageChanged;
            currentLanguage = Localization.Localization.GetLanguageCodeFromName(Commons.Instance.SelectedLanguage);

            UpdateLanguage();
        }

        private void Commons_SelectedLanguageChanged(object sender, EventArgs e)
        {
            currentLanguage = Localization.Localization.GetLanguageCodeFromName(Commons.Instance.SelectedLanguage);
            UpdateLanguage();
        }

        private HotkeyCommandManager _manager;
        private ICommand _rebindCommand;
        private string _rebindButtonText;
        private string currentLanguage;

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

        public string RebindButtonText
        {
            get { return _rebindButtonText; }
            set { UpdateProperty(ref _rebindButtonText, value); }
        }

        public string RebindButtonCurrentTextKey { get; set; } = REBIND_KEY;

        private void ExecuteRebind(Hotkey hotkey)
        {
            RebindButtonCurrentTextKey = RECORDING_KEY;
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
                        hotkey.Binding = GetMouseBinding(mouseBinding, action, modifiers);
                    }
                    else
                    {
                        hotkey.Binding = GetMouseBinding(hotkey.Binding as KeyBinding, action, modifiers);
                    }
                }
            }
            RebindButtonCurrentTextKey = REBIND_KEY;
            UpdateRebindButtonText();
        }

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

        private MouseBinding GetMouseBinding(MouseBinding mouseBinding, MouseAction action, ModifierKeys modifierKeys)
        {
            //This is not an exact copy, hence why this method is private. It should only be used internally.
            //Properties such as IsFrozen, IsSealed are not copied, and could lead to inconsistencies if used
            //in a wider scope.
            var mouseGesture = new MouseGesture(action, modifierKeys);
            mouseBinding.Gesture = mouseGesture;
            return mouseBinding;
        }

        private void UpdateLanguage()
        {
            UpdateRebindButtonText();
        }

        private void UpdateRebindButtonText()
        {
            RebindButtonText = Localization.Localization.Translations[currentLanguage][RebindButtonCurrentTextKey];
        }
    }
}
