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
            this.appSettings = appSettings;
            this.navigationService = navigationService;
            Manager = manager;

            PageViewModels = new Dictionary<string, object>()
            {
                { "ManageKeybindings", new ManageKeybindingsViewModel(manager) },
                { "UpdateSettings", null }
            };
        }

        private readonly NavigationService navigationService;
        private readonly IAppSettings appSettings;
        private HotkeyCommandManager _manager;
        private ListViewItem _selectedItem;
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

        public Dictionary<string, object> PageViewModels
        {
            get => _pageViewModels;
            set => UpdateProperty(ref _pageViewModels, value);
        }

        
    }


}
