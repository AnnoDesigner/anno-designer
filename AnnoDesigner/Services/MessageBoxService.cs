using AnnoDesigner.Core.Services;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoDesigner.Services;

public class MessageBoxService : IMessageBoxService
{
    public async void ShowMessage(object owner, string message, string title)
    {
        Wpf.Ui.Controls.MessageBox uiMessageBox = new()
        {
            Owner = owner as Window,
            Title = title,
            Content = message,
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "OK",
        };

        _ = await uiMessageBox.ShowDialogAsync();

    }

    public async void ShowWarning(object owner, string message, string title)
    {
        Wpf.Ui.Controls.MessageBox uiMessageBox = new()
        {
            Owner = owner as Window,
            Title = title,
            Content = message,
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "OK",
            PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Caution
        };

        _ = await uiMessageBox.ShowDialogAsync();
    }

    public async void ShowError(object owner, string message, string title)
    {
        Wpf.Ui.Controls.MessageBox uiMessageBox = new()
        {
            Owner = owner as Window,
            Title = title,
            Content = message,
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = "OK",
            PrimaryButtonAppearance = Wpf.Ui.Controls.ControlAppearance.Danger
        };

        _ = await uiMessageBox.ShowDialogAsync();
    }

    public async Task<bool> ShowQuestion(object owner, string message, string title)
    {
        Wpf.Ui.Controls.MessageBox uiMessageBox = new()
        {
            Owner = owner as Window,
            Title = title,
            Content = message,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = "No",
            PrimaryButtonText = "Yes",
        };

        Wpf.Ui.Controls.MessageBoxResult result = await uiMessageBox.ShowDialogAsync();

        return result == Wpf.Ui.Controls.MessageBoxResult.Primary;
    }

    public async Task<bool?> ShowQuestionWithCancel(object owner, string message, string title)
    {
        Wpf.Ui.Controls.MessageBox uiMessageBox = new()
        {
            Owner = owner as Window,
            Title = title,
            Content = message,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,

            SecondaryButtonText = "No",
            PrimaryButtonText = "Yes",
        };

        Wpf.Ui.Controls.MessageBoxResult result = await uiMessageBox.ShowDialogAsync();
        return result switch
        {
            Wpf.Ui.Controls.MessageBoxResult.Primary => true,
            Wpf.Ui.Controls.MessageBoxResult.Secondary => false,
            _ => null,
        };
    }


}
