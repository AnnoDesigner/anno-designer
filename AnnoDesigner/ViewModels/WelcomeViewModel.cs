using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;

namespace AnnoDesigner.ViewModels
{
    public class WelcomeViewModel : Notify
    {
        private readonly ICommons _commons;
        private readonly IAppSettings _appSettings;
        private ObservableCollection<SupportedLanguage> _languages;
        private SupportedLanguage _selectedItem;

        public WelcomeViewModel(ICommons commonsToUse, IAppSettings appSettingsToUse)
        {
            _commons = commonsToUse;
            _appSettings = appSettingsToUse;

            Languages = new ObservableCollection<SupportedLanguage>();
            Languages.Add(new SupportedLanguage("English")
            {
                FlagPath = "Flags/United Kingdom.png"
            });
            Languages.Add(new SupportedLanguage("Deutsch")
            {
                FlagPath = "Flags/Germany.png"
            });
            Languages.Add(new SupportedLanguage("Français")
            {
                FlagPath = "Flags/France.png"
            });
            Languages.Add(new SupportedLanguage("Polski")
            {
                FlagPath = "Flags/Poland.png"
            });
            Languages.Add(new SupportedLanguage("Русский")
            {
                FlagPath = "Flags/Russia.png"
            });
            Languages.Add(new SupportedLanguage("Español")
            {
                FlagPath = "Flags/Spain.png"
            });
            Languages.Add(new SupportedLanguage("简体中文")
            {
                FlagPath = "Flags/China.png"
            });
            ContinueCommand = new RelayCommand(Continue, CanContinue);
        }

        public ObservableCollection<SupportedLanguage> Languages
        {
            get { return _languages; }
            set { UpdateProperty(ref _languages, value); }
        }

        public SupportedLanguage SelectedItem
        {
            get { return _selectedItem; }
            set { UpdateProperty(ref _selectedItem, value); }
        }

        public ICommand ContinueCommand { get; private set; }

        private void Continue(object param)
        {
            LoadSelectedLanguage(param as ICloseable);
        }

        private bool CanContinue(object param)
        {
            return SelectedItem != null;
        }

        private void LoadSelectedLanguage(ICloseable window)
        {
            _commons.CurrentLanguage = SelectedItem.Name;

            _appSettings.SelectedLanguage = _commons.CurrentLanguage;
            _appSettings.Save();

            window?.Close();
        }
    }
}
