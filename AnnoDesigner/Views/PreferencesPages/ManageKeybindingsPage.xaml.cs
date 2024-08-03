using AnnoDesigner.Models;
using AnnoDesigner.ViewModels;
using System.Windows.Controls;
using System.Windows.Data;

namespace AnnoDesigner.PreferencesPages;

/// <summary>
/// Interaction logic for ManageKeybindings.xaml
/// </summary>
public partial class ManageKeybindingsPage : Page, INavigatedTo
{
    public void NavigatedTo(object viewModel)
    {
        ViewModel = (ManageKeybindingsViewModel)viewModel;
        DataContext = ViewModel;
        ViewModel.HotkeyCommandManager.CollectionChanged += Manager_CollectionChanged;
    }

    private void Manager_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //Manually refresh the entire item source
        CollectionViewSource.GetDefaultView(HotkeyActions.ItemsSource).Refresh();
    }

    public ManageKeybindingsViewModel ViewModel;
}
