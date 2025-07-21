using System;
using System.Threading.Tasks;
using AnnoDesigner.Core.Services;

namespace AnnoDesigner.Core.Extensions;

public static class MessageBoxServiceExtensions
{
    public static void ShowMessage(this IMessageBoxService service, string message, string title = "Information")
    {
        ArgumentNullException.ThrowIfNull(service);

        service.ShowMessage(null, message, title);
    }

    public static void ShowWarning(this IMessageBoxService service, string message, string title = "Warning")
    {
        ArgumentNullException.ThrowIfNull(service);

        service.ShowWarning(null, message, title);
    }

    public static void ShowError(this IMessageBoxService service, string message, string title = "Error")
    {
        ArgumentNullException.ThrowIfNull(service);

        service.ShowError(null, message, title);
    }

    public static Task<bool> ShowQuestion(this IMessageBoxService service, string message, string title = "Question")
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.ShowQuestion(null, message, title);
    }

    public static Task<bool?> ShowQuestionWithCancel(this IMessageBoxService service, string message, string title = "Question")
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.ShowQuestionWithCancel(null, message, title);
    }
}
