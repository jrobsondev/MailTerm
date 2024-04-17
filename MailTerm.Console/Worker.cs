using MailTerm.Server.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MailTerm.Console;

public class Worker(
    ILogger<Worker> _logger,
    ISmtpServer _server,
    CommandLineOptions _commandLineOptions,
    IMailManager _mailManager)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting worker");
        var _ = new ConsoleRenderer(_mailManager);
        await _server.StartServerAsync("127.0.0.1", _commandLineOptions.Port,
            _commandLineOptions.AttachmentFilePath!,
            cancellationToken);
        _logger.LogInformation("Stopping worker");
    }
}