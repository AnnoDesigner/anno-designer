using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using AnnoDesigner.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace AnnoDesigner;

public partial class PreferencesWindow : FluentWindow, ICloseable
{
    public PreferencesWindow()
    {
        InitializeComponent();

        Loaded += Preferences_Loaded;
    }

    private void Preferences_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is PreferencesViewModel vm)
        {
            vm.NavigationService = CurrentPage.NavigationService;
            vm.ShowFirstPage();
        }

        CurrentPage.Navigated += CurrentPage_Navigated;
    }

    private void CurrentPage_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
    {
        if (e.Content is INavigatedTo page)
        {
            page.NavigatedTo(e.ExtraData);
        }
    }
}
