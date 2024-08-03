using AnnoDesigner.ViewModels;
using System.Windows.Controls;

namespace AnnoDesigner;

/// <summary>
/// Interaction logic for VersionView.xaml
/// </summary>
public partial class VersionView : UserControl
{
    public LayoutSettingsViewModel Context
    {
        get => DataContext as LayoutSettingsViewModel;
        set => DataContext = value;
    }

    public VersionView()
    {
        InitializeComponent();
    }
}
