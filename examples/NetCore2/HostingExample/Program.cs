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
        static async Task Main(string[] args)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            try
            {
                var hostBuilder = new HostBuilder().UseNLog().ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<IHostedService, ConsoleHostedService>();
                });
                await hostBuilder.RunConsoleAsync();
                Console.WriteLine("Press ANY key to exit");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public class ConsoleHostedService : Microsoft.Extensions.Hosting.IHostedService
        {
            readonly ILogger<ConsoleHostedService> _logger;

            public ConsoleHostedService(ILogger<ConsoleHostedService> logger)
            {
                _logger = logger;
                _logger.LogInformation("Created");
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Started");
                await Task.Yield();
            }

            public async Task StopAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Stopped");
                await Task.Yield();
            }
        }
    }
}
