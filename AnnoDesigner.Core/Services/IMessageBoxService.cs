using System.Threading.Tasks;

namespace AnnoDesigner.Core.Services;

public interface IMessageBoxService
{
    void ShowMessage(object owner, string message, string title);

    void ShowWarning(object owner, string message, string title);

    void ShowError(object owner, string message, string title);

    Task<bool> ShowQuestion(object owner, string message, string title);

    Task<bool?> ShowQuestionWithCancel(object owner, string message, string title);
}
