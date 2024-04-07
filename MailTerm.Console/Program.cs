using CommandLine;
using MailTerm.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await CreateHostBuilder(args)
    .Build()
    .RunAsync();
return;

static IHostBuilder CreateHostBuilder(string[] args)
    => Host.CreateDefaultBuilder(args)
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
            services.AddSingleton<SmtpServer>();
            services.AddSingleton(commandLineOptions);
        });