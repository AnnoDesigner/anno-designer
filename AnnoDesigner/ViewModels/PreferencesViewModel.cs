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
using AnnoDesigner.PreferencesPages;
using NLog;

namespace AnnoDesigner.ViewModels
{
    public class PreferencesViewModel : Notify
    {
        public PreferencesViewModel(IAppSettings appSettings, ICommons commons, HotkeyCommandManager manager, NavigationService navigationService)
        {
            this.appSettings = appSettings;
            this.navigationService = navigationService;
            Manager = manager;

            ViewModels = new Dictionary<string, object>()
            {
                { "ManageKeybindingsPage",  new ManageKeybindingsViewModel(manager, commons) },
                { "UpdateSettingsPage", "" }
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
                if (ViewModels.TryGetValue(value.Name, out var extraData))
                {
                    navigationService.Navigate(new Uri($@"pack://application:,,,/PreferencesPages\{value.Name}.xaml", UriKind.RelativeOrAbsolute), extraData);
                }
#if DEBUG
                else
                {
                    throw new KeyNotFoundException($"Page {value.Name}.xaml not found.");
                }
#endif
            }
        }

        public Dictionary<string, object> ViewModels
        {
            get => _pageViewModels;
            set => UpdateProperty(ref _pageViewModels, value);
        }

        
    }


}
