using MailTerm.Server.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MailTerm.Console;

public class Worker(
    ILogger<Worker> _logger,
    ISmtpServer _smtpServer,
    CommandLineOptions _commandLineOptions,
    IMailManager _mailManager)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var _ = new ConsoleRenderer(_smtpServer, _mailManager, _commandLineOptions, cancellationToken);
        await _smtpServer.StartServerAsync("127.0.0.1", _commandLineOptions.Port,
            _commandLineOptions.AttachmentFilePath!, cancellationToken);
    }
}