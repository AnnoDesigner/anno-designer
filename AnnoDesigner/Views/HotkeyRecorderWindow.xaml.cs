using AnnoDesigner.Core.Controls;
using AnnoDesigner.Core.Models;
using AnnoDesigner.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace AnnoDesigner;

/// <summary>
/// Interaction logic for KeyRecorderWindow.xaml
/// </summary>
public partial class HotkeyRecorderWindow : Window, ICloseable
{
    public HotkeyRecorderWindow()
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        InitializeComponent();
        ViewModel = new HotkeyRecorderViewModel
        {
            ActionRecorder = ActionRecorder
        };
        DataContext = ViewModel;
        _ = ActionRecorder.Focus();
        Closing += HotkeyRecorderWindow_Closing;
    }

    private void HotkeyRecorderWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        //handle when the user just clicks the "X"
        if (!DialogResult.HasValue)
        {
            DialogResult = false;
        }
    }

    public (Key key, ModifierKeys modifiers, ExtendedMouseAction action, ActionRecorder.ActionType result, bool userCancelled) RecordNewAction()
    {
        ViewModel.Reset();
        _ = ShowDialog();
        return (ViewModel.Key, ViewModel.Modifiers, ViewModel.MouseAction, ViewModel.Result, !DialogResult.Value);
    }

    private HotkeyRecorderViewModel ViewModel { get; set; }
}
