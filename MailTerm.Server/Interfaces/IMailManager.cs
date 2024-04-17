using System.Collections.ObjectModel;

namespace MailTerm.Server.Interfaces;

public interface IMailManager
{
    ObservableCollection<IEmail> EmailQueue { get; }
    void ConvertStringToEmailAndAddToQueue(string email, string? attachmentFilePath = null);
    void ClearEmails();
}