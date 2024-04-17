using System.Collections.Specialized;
using MailTerm.Server.Interfaces;
using Spectre.Console;

namespace MailTerm.Console;

record Option(string Text, Action Action);

public class ConsoleRenderer
{
    private readonly IMailManager _mailManager;

    public ConsoleRenderer(IMailManager mailManager)
    {
        _mailManager = mailManager;
        _mailManager.EmailQueue.CollectionChanged += EmailQueueChanged;
    }

    private void EmailQueueChanged(object? o, NotifyCollectionChangedEventArgs args) => RefreshConsole();

    private void RefreshConsole()
    {
        UpdateEmailTable();
        CreateOptions();
    }

    private void UpdateEmailTable()
    {
        AnsiConsole.Clear();

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

    private void CreateOptions()
    {
        var rule = new Rule("Options")
        {
            Justification = Justify.Left
        };

        AnsiConsole.Write(rule);

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<Option>()
                .AddChoices([
                    new("Refresh", RefreshConsole),
                    new("Clear Emails", () => _mailManager.ClearEmails())
                ])
                .UseConverter(x => x.Text));

        selection.Action();
    }
}