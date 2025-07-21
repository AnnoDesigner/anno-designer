using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Navigation;

namespace AnnoDesigner.ViewModels;

public class PreferencesViewModel : Notify
{
    private PreferencePage _selectedItem;

    public PreferencesViewModel()
    {
        Pages = [];
        CloseWindowCommand = new RelayCommand<ICloseable>(CloseWindow);
    }

    public NavigationService NavigationService { get; set; }

    public PreferencePage SelectedItem
    {
        get => _selectedItem;
        set
        {
            _ = UpdateProperty(ref _selectedItem, value);
            ShowPage(value.Name);
        }
    }

    public ObservableCollection<PreferencePage> Pages { get; set; }

    public void ShowFirstPage()
    {
        SelectedItem = Pages.First();
    }

    private void ShowPage(string name)
    {
        PreferencePage foundPage = Pages.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (foundPage != null)
        {
            NavigationService?.Navigate(new Uri($@"pack://application:,,,/Views\PreferencesPages\{name}.xaml", UriKind.RelativeOrAbsolute), foundPage.ViewModel);
        }

    }

    public ICommand CloseWindowCommand { get; private set; }
    private void CloseWindow(ICloseable window)
    {
        window?.Close();
    }
}
