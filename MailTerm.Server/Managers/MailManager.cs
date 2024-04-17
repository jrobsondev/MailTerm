using System.Collections.ObjectModel;
using System.Text;
using MailTerm.Server.Interfaces;
using MailTerm.Server.Models;
using MimeKit;

namespace MailTerm.Server.Managers;

public class MailManager : IMailManager
{
    public ObservableCollection<IEmail> EmailQueue { get; } = new();

    public void ConvertStringToEmailAndAddToQueue(string email, string? attachmentFilePath = null)
    {
        var message = MimeMessage.Load(new MemoryStream(Encoding.UTF8.GetBytes(email)));
        EmailQueue.Add(new Email
        {
            From = new EmailAccount(message.From.Mailboxes.First()),
            To = new EmailAccount(message.To.Mailboxes.First()),
            Subject = message.Subject,
            Body = message.TextBody,
            AttachmentPath = attachmentFilePath
        });
    }

    public void ClearEmails() => EmailQueue.Clear();
}