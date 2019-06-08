using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class MicrosoftILoggerTargetTests
    {
        [Fact]
        public void SimpleILoggerMessageTest()
        {
            // Arrange
            var (logger, mock) = CreateLoggerMock();

            // Act
            logger.Info("Hello World");

            // Assert
            Assert.Equal("Hello World", mock.LastLogMessage);
            Assert.Single(mock.LastLogProperties);
            Assert.Equal("Hello World", mock.LastLogProperties[0].Value);
        }

        [Fact]
        public void FilterILoggerMessageTest()
        {
            // Arrange
            var (logger, mock) = CreateLoggerMock();

            // Act
            logger.Debug("Hello World");

            // Assert
            Assert.Null(mock.LastLogMessage);
        }

        [Fact]
        public void StructuredILoggerMessageTest()
        {
            // Arrange
            var (logger, mock) = CreateLoggerMock();

            // Act
            logger.Info("Hello {Planet}", "Earth");

            // Assert
            Assert.Equal("Hello \"Earth\"", mock.LastLogMessage);
            Assert.Equal(2, mock.LastLogProperties.Count);
            Assert.Equal("Planet", mock.LastLogProperties[0].Key);
            Assert.Equal("Earth", mock.LastLogProperties[0].Value);
            Assert.Equal("Hello {Planet}", mock.LastLogProperties[1].Value);
        }

        [Fact]
        public void ContextPropertiesILoggerTest()
        {
            // Arrange
            var (logger, mock) = CreateLoggerMock(out var target);
            target.ContextProperties.Add(new Targets.TargetPropertyWithContext
                { Name = "ThreadId", Layout = "${threadid}" }
            );

            // Act
            logger.Info("Hello {Planet}", "Earth");

            // Assert
            Assert.Equal("Hello \"Earth\"", mock.LastLogMessage);
            Assert.Equal(3, mock.LastLogProperties.Count);
            Assert.Equal("Planet", mock.LastLogProperties[0].Key);
            Assert.Equal("Earth", mock.LastLogProperties[0].Value);
            Assert.Equal("ThreadId", mock.LastLogProperties[1].Key);
            Assert.Equal(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(), mock.LastLogProperties[1].Value);
            Assert.Equal("Hello {Planet}", mock.LastLogProperties[2].Value);
        }

        private static (Logger, LoggerMock) CreateLoggerMock()
        {
            return CreateLoggerMock(out _);
        }

        private static (Logger, LoggerMock) CreateLoggerMock(out MicrosoftILoggerTarget target)
        {
            var logFactory = new NLog.LogFactory();
            var logConfig = new NLog.Config.LoggingConfiguration();
            var loggerMock = new LoggerMock();
            target = new MicrosoftILoggerTarget(loggerMock) { Layout = "${message}" };
            logConfig.AddRuleForAllLevels(target);
            logFactory.Configuration = logConfig;
            var logger = logFactory.GetCurrentClassLogger();
            return (logger, loggerMock);
        }

        class LoggerMock : Microsoft.Extensions.Logging.ILogger
        {
            public Microsoft.Extensions.Logging.LogLevel LastLogLevel;
            public string LastLogMessage;
            public Exception LastLogException;
            public IList<KeyValuePair<string, object>> LastLogProperties;
            public Microsoft.Extensions.Logging.EventId LastLogEventId;

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case Microsoft.Extensions.Logging.LogLevel.Trace:
                    case Microsoft.Extensions.Logging.LogLevel.Debug:
                        return false;
                }
                return true;
            }

            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogLevel = logLevel;
                LastLogEventId = eventId;
                LastLogMessage = formatter(state, exception);
                LastLogException = exception;
                var propertiesList = (state as IReadOnlyList<KeyValuePair<string, object>>);
                LastLogProperties = propertiesList?.ToList();
                for (int i = 0; i < propertiesList?.Count; ++i)
                {
                    var property = propertiesList[i];
                    if (property.Key != LastLogProperties[i].Key)
                        throw new ArgumentException($"Property key mismatch {LastLogProperties[i].Key} <-> {property.Key}");
                    if (property.Value != LastLogProperties[i].Value)
                        throw new ArgumentException($"Property Value mismatch {LastLogProperties[i].Value} <-> {property.Value}");
                }
            }
        }
    }
}
