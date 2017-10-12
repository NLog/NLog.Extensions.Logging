using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace ConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var servicesProvider = BuildDi();
            var runner = servicesProvider.GetRequiredService<Runner>();

            runner.DoAction("Action1");

            Console.WriteLine("Press ANY key to exit");
            Console.ReadLine();
        }


        private static IServiceProvider BuildDi()
        {
            var services = new ServiceCollection();

            services.AddTransient<Runner>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            
            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            //configureNLog
            loggerFactory.AddNLog(new NLogProviderOptions { EnableStructuredLogging = true });
            loggerFactory.ConfigureNLog("nlog.config"); //todo add ${date}

            return serviceProvider;
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
        }


    }
}
