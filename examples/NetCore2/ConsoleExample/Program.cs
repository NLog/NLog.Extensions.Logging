using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace ConsoleExample
{
    internal static class Program
    {
        private static void Main()
        {
            var config = new ConfigurationBuilder().Build();

            var logger = LogManager.Setup()
                                   .SetupExtensions(ext => ext.RegisterConfigSettings(config))
                                   .GetCurrentClassLogger();

            try
            {
                using var servicesProvider = new ServiceCollection()
                    .AddTransient<Runner>() // Runner is the custom class
                    .AddLogging(loggingBuilder =>
                    {
                        // configure Logging with NLog
                        loggingBuilder.ClearProviders();
                        loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                        loggingBuilder.AddNLog(config);
                    }).BuildServiceProvider();

                var runner = servicesProvider.GetRequiredService<Runner>();
                runner.DoAction("Action1");

                Console.WriteLine("Press ANY key to exit");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }
    }

    public class Runner
    {
        private readonly ILogger<Runner> _logger;

        public Runner(ILogger<Runner> logger)
        {
            _logger = logger;
        }

        public void DoAction(string name)
        {
            _logger.LogDebug(20, "Doing hard work! {Action}", name);
            _logger.LogInformation(21, "Doing hard work! {Action}", name);
            _logger.LogWarning(22, "Doing hard work! {Action}", name);
            _logger.LogError(23, "Doing hard work! {Action}", name);
            _logger.LogCritical(24, "Doing hard work! {Action}", name);
        }
    }
}
