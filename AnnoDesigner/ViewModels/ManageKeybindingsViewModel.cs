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
        private const string EDIT = "Edit";
        private const string RECORDING = "Recording";
        private const string RESET_ALL = "ResetAll";
        private const string RESET_ALL_CONFIRMATION_MESSAGE = "ResetAllConfirmationMessage";

        private readonly ICommons commons;
        private readonly IMessageBoxService _messageBoxService;
        private readonly ILocalizationHelper _localizationHelper;
        private HotkeyCommandManager _manager;
        private ICommand _editCommand;
        private ICommand _resetHotkeysCommand;
        private string _editButtonText;

        public ManageKeybindingsViewModel(HotkeyCommandManager hotkeyCommandManager,
            ICommons commons,
            IMessageBoxService messageBoxServiceToUse,
            ILocalizationHelper localizationHelperToUse)
        {
            HotkeyCommandManager = hotkeyCommandManager;
            _messageBoxService = messageBoxServiceToUse;
            _localizationHelper = localizationHelperToUse;

            EditCommand = new RelayCommand<Hotkey>(ExecuteRebind);
            ResetHotkeysCommand = new RelayCommand(ExecuteResetHotkeys);
            this.commons = commons;
            this.commons.SelectedLanguageChanged += Instance_SelectedLanguageChanged;

            UpdateRebindButtonText();
        }

        private void Instance_SelectedLanguageChanged(object sender, EventArgs e)
        {
            UpdateRebindButtonText();
        }

        public HotkeyCommandManager HotkeyCommandManager
        {
            get { return _manager; }
            set { UpdateProperty(ref _manager, value); }
        }

        public ICommand EditCommand
        {
            get { return _editCommand; }
            set { UpdateProperty(ref _editCommand, value); }
        }

        public ICommand ResetHotkeysCommand
        {
            get { return _resetHotkeysCommand; }
            set { UpdateProperty(ref _resetHotkeysCommand, value); }
        }

        public string EditButtonText
        {
            get { return _editButtonText; }
            set { UpdateProperty(ref _editButtonText, value); }
        }

        public string EditButtonCurrentTextKey { get; set; } = EDIT;

        private void ExecuteRebind(Hotkey hotkey)
        {
            EditButtonCurrentTextKey = RECORDING;
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
            EditButtonCurrentTextKey = EDIT;
            UpdateRebindButtonText();
        }

        private void ExecuteResetHotkeys(object param)
        {
            if (_messageBoxService.ShowQuestion(_localizationHelper.GetLocalization(RESET_ALL_CONFIRMATION_MESSAGE),
                _localizationHelper.GetLocalization(RESET_ALL)))
            {
                HotkeyCommandManager.ResetHotkeys();
            }
        }

        private void UpdateRebindButtonText()
        {
            EditButtonText = _localizationHelper.GetLocalization(EditButtonCurrentTextKey);
        }
    }
}
