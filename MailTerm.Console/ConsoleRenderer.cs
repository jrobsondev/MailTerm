using MailTerm.Server.Interfaces;
using Spectre.Console;

namespace MailTerm.Console;

internal record Option(string Text, Action Action);

public class ConsoleRenderer
{
    private readonly IMailManager _mailManager;

    private CancellationTokenSource? _cts;

    public ConsoleRenderer(IMailManager mailManager, ISmtpServer smtpServer)
    {
        _mailManager = mailManager;
        smtpServer.EmailReceived += async (sender, args) => await RefreshConsoleAsync();
    }

    public async Task RefreshConsoleAsync()
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
        }

        AnsiConsole.Clear();
        var hasMail = _mailManager.EmailQueue.Any();
        if (!hasMail)
        {
            AnsiConsole.Write(new FigletText("MailTerm").LeftJustified().Color(Color.Yellow));
        }

        UpdateEmailTable();
        if (hasMail)
        {
            await CreateOptions();
        }
    }

    private void UpdateEmailTable()
    {
        var table = new Table();

        table.AddColumn("From");
        table.AddColumn("To");
        table.AddColumn("Subject");
        table.AddColumn("Body");
        table.AddColumn("Attachments");

        foreach (var email in _mailManager.EmailQueue)
        {
            table.AddRow(new Text(email.From.Name),
                new Text(email.To.Name),
                new Text(email.Subject),
                new Text(email.Body),
                new Markup($"file://{email.AttachmentPath}"));
        }

        AnsiConsole.Write(table);
    }

    private async Task CreateOptions()
    {
        var rule = new Rule("Options")
        {
            Justification = Justify.Left
        };

        AnsiConsole.Write(rule);

        _cts = new();
        try
        {
            var selection = await new SelectionPrompt<Option>()
                .AddChoices([
                    new("Refresh", async () => await RefreshConsoleAsync()),
                    new("Clear Emails", async () => await ClearEmailsAsync())
                ])
                .UseConverter(x => x.Text)
                .ShowAsync(AnsiConsole.Console, _cts.Token);

            selection?.Action();
        }
        catch (TaskCanceledException ex)
        {
            //Need to catch cancellation exceptions as we don't mind them
        }
    }

    private async Task ClearEmailsAsync()
    {
        _mailManager.ClearEmails();
        await RefreshConsoleAsync();
    }
}