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
            this.manager = manager;

            ViewModels = new Dictionary<string, object>()
            {
                { nameof(ManageKeybindingsPage),  new ManageKeybindingsViewModel(manager, commons) },
                { nameof(UpdateSettingsPage), "" }
            };
        }

        private readonly NavigationService navigationService;
        private HotkeyCommandManager manager;
        private readonly IAppSettings appSettings;
        private ListViewItem _selectedItem;
        private Dictionary<string, object> _viewModels;

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
            get => _viewModels;
            set => UpdateProperty(ref _viewModels, value);
        }

        
    }


}
