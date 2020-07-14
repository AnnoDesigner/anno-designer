using System;
using AnnoDesigner.Core.Services;

namespace AnnoDesigner.Core.Extensions
{
    public static class MessageBoxServiceExtensions
    {
        public static void ShowMessage(this IMessageBoxService service, string message, string title = "Information")
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            service.ShowMessage(null, message, title);
        }

        public static void ShowWarning(this IMessageBoxService service, string message, string title = "Warning")
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            service.ShowWarning(null, message, title);
        }

        public static void ShowError(this IMessageBoxService service, string message, string title = "Error")
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            service.ShowError(null, message, title);
        }


        public static bool ShowQuestion(this IMessageBoxService service, string message, string title = "Question")
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return service.ShowQuestion(null, message, title);
        }
    }
}
