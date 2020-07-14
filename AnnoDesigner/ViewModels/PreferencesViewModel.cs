using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Services;
using AnnoDesigner.Models;
using AnnoDesigner.PreferencesPages;

namespace AnnoDesigner.ViewModels
{
    public class PreferencesViewModel : Notify
    {
        public PreferencesViewModel(IAppSettings appSettings,
            ICommons commons,
            HotkeyCommandManager manager,
            NavigationService navigationService,
            IMessageBoxService messageBoxServiceToUse)
        {
            this.appSettings = appSettings;
            this.navigationService = navigationService;
            this.manager = manager;

            ViewModels = new Dictionary<string, object>()
            {
                { nameof(ManageKeybindingsPage),  new ManageKeybindingsViewModel(manager, commons, messageBoxServiceToUse) },
                { nameof(UpdateSettingsPage), "" }
            };
        }

        private readonly NavigationService navigationService;
        private readonly HotkeyCommandManager manager;
        private readonly IAppSettings appSettings;
        private ListViewItem _selectedItem;
        private Dictionary<string, object> _viewModels;

        public ListViewItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                UpdateProperty(ref _selectedItem, value);
                ShowPage(value.Name);
            }
        }

        public Dictionary<string, object> ViewModels
        {
            get { return _viewModels; }
            set
            {
                UpdateProperty(ref _viewModels, value);
                //select first item by default
                ShowPage(_viewModels.First().Key);
            }
        }

        private void ShowPage(string name)
        {
            if (ViewModels.TryGetValue(name, out var extraData))
            {
                navigationService.Navigate(new Uri($@"pack://application:,,,/PreferencesPages\{name}.xaml", UriKind.RelativeOrAbsolute), extraData);
            }
#if DEBUG
            else
            {
                throw new KeyNotFoundException($"Page {name}.xaml not found.");
            }
#endif
        }
    }
}
