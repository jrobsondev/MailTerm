namespace MailTerm.Server.Interfaces;

public interface ISmtpServer
{
    Task StartServerAsync(string hostAddress, int port, string attachmentsSaveFilePath,
        CancellationToken cancellationToken);
}