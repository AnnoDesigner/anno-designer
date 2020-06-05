using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AnnoDesigner.Core.Controls;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;

namespace AnnoDesigner.ViewModels
{
    public class HotkeyRecorderViewModel : Notify
    {
        public HotkeyRecorderViewModel() 
        {
            Commons.Instance.SelectedLanguageChanged += Commons_SelectedLanguageChanged;
        }

        private void Commons_SelectedLanguageChanged(object sender, EventArgs e)
        {
            UpdateLanguage();
        }

        private ActionRecorder.ActionType _result;
        private Key _key;
        private MouseAction _mouseAction;
        private ModifierKeys _modifiers;

        public ActionRecorder.ActionType Result
        {
            get => _result;
            set => UpdateProperty(ref _result, value);
        }
        public Key Key
        {
            get => _key;
            set => UpdateProperty(ref _key, value);
        }
        public MouseAction MouseAction
        {
            get => _mouseAction;
            set => UpdateProperty(ref _mouseAction, value);
        }
        public ModifierKeys Modifiers
        {
            get => _modifiers;
            set => UpdateProperty(ref _modifiers, value);
        }

        public ICommand CancelCommand;
        public ICommand SaveCommand;

        private ActionRecorder _actionRecorder;
        public ActionRecorder ActionRecorder
        {
            get => _actionRecorder;
            set
            {
                if (_actionRecorder != null)
                {
                    //_actionRecorder.RecordingStarted -= ActionRecorder_RecordingStarted;
                    _actionRecorder.RecordingFinished -= ActionRecorder_RecordingFinished;
                }
                _actionRecorder = value;
                //ActionRecorder.RecordingStarted += ActionRecorder_RecordingStarted;
                ActionRecorder.RecordingFinished += ActionRecorder_RecordingFinished;
            }
        }

        private void ActionRecorder_RecordingFinished(object sender, Core.CustomEventArgs.ActionRecorderEventArgs e)
        {
               
        }

        private void ActionRecorder_RecordingStarted(object sender, Core.CustomEventArgs.ActionRecorderEventArgs e)
        {
            //Nothing to do here yet.    
        }


        /// <summary>
        /// Resets the ActionRecorder and any recorded actions on the view model.
        /// </summary>
        public void Reset()
        {
            ActionRecorder.Reset();
        }

        #region Localization
        private string _save;
        private string _cancel;

        public string Save
        {
            get => _save;
            set => UpdateProperty(ref _save, value);
        }

        public string Cancel
        {
            get => _cancel;
            set => UpdateProperty(ref _cancel, value);
        }

        private void UpdateLanguage()
        {
            var language = Localization.Localization.GetLanguageCodeFromName(Commons.Instance.SelectedLanguage);
            Save = Localization.Localization.Translations[language]["Save"];
            Cancel = Localization.Localization.Translations[language]["Cancel"];
        }
        #endregion
    }
}
