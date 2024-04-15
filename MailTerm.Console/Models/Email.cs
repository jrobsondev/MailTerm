using MailTerm.Console.Interfaces;

namespace MailTerm.Console.Models;

public class Email : IEmail
{
    public IEmailAccount From { get; set; }
    public IEmailAccount To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string? AttachmentPath { get; set; }

    public bool HasAttachment
    {
        get => !string.IsNullOrEmpty(AttachmentPath);
    }
}