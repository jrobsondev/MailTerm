using System.Text;
using MailTerm.Server.Interfaces;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace MailTerm.Server;

public class SmtpCommandHandler : ISmtpCommandHandler
{
    private readonly ILogger<SmtpCommandHandler> _logger;
    private readonly IMailManager _mailManager;
    private bool _isDataCommandReceived;
    private readonly StringBuilder _dataBuilder = new();

    public SmtpCommandHandler(ILogger<SmtpCommandHandler> logger, IMailManager mailManager)
    {
        _logger = logger;
        _mailManager = mailManager;
    }

    public string? HandleCommand(string line, string attachmentsSaveFilePath)
    {
        if (_isDataCommandReceived)
        {
            return HandleDataCommand(line, attachmentsSaveFilePath);
        }

        if (line.StartsWith("HELO", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase))
        {
            return $"250 Hello {line.Split(' ')[1]}, pleased to meet you";
        }

        if (line.StartsWith("DATA", StringComparison.OrdinalIgnoreCase))
        {
            _isDataCommandReceived = true;
            return "354 Start mail input; end with <CRLF>.<CRLF>";
        }

        if (line.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase))
        {
            return "221 Bye";
        }

        return "250 OK";
    }

    private string? HandleDataCommand(string line, string attachmentsSaveFilePath)
    {
        if (line.Equals("."))
        {
            _isDataCommandReceived = false;
            var messageProcessed = ProcessData(_dataBuilder.ToString(), attachmentsSaveFilePath);
            _dataBuilder.Clear();
            return messageProcessed ? "250 OK: Message received" : "550 Failed to process message";
        }

        _dataBuilder.AppendLine(line);
        return null; // No response yet
    }

    private bool ProcessData(string data, string attachmentsSaveFilePath)
    {
        try
        {
            var message = MimeMessage.Load(new MemoryStream(Encoding.UTF8.GetBytes(data)));
            foreach (var attachment in message.Attachments)
            {
                if (!(attachment is MimePart mimePart)) continue;

                var fileName = Path.Combine(attachmentsSaveFilePath, mimePart.FileName);
                if (!Directory.Exists(attachmentsSaveFilePath))
                    Directory.CreateDirectory(attachmentsSaveFilePath);

                using var fileStream = File.Create(fileName);
                mimePart.Content.DecodeTo(fileStream);
                _mailManager.ConvertStringToEmailAndAddToQueue(data, fileName);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to process email data: {ExMessage}", ex.Message);
            return false;
        }
    }
}