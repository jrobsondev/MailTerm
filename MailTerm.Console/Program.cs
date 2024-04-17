using CommandLine;
using MailTerm.Console;
using MailTerm.Server;
using MailTerm.Server.Interfaces;
using MailTerm.Server.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await CreateHostBuilder(args)
    .Build()
    .RunAsync();
return;

static IHostBuilder CreateHostBuilder(string[] args)
    => Host.CreateDefaultBuilder(args)
        .ConfigureLogging((ctx, b) => b.SetMinimumLevel(LogLevel.Warning))
        .ConfigureServices(services =>
        {
            var commandLineOptions = new CommandLineOptions();
            Parser.Default.ParseArguments<CommandLineOptions>(new[] { "-a /Users/jake/Desktop" })
                .WithParsed(o =>
                {
                    var path = o.AttachmentFilePath ?? Path.Combine(Directory.GetCurrentDirectory(), "Attachments");
                    o.AttachmentFilePath = path.Trim();
                    commandLineOptions = o;
                }).WithNotParsed(errors => { Environment.Exit(0); });

            services.AddHostedService<Worker>();
            services.AddSingleton<ISmtpServer, SmtpServer>();
            services.AddSingleton<IMailManager, MailManager>();
            services.AddSingleton(commandLineOptions);
        });