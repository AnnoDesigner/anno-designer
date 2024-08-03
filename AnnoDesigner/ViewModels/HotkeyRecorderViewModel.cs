using AnnoDesigner.Core.Controls;
using AnnoDesigner.Core.Models;
using System.Windows;
using System.Windows.Input;

namespace AnnoDesigner.ViewModels;

public class HotkeyRecorderViewModel : Notify
{
    public HotkeyRecorderViewModel()
    {
        SaveCommand = new RelayCommand<Window>(ExecuteSave);
        CancelCommand = new RelayCommand<Window>(ExecuteCancel);
    }

    private ActionRecorder.ActionType _result;
    private Key _key;
    private ExtendedMouseAction _mouseAction;
    private ModifierKeys _modifiers;

    public ActionRecorder.ActionType Result
    {
        get => _result;
        set => UpdateProperty(ref _result, value);
    }
    public Key Key
    {
        get => _key;
        set => UpdateProperty(ref _key, value);
    }
    public ExtendedMouseAction MouseAction
    {
        get => _mouseAction;
        set => UpdateProperty(ref _mouseAction, value);
    }
    public ModifierKeys Modifiers
    {
        get => _modifiers;
        set => UpdateProperty(ref _modifiers, value);
    }

    public ICommand CancelCommand { get; private set; }
    private void ExecuteCancel(Window w)
    {
        w.DialogResult = false;
        w.Close();
    }

    public ICommand SaveCommand { get; private set; }
    private void ExecuteSave(Window w)
    {
        w.DialogResult = true;
        w.Close();
    }

    public ActionRecorder ActionRecorder { get; set; }

    /// <summary>
    /// Resets the ActionRecorder and any recorded actions on the view model.
    /// </summary>
    public void Reset()
    {
        ActionRecorder.Reset();
    }
}
