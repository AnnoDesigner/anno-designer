using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Navigation;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;

namespace AnnoDesigner.ViewModels
{
    public class PreferencesViewModel : Notify
    {
        private PreferencePage _selectedItem;

        public PreferencesViewModel()
        {
            Pages = new ObservableCollection<PreferencePage>();
        }

        public NavigationService NavigationService { get; set; }

        public PreferencePage SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                UpdateProperty(ref _selectedItem, value);
                ShowPage(value.Name);
            }
        }

        public ObservableCollection<PreferencePage> Pages { get; }

        public void ShowFirstPage()
        {
            SelectedItem = Pages.First();
        }

        private void ShowPage(string name)
        {
            var foundPage = Pages.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (foundPage != null)
            {
                NavigationService?.Navigate(new Uri($@"pack://application:,,,/PreferencesPages\{name}.xaml", UriKind.RelativeOrAbsolute), foundPage.ViewModel);
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
