namespace MailTerm.Server.Interfaces;

public interface ISmtpCommandHandler
{
    string? HandleCommand(string line, string attachmentsSaveFilePath);
}