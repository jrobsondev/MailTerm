using MailTerm.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await CreateHostBuilder(args).Build().RunAsync();
return;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddHostedService<Worker>();
            services.AddSingleton<SmtpServer>();
        });