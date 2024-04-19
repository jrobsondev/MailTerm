using System.Net;
using System.Net.Sockets;
using MailTerm.Server.Interfaces;
using Microsoft.Extensions.Logging;

namespace MailTerm.Server;

public class SmtpServer : ISmtpServer
{
    private readonly ILogger<SmtpServer> _logger;
    private readonly ISmtpCommandHandler _smtpCommandHandler;
    private TcpListener _tcpListener;

    public event EventHandler EmailReceived;

    public SmtpServer(ILogger<SmtpServer> logger, ISmtpCommandHandler smtpCommandHandler)
    {
        _logger = logger;
        _smtpCommandHandler = smtpCommandHandler;
    }

    public async Task StartServerAsync(string hostAddress, int port, string attachmentsSaveFilePath,
        CancellationToken cancellationToken)
    {
        _tcpListener = new TcpListener(IPAddress.Parse(hostAddress), port);
        _tcpListener.Start();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
                _ = ProcessClientAsync(client, attachmentsSaveFilePath, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogCritical("Program has been suspended");
        }
        finally
        {
            _tcpListener.Stop();
            _logger.LogInformation("Smtp Server stopped");
        }
    }

    private async Task ProcessClientAsync(TcpClient client, string attachmentsSaveFilePath,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var networkStream = client.GetStream();
            using var reader = new StreamReader(networkStream);
            await using var writer = new StreamWriter(networkStream) { AutoFlush = true };

            await writer.WriteLineAsync($"220 {_tcpListener.LocalEndpoint} MailTerm Server");

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (line is null) continue;

                var response = _smtpCommandHandler.HandleCommand(line, attachmentsSaveFilePath);
                if (response is not null)
                {
                    await writer.WriteLineAsync(response);
                }

                if (response is not null && response.StartsWith("221")) // QUIT command response
                {
                    EmailReceived?.Invoke(this, EventArgs.Empty);
                    break;
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