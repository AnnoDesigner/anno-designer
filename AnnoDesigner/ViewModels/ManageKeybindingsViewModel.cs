using System;
using System.Windows.Input;
using AnnoDesigner.Core.Controls;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Services;
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

        private readonly ICommons commons;
        private readonly IMessageBoxService _messageBoxService;

        public ManageKeybindingsViewModel(HotkeyCommandManager hotkeyCommandManager,
            ICommons commons,
            IMessageBoxService messageBoxServiceToUse)
        {
            HotkeyCommandManager = hotkeyCommandManager;
            RebindCommand = new RelayCommand<Hotkey>(ExecuteRebind);
            ResetHotkeysCommand = new RelayCommand(ExecuteResetHotkeys);
            this.commons = commons;
            this.commons.SelectedLanguageChanged += Instance_SelectedLanguageChanged;
            _messageBoxService = messageBoxServiceToUse;

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
            (Key key, ModifierKeys modifiers, ExtendedMouseAction action, ActionRecorder.ActionType actionType, bool userCancelled) = window.RecordNewAction();
#pragma warning restore IDE0007 // Use implicit type

            //Only set new hotkeys if the user didn't click cancel, and they didn't close the window without a key/action bound
            if (!userCancelled && !(key == Key.None && action == ExtendedMouseAction.None))
            {
                hotkey.UpdateHotkey(key, action, modifiers, (actionType == ActionRecorder.ActionType.KeyAction ? GestureType.KeyGesture : GestureType.MouseGesture));
            }
            RebindButtonCurrentTextKey = REBIND;
            UpdateRebindButtonText();
        }

        private void ExecuteResetHotkeys(object param)
        {
            if (_messageBoxService.ShowQuestion(Localization.Localization.Translations[RESET_ALL_CONFIRMATION_MESSAGE],
                Localization.Localization.Translations[RESET_ALL]))
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
