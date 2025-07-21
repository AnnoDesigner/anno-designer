using AnnoDesigner.ViewModels;
using System.Windows.Controls;

namespace AnnoDesigner;

/// <summary>
/// Interaction logic for StatisticsView.xaml
/// </summary>
public partial class StatisticsView : UserControl
{
    public StatisticsViewModel Context
    {
        get => DataContext as StatisticsViewModel;
        set => DataContext = value;
    }

    public StatisticsView()
    {
        InitializeComponent();
    }
}
