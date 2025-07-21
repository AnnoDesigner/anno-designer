using AnnoDesigner.ViewModels;
using System;

namespace AnnoDesigner.Undo.Operations;

public class ModifyLayoutVersionOperation : BaseOperation
{
    public LayoutSettingsViewModel LayoutSettingsViewModel { get; set; }

    public Version OldValue { get; set; }

    public Version NewValue { get; set; }

    protected override void UndoOperation()
    {
        LayoutSettingsViewModel.LayoutVersion = OldValue;
    }

    protected override void RedoOperation()
    {
        LayoutSettingsViewModel.LayoutVersion = NewValue;
    }
}
