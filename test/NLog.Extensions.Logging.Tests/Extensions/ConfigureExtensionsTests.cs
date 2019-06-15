using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using System;
using System.Linq;
using NLog.Targets;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace NLog.Extensions.Logging.Tests.Extensions
{
    public class ConfigureExtensionsTests
    {
        [Fact]
        public void AddNLog_LoggerFactory_LogInfo_ShouldLogToNLog()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var config = CreateConfigWithMemoryTarget(out var memoryTarget);

            // Act
            loggerFactory.AddNLog();
            LogManager.Configuration = config;
            var logger = loggerFactory.CreateLogger("logger1");
            logger.LogInformation("test message with {0} arg", 1);

            // Assert
            Assert.Equal(1, memoryTarget.Logs.Count);
            var log = memoryTarget.Logs.Single();
            Assert.Equal("Info|test message with 1 arg", log);

        }

        [Theory]
        [InlineData("EventId", "eventId2")]
        [InlineData("EventId_Name", "eventId2")]
        [InlineData("EventId_Id", "2")]
        public void AddNLog_LoggerFactory_LogInfoWithEventId_ShouldLogToNLogWithEventId(string eventPropery, string expectedEventInLog)
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var config = CreateConfigWithMemoryTarget(out var memoryTarget, $"${{event-properties:{eventPropery}}} - ${{message}}");

            // Act
            loggerFactory.AddNLog(new NLogProviderOptions() { EventIdSeparator = "_" });
            LogManager.Configuration = config;
            var logger = loggerFactory.CreateLogger("logger1");
            logger.LogInformation(new EventId(2, "eventId2"), "test message with {0} arg", 1);

            // Assert
            Assert.Equal(1, memoryTarget.Logs.Count);
            var log = memoryTarget.Logs.Single();
            Assert.Equal($"{expectedEventInLog} - test message with 1 arg", log);

        }

#if !NETCOREAPP1_1 && !NET452

        [Fact]
        public void AddNLog_LogginBuilder_LogInfo_ShouldLogToNLog()
        {
            // Arrange
            ILoggingBuilder builder = new LoggingBuilderStub();
            var config = CreateConfigWithMemoryTarget(out var memoryTarget);

            // Act
            builder.AddNLog();
            LogManager.Configuration = config;
            var services = builder.Services;
            var serviceProvider = services.BuildServiceProvider();
            var provider = serviceProvider.GetRequiredService<ILoggerProvider>();
            var logger = provider.CreateLogger("logger1");

            logger.LogInformation("test message with {0} arg", 1);

            // Assert
            Assert.Equal(1, memoryTarget.Logs.Count);
            var log = memoryTarget.Logs.Single();
            Assert.Equal("Info|test message with 1 arg", log);
        }

        internal class LoggingBuilderStub : ILoggingBuilder
        {
            public IServiceCollection Services { get; set; } = new ServiceCollection();
        }
#endif

        private static LoggingConfiguration CreateConfigWithMemoryTarget(out MemoryTarget memoryTarget, string levelMessage = "${level}|${message}")
        {
            var config = new LoggingConfiguration();
            memoryTarget = new MemoryTarget { Layout = levelMessage };
            config.AddRuleForAllLevels(memoryTarget);
            return config;
        }

    }
}
