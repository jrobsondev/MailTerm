using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MailTerm.Console;

public class Worker(ILogger<Worker> _logger, SmtpServer _server, CommandLineOptions _commandLineOptions)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting worker");
        await _server.StartServerAsync("127.0.0.1", _commandLineOptions.Port,
            _commandLineOptions.AttachmentFilePath!,
            cancellationToken);
        _logger.LogInformation("Stopping worker");
    }
}