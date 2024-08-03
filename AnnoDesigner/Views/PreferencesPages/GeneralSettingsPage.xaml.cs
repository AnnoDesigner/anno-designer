using AnnoDesigner.Models;
using AnnoDesigner.ViewModels;
using System.Windows.Controls;

namespace AnnoDesigner.PreferencesPages;

/// <summary>
/// Interaction logic for GeneralSettingsPage.xaml
/// </summary>
public partial class GeneralSettingsPage : Page, INavigatedTo
{
    public GeneralSettingsPage()
    {
        InitializeComponent();
    }

    public void NavigatedTo(object viewModel)
    {
        if (viewModel is GeneralSettingsViewModel vm)
        {
            DataContext = vm;
        }
    }
}
