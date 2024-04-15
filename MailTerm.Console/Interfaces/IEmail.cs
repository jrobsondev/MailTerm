namespace MailTerm.Console.Interfaces;

public interface IEmail
{
    IEmailAccount From { get; set; }
    IEmailAccount To { get; set; }
    string Subject { get; set; }
    string Body { get; set; }
    string? AttachmentPath { get; set; }
    bool HasAttachment { get; }
}