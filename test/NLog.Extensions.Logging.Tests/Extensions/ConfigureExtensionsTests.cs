using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Targets;
using Xunit;

namespace NLog.Extensions.Logging.Tests.Extensions
{
    public class ConfigureExtensionsTests
    {
#if NET5_0_OR_GREATER
        [Fact]
        public void AddNLog_LoggerFactory_IncludeActivityIdsWithBeginScope()
        {
            // Arrange
            var loggerFactory = LoggerFactory.Create(builder => builder.AddNLog(new NLogProviderOptions { IncludeActivityIdsWithBeginScope = true }));
            var config = CreateConfigWithMemoryTarget(out var memoryTarget, $"${{scopeproperty:ParentId}} - ${{message}}");

            // Act
            LogManager.Configuration = config;
            var logger = loggerFactory.CreateLogger(nameof(AddNLog_LoggerFactory_IncludeActivityIdsWithBeginScope));
            var activity = new System.Diagnostics.Activity("TestActivity").SetParentId("42").Start();
            var scopeProperties = new Dictionary<string, object> { { "RequestId", "123" }, { "RequestPath", "Unknown" } };
            using (logger.BeginScope(new ArraySegment<KeyValuePair<string, object>>(scopeProperties.ToArray())))
            {
                logger.LogInformation(default(EventId), "test message with {0} arg", 1);
            }

            // Assert
            AssertSingleMessage(memoryTarget, "42 - test message with 1 arg");
        }
#endif

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
        [InlineData("EventId", "eventId2", true)]
        [InlineData("EventName", "", true)]
        [InlineData("EventId_Name", "eventId2", true)]
        [InlineData("EventId_Id", "2", true)]
        [InlineData("EventId", "2", false)]
        [InlineData("EventName", "eventId2", false)]
        [InlineData("EventId_Id", "", false)]
        [InlineData("EventId_Name", "", false)]
        public void AddNLog_LoggingBuilder_LogInfoWithEventId_ShouldLogToNLogWithEventId(string eventPropery, string expectedEventInLog, bool captureEntireEventId)
        {
            // Arrange
            ILoggingBuilder builder = new LoggingBuilderStub();
            var config = CreateConfigWithMemoryTarget(out var memoryTarget, $"${{event-properties:{eventPropery}}} - ${{message}}");
            var options = new NLogProviderOptions { EventIdSeparator = "_", CaptureEventId = captureEntireEventId ? EventIdCaptureType.Legacy : (EventIdCaptureType.EventId | EventIdCaptureType.EventName) };

            // Act
            builder.AddNLog(config, options);
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

        [Fact]
        public void AddNLog_ReplaceLoggerFactory()
        {
            // Arrange
            ILoggingBuilder builder = new LoggingBuilderStub();

            // Act
            builder.AddNLog(new NLogProviderOptions() { ReplaceLoggerFactory = true, RemoveLoggerFactoryFilter = true });
            var loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();
            var loggerProvider = GetLoggerProvider(builder);

            // Assert
            Assert.Equal(typeof(NLogLoggerFactory), loggerFactory.GetType());
            Assert.Equal(typeof(NLogLoggerProvider), loggerProvider.GetType());
        }

        [Fact]
        public void AddNLog_ServiceCollection_ReplaceLoggerFactory()
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            collection.AddNLog(new NLogProviderOptions() { ReplaceLoggerFactory = true });

            // Assert
            using var provider = collection.BuildServiceProvider();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
            Assert.Equal(typeof(NLogLoggerFactory), loggerFactory.GetType());
        }

        [Fact]
        public void AddNLog_ServiceCollection_ShouldLog()
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            var nlogTarget = new Targets.MemoryTarget() { Name = "Output" };
            collection.AddNLog(new NLogProviderOptions(), (ServiceProvider) =>
            {
                var nLogFactory = new LogFactory().Setup().LoadConfiguration(c => c.ForLogger().WriteTo(nlogTarget)).LogFactory;
                return nLogFactory;
            });

            // Assert
            using var provider = collection.BuildServiceProvider();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);

            var logger = loggerFactory.CreateLogger("Hello");
            logger.LogCritical("World");
            Assert.Single(nlogTarget.Logs);
        }

        [Fact]
        public void AddNLog_ArgumentNullException()
        {
            ILoggingBuilder loggingBuilder = null;
            var argNulLException = Assert.Throws<ArgumentNullException>(() => loggingBuilder.AddNLog());
            Assert.Equal("builder", argNulLException.ParamName);
        }

        [Fact]
        public void AddNLog_WithConfig_ReplaceLoggerFactory()
        {
            // Arrange
            ILoggingBuilder builder = new LoggingBuilderStub();
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig["Logging:NLog:ReplaceLoggerFactory"] = "True";
            memoryConfig["Logging:NLog:RemoveLoggerFactoryFilter"] = "True";
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();

            // Act
            builder.AddNLog(configuration);
            var loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();
            var loggerProvider = GetLoggerProvider(builder);

            // Assert
            Assert.Equal(typeof(NLogLoggerFactory), loggerFactory.GetType());
            Assert.Equal(typeof(NLogLoggerProvider), loggerProvider.GetType());
        }

        private static ILoggerProvider GetLoggerProvider(ILoggingBuilder builder)
        {
            var services = builder.Services;
            var serviceProvider = services.BuildServiceProvider();
            var provider = serviceProvider.GetRequiredService<ILoggerProvider>();
            return provider;
        }

        internal sealed class LoggingBuilderStub : ILoggingBuilder
        {
            public IServiceCollection Services { get; set; } = new ServiceCollection();
        }

        private static void AssertSingleMessage(MemoryTarget memoryTarget, string expectedMessage)
        {
            Assert.Single(memoryTarget.Logs);
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
