﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Hosting;

namespace HostingExample
{
    public class Program
    {
        private static async Task Main()
        {
            var config = new ConfigurationBuilder().Build();

            var logger = LogManager.Setup()
                                   .SetupExtensions(ext => ext.RegisterHostSettings(config))
                                   .GetCurrentClassLogger();

            try
            {
                var hostBuilder = new HostBuilder()
                    .ConfigureLogging(builder => builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace))
                    .ConfigureServices((hostContext, services) => services.AddHostedService<ConsoleHostedService>())
                    .UseNLog();

                // Build and run the host in one go; .RCA is specialized for running it in a console.
                // It registers SIGTERM(Ctrl-C) to the CancellationTokenSource that's shared with all services in the container.
                await hostBuilder.RunConsoleAsync();

                Console.WriteLine("The host container has terminated. Press ANY key to exit the console.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                // NLog: catch setup errors (exceptions thrown inside of any containers may not necessarily be caught)
                logger.Fatal(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
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
