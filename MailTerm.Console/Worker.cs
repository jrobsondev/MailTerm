using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MailTerm.Console;

public class Worker(ILogger<Worker> logger, SmtpServer server) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting worker");
        await server.StartServerAsync("127.0.0.1", 9999, cancellationToken);
        logger.LogInformation("Stopping worker");
    }
}