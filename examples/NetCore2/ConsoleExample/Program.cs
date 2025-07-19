using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ConsoleExample
{
    internal static class Program
    {
        private static void Main()
        {
            // Disposing the ServiceProvider will also flush / dispose / shutdown the NLog Logging Provider
            using var servicesProvider = new ServiceCollection()
                .AddTransient<Runner>() // Runner is the custom class
                .AddLogging(loggingBuilder =>
                {
                    // configure Logging with NLog
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddNLog();
                }).BuildServiceProvider();

            var runner = servicesProvider.GetRequiredService<Runner>();
            runner.DoAction("Action1");

            Console.WriteLine("Press ANY key to exit");
            Console.ReadKey();
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
