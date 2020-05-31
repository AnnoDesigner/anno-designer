using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
            Manager = hotkeyCommandManager;
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

        public HotkeyCommandManager Manager
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
#pragma warning disable IDE0007 // Use implicit type //Intent is much clearer
            (Key key, ModifierKeys modifiers, bool userCancelled) = HotkeyRecordingWindow.RecordNewBinding();
#pragma warning restore IDE0007 // Use implicit type
            //Only set new hotkeys if the user didn't click cancel, and they didn't close the window without pressing anything
            if (!userCancelled && !(key == Key.None && modifiers == ModifierKeys.None))
            {
                Debug.WriteLine($"Recieved the following binding: {modifiers} + {key}");
                var keybinding = hotkey.Binding as KeyBinding;
                keybinding.Key = key;
                keybinding.Modifiers = modifiers;
            }
            RebindButtonCurrentTextKey = REBIND_KEY;
            UpdateRebindButtonText();
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
