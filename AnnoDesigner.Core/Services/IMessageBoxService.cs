namespace AnnoDesigner.Core.Services
{
    public interface IMessageBoxService
    {
        void ShowMessage(object owner, string message, string title);

        void ShowWarning(object owner, string message, string title);

        void ShowError(object owner, string message, string title);

        bool ShowQuestion(object owner, string message, string title);
    }
}
