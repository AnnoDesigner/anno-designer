using System;
using AnnoDesigner.Core.Services;

namespace AnnoDesigner.Core.Extensions
{
    public static class MessageBoxServiceExtensions
    {
        public static void ShowMessage(this IMessageBoxService service, string message)
        {
            ShowMessage(service, message, "Information");
        }

        public static void ShowMessage(this IMessageBoxService service, string message, string title)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            service.ShowMessage(null, message, title);
        }

        public static void ShowWarning(this IMessageBoxService service, string message)
        {
            ShowWarning(service, message, "Warning");
        }

        public static void ShowWarning(this IMessageBoxService service, string message, string title)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            service.ShowWarning(null, message, title);
        }

        public static void ShowError(this IMessageBoxService service, string message)
        {
            ShowError(service, message, "Error");
        }

        public static void ShowError(this IMessageBoxService service, string message, string title)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            service.ShowError(null, message, title);
        }

        public static void ShowQuestion(this IMessageBoxService service, string message)
        {
            ShowQuestion(service, message, "Question");
        }

        public static bool ShowQuestion(this IMessageBoxService service, string message, string title)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return service.ShowQuestion(null, message, title);
        }
    }
}
