using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.model;
using AnnoDesigner.Properties;

namespace AnnoDesigner.viewmodel
{
    public class WelcomeViewModel : Notify
    {
        private ObservableCollection<SupportedLanguage> _languages;
        private SupportedLanguage _selectedItem;

        public WelcomeViewModel()
        {
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
            MainWindow.SelectedLanguage = SelectedItem.Name;

            Settings.Default.SelectedLanguage = MainWindow.SelectedLanguage;
            Settings.Default.Save();

            window?.Close();
        }
    }
}
