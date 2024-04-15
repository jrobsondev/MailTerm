using System.Collections.Specialized;
using MailTerm.Console.Managers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace MailTerm.Console;

public class Worker(
    ILogger<Worker> _logger,
    SmtpServer _server,
    CommandLineOptions _commandLineOptions,
    MailManager _mailManager)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting worker");

        //Test code, move somewhere better
        _mailManager.EmailQueue.CollectionChanged += EmailQueueChanged;

        await _server.StartServerAsync("127.0.0.1", _commandLineOptions.Port,
            _commandLineOptions.AttachmentFilePath!,
            cancellationToken);
        _logger.LogInformation("Stopping worker");
    }

    private void EmailQueueChanged(object? o, NotifyCollectionChangedEventArgs args)
    {
        AnsiConsole.Clear();

        var table = new Table();

        table.AddColumn("From");
        table.AddColumn("To");
        table.AddColumn("Subj.");
        table.AddColumn("Body");
        table.AddColumn("Att.");

        foreach (var email in _mailManager.EmailQueue)
        {
            table.AddRow(email.From.Name, email.To.Name, email.Subject, email.Body, email.HasAttachment.ToString());
        }

        AnsiConsole.Write(table);
    }
}