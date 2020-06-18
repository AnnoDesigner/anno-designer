using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using NLog;

namespace AnnoDesigner.ViewModels
{
    public class PreferencesViewModel : Notify
    {
        public PreferencesViewModel(IAppSettings appSettings, HotkeyCommandManager manager, NavigationService navigationService)
        {
            Commons.Instance.SelectedLanguageChanged += Commons_SelectedLanguageChanged;
            this.appSettings = appSettings;
            this.navigationService = navigationService;
            Manager = manager;

            PageViewModels = new Dictionary<string, object>()
            {
                { "ManageKeybindings", new ManageKeybindingsViewModel(manager) },
                { "UpdateSettings", null }
            };

            UpdateLanguage();
        }

        private void Commons_SelectedLanguageChanged(object sender, EventArgs e)
        {
            UpdateLanguage();
        }

        private void UpdateLanguage()
        {
            var language = Localization.Localization.GetLanguageCodeFromName(Commons.Instance.SelectedLanguage);
            Preferences = Localization.Localization.Translations[language]["Preferences"];
            UpdateSettings = Localization.Localization.Translations[language]["UpdateSettings"];
            ManageKeybindings = Localization.Localization.Translations[language]["ManageKeybindings"];
        }

        private readonly NavigationService navigationService;
        private readonly IAppSettings appSettings;
        private HotkeyCommandManager _manager;
        private ListViewItem _selectedItem;
        private string _updateSettings;
        private string _manageKeybindings;
        private string _preferences;
        private Dictionary<string, object> _pageViewModels;

        public HotkeyCommandManager Manager
        {
            get { return _manager; }
            set { UpdateProperty(ref _manager, value); }
        }

        public ListViewItem SelectedItem
        {
            get { return _selectedItem; }
            set 
            { 
                UpdateProperty(ref _selectedItem, value);
                //var t = Type.GetType($"AnnoDesigner.PreferencesPages.{value.Name}");
                //var page = Activator.CreateInstance(t, manager);
                //navigator.Navigate(page);
                navigationService.Navigate(new Uri($@"pack://application:,,,/PreferencesPages\{value.Name}.xaml", UriKind.RelativeOrAbsolute), value.DataContext);
            }
        }
        
        public string UpdateSettings
        {
            get => _updateSettings;
            set => UpdateProperty(ref _updateSettings, value);
        }
        
        public string ManageKeybindings
        {
            get => _manageKeybindings;
            set => UpdateProperty(ref _manageKeybindings, value);
        }

        public string Preferences
        {
            get => _preferences;
            set => UpdateProperty(ref _preferences, value);
        }

        public Dictionary<string, object> PageViewModels
        {
            get => _pageViewModels;
            set => UpdateProperty(ref _pageViewModels, value);
        }

        
    }


}
