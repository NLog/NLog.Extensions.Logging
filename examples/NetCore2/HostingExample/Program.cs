using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Hosting;

namespace HostingExample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // Setup NLog and load configuration from appsettings.json
            builder.Logging.ClearProviders();
            builder.UseNLog();

            builder.Services.AddHostedService<ConsoleHostedService>();
            using var host = builder.Build();
            await host.RunAsync();
        }

        public class ConsoleHostedService : BackgroundService
        {
            private readonly ILogger<ConsoleHostedService> _logger;

            public ConsoleHostedService(ILogger<ConsoleHostedService> logger)
            {
                _logger = logger;
                _logger.LogInformation("ConsoleHostedService instance created...");
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                _logger.LogInformation("Hello from your hosted service thread!");
                _logger.LogTrace("I may or may not return for a long time depending on what I do.");
                _logger.LogDebug("In this example, I return right away, but my host will continue to run until");
                _logger.LogInformation("its CancellationToken is Cancelled (SIGTERM(Ctrl-C) or a Lifetime Event )");
                await Task.CompletedTask;
            }
        }
    }
}
