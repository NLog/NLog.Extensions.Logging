using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Targets;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class LoggerTests
    {
        private static Lazy<IServiceProvider> ServiceProvider = new Lazy<IServiceProvider>(BuildDi);

        public LoggerTests()
        {
            var target = GetTarget();
            target?.Logs.Clear();
        }

        [Fact]
        public void TestInit()
        {
            GetRunner().Init();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|init runner |0", target.Logs.FirstOrDefault());
           
        }

        [Fact]
        public void TestEventId()
        {
            GetRunner().LogDebugWithId();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id |20", target.Logs.FirstOrDefault());
           
        }

        private static Runner GetRunner()
        {
            var serviceProvider = ServiceProvider.Value;

            // Start program
            var runner = serviceProvider.GetRequiredService<Runner>();
            return runner;
        }

        private static MemoryTarget GetTarget()
        {
            var target = LogManager.Configuration.FindTargetByName<MemoryTarget>("target1");
            return target;
        }





        private static IServiceProvider BuildDi()
        {
            var services = new ServiceCollection();

            services.AddTransient<Runner>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();

            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            loggerFactory.AddNLog();
            loggerFactory.ConfigureNLog("nlog.config");
            return serviceProvider;
        }


        public class Runner
        {
            private readonly ILogger<Runner> _logger;

            public Runner(ILoggerFactory fac)
            {
                _logger = fac.CreateLogger<Runner>();
            }


            public void LogDebugWithId()
            {
                _logger.LogDebug(20, "message with id");
            }

            public void LogDebugWithParameters()
            {
                _logger.LogDebug(20, "message with id and {0} parameters", 1);
            }

            public void LogWithScope()
            {
                using (_logger.BeginScope("scope1"))
                {
                    _logger.LogDebug(20, "message with id and {0} parameters", 1);
                }
            }

            public void Init()
            {
                _logger.LogDebug("init runner");

            }
        }
    }
}
