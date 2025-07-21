using AnnoDesigner.ViewModels;
using System.Windows;

namespace AnnoDesigner;

/// <summary>
/// Interaction logic for LicensesWindow.xaml
/// </summary>
public partial class LicensesWindow : Window
{
    public LicensesWindow()
    {
        InitializeComponent();
        DataContext = new LicensesViewModel();
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
}
