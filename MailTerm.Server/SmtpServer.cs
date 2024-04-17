using System.Net;
using System.Net.Sockets;
using System.Text;
using MailTerm.Server.Interfaces;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace MailTerm.Server;

public class SmtpServer(ILogger<SmtpServer> _logger, IMailManager _mailManager) : ISmtpServer
{
    private TcpListener? _tcpListener { get; set; }

    public async Task StartServerAsync(string hostAddress, int port, string attachmentsSaveFilePath,
        CancellationToken cancellationToken)
    {
        var ipAddress = IPAddress.Parse(hostAddress);
        _tcpListener = new(ipAddress, port);
        _tcpListener.Start();

        try
        {
            while (cancellationToken.IsCancellationRequested is false)
            {
                var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
                _ = ProcessClientAsync(client, attachmentsSaveFilePath);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException operationCanceledException)
        {
            _logger.LogCritical("Program has been suspended");
        }
        finally
        {
            _tcpListener.Stop();
            _logger.LogInformation("Smtp Server stopped");
        }
    }

    private async Task ProcessClientAsync(TcpClient client, string attachmentsSaveFilePath)
    {
        var isDataCommandReceived = false;
        var dataBuilder = new StringBuilder();

        try
        {
            await using var networkStream = client.GetStream();
            using var reader = new StreamReader(networkStream);
            await using var writer = new StreamWriter(networkStream) { AutoFlush = true };

            await writer.WriteLineAsync($"220 {_tcpListener.LocalEndpoint} MailTerm Server");

            while (true)
            {
                var line = await reader.ReadLineAsync();

                if (isDataCommandReceived)
                {
                    if (line.Equals("."))
                    {
                        isDataCommandReceived = false;
                        try
                        {
                            var emailData = dataBuilder.ToString();
                            var message =
                                await MimeMessage.LoadAsync(new MemoryStream(Encoding.UTF8.GetBytes(emailData)));
                            foreach (var attachment in message.Attachments)
                            {
                                if (attachment is not MimePart mimePart)
                                {
                                    continue;
                                }

                                var fileName = Path.Combine(attachmentsSaveFilePath, mimePart.FileName);

                                if (Directory.Exists(attachmentsSaveFilePath) is false)
                                {
                                    Directory.CreateDirectory(attachmentsSaveFilePath);
                                }

                                await using var fileStream = File.Create(fileName);
                                await mimePart.Content.DecodeToAsync(fileStream);
                                _mailManager.ConvertStringToEmailAndAddToQueue(dataBuilder.ToString(), fileName);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Failed to process email data: {ExMessage}", ex.Message);
                        }

                        dataBuilder.Clear();
                        await writer.WriteLineAsync("250 OK: Message received");
                    }
                    else
                    {
                        dataBuilder.AppendLine(line);
                    }
                }
                else if (line.StartsWith("HELO", StringComparison.OrdinalIgnoreCase) ||
                         line.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase))
                {
                    await writer.WriteLineAsync($"250 Hello {line.Split(' ')[1]}, pleased to meet you");
                }
                else if (line.StartsWith("DATA", StringComparison.OrdinalIgnoreCase))
                {
                    isDataCommandReceived = true;
                    await writer.WriteLineAsync("354 Start mail input; end with <CRLF>.<CRLF>");
                }
                else if (line.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    await writer.WriteLineAsync("221 Bye");
                    break;
                }
                else
                {
                    await writer.WriteLineAsync("250 OK");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Client processing failed: {ExMessage}", ex.Message);
        }
        finally
        {
            client.Close();
        }
    }
}