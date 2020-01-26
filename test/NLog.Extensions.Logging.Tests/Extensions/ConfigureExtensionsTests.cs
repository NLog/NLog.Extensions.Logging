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
        [Obsolete("Instead use ILoggingBuilder.AddNLog() or IHostBuilder.UseNLog()")]
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
            AssertSingleMessage(memoryTarget, "Info|test message with 1 arg");
        }

        [Theory]
        [InlineData("EventId", "eventId2")]
        [InlineData("EventId_Name", "eventId2")]
        [InlineData("EventId_Id", "2")]
        [Obsolete("Instead use ILoggingBuilder.AddNLog() or IHostBuilder.UseNLog()")]
        public void AddNLog_LoggerFactory_LogInfoWithEventId_ShouldLogToNLogWithEventId(string eventPropery, string expectedEventInLog)
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var config = CreateConfigWithMemoryTarget(out var memoryTarget, $"${{event-properties:{eventPropery}}} - ${{message}}");

            // Act
            loggerFactory.AddNLog(new NLogProviderOptions { EventIdSeparator = "_" });
            LogManager.Configuration = config;
            var logger = loggerFactory.CreateLogger("logger1");
            logger.LogInformation(new EventId(2, "eventId2"), "test message with {0} arg", 1);

            // Assert
            AssertSingleMessage(memoryTarget, $"{expectedEventInLog} - test message with 1 arg");
        }

#if !NETCOREAPP1_1 && !NET452
        [Fact]
        public void AddNLog_LoggingBuilder_LogInfo_ShouldLogToNLog()
        {
            // Arrange
            ILoggingBuilder builder = new LoggingBuilderStub();
            var config = CreateConfigWithMemoryTarget(out var memoryTarget);

            // Act
            builder.AddNLog(config);
            var provider = GetLoggerProvider(builder);
            var logger = provider.CreateLogger("logger1");

            logger.LogInformation("test message with {0} arg", 1);

            // Assert
            AssertSingleMessage(memoryTarget, "Info|test message with 1 arg");
        }

        [Theory]
        [InlineData("EventId", "eventId2")]
        [InlineData("EventId_Name", "eventId2")]
        [InlineData("EventId_Id", "2")]
        public void AddNLog_LoggingBuilder_LogInfoWithEventId_ShouldLogToNLogWithEventId(string eventPropery, string expectedEventInLog)
        {
            // Arrange
            ILoggingBuilder builder = new LoggingBuilderStub();
            var options = new NLogProviderOptions { EventIdSeparator = "_" };
            var config = CreateConfigWithMemoryTarget(out var memoryTarget, $"${{event-properties:{eventPropery}}} - ${{message}}");

            // Act
            builder.AddNLog(options, config);
            var provider = GetLoggerProvider(builder);
            var logger = provider.CreateLogger("logger1");
            logger.LogInformation(new EventId(2, "eventId2"), "test message with {0} arg", 1);

            // Assert
            AssertSingleMessage(memoryTarget, $"{expectedEventInLog} - test message with 1 arg");
        }

        [Fact]
        public void AddNLog_LogFactoryBuilder_LogInfo_ShouldLogToNLog()
        {
            // Arrange
            ILoggingBuilder builder = new LoggingBuilderStub();

            // Act
            MemoryTarget memoryTarget = null;
            builder.AddNLog(ServiceProvider => CreateConfigWithMemoryTarget(out memoryTarget, logFactory: new NLog.LogFactory()).LogFactory);
            var provider = GetLoggerProvider(builder);
            var logger = provider.CreateLogger("logger1");

            logger.LogInformation("test message with {0} arg", 1);

            // Assert
            AssertSingleMessage(memoryTarget, "Info|test message with 1 arg");
        }

        private static ILoggerProvider GetLoggerProvider(ILoggingBuilder builder)
        {
            var services = builder.Services;
            var serviceProvider = services.BuildServiceProvider();
            var provider = serviceProvider.GetRequiredService<ILoggerProvider>();
            return provider;
        }

        internal class LoggingBuilderStub : ILoggingBuilder
        {
            public IServiceCollection Services { get; set; } = new ServiceCollection();
        }
#endif

        private static void AssertSingleMessage(MemoryTarget memoryTarget, string expectedMessage)
        {
            Assert.Equal(1, memoryTarget.Logs.Count);
            var log = memoryTarget.Logs.Single();
            Assert.Equal(expectedMessage, log);
        }

        private static LoggingConfiguration CreateConfigWithMemoryTarget(out MemoryTarget memoryTarget, string levelMessage = "${level}|${message}", LogFactory logFactory = null)
        {
            var config = new LoggingConfiguration(logFactory);
            memoryTarget = new MemoryTarget { Layout = levelMessage };
            config.AddRuleForAllLevels(memoryTarget);
            if (logFactory != null)
                logFactory.Configuration = config;
            return config;
        }
    }
}
