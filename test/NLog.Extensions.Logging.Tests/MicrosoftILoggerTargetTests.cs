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
        public void SimpleILoggerFactoryMessageTest()
        {
            // Arrange
            var (logger, mock) = CreateLoggerFactoryMock(out _);

            // Act
            logger.Info("Hello World");

            // Assert
            Assert.Single(mock.Loggers);
            Assert.Equal(GetType().ToString(), mock.Loggers.First().Key);
            Assert.Equal("Hello World", mock.Loggers.First().Value.LastLogMessage);
            Assert.Single(mock.Loggers.First().Value.LastLogProperties);
            Assert.Equal("Hello World", mock.Loggers.First().Value.LastLogProperties[0].Value);
        }

        [Fact]
        public void OverrideLoggerNameILoggerFactoryMessageTest()
        {
            // Arrange
            var (logger, mock) = CreateLoggerFactoryMock(out var microsoftTarget);
            microsoftTarget.LoggerName = "${scopeproperty:FunctionName}";

            // Act
            using (NLog.ScopeContext.PushProperty("FunctionName", nameof(OverrideLoggerNameILoggerFactoryMessageTest)))
                logger.Info("Hello World");

            // Assert
            Assert.Single(mock.Loggers);
            Assert.Equal(nameof(OverrideLoggerNameILoggerFactoryMessageTest), mock.Loggers.First().Key);
            Assert.Equal("Hello World", mock.Loggers.First().Value.LastLogMessage);
            Assert.Single(mock.Loggers.First().Value.LastLogProperties);
            Assert.Equal("Hello World", mock.Loggers.First().Value.LastLogProperties[0].Value);
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


        [Fact]
        public void LogWithEventIdTest()
        {
            // Arrange
            var (logger, mock) = CreateLoggerMock(out var target);
            target.EventId = "${event-properties:item1}";

            // Act
            logger.WithProperty("item1", 123).Info("Hello there");

            // Assert
            Assert.Equal("Hello there", mock.LastLogMessage);
            Assert.Equal(123, mock.LastLogEventId.Id);
            Assert.Null(mock.LastLogEventId.Name);

            Assert.Equal(2, mock.LastLogProperties.Count);
            Assert.Equal("item1", mock.LastLogProperties[0].Key);
            Assert.Equal(123, mock.LastLogProperties[0].Value);
            Assert.Equal("{OriginalFormat}", mock.LastLogProperties[1].Key);
            Assert.Equal("Hello there", mock.LastLogProperties[1].Value);
        }

        [Fact]
        public void LogWithEventNameAndIdTest()
        {
            // Arrange
            var (logger, mock) = CreateLoggerMock(out var target);
            target.EventId = "123";
            target.EventName = "event1";

            // Act
            logger.WithProperty("item1", 123).Info("Hello there");

            // Assert
            Assert.Equal("Hello there", mock.LastLogMessage);
            Assert.Equal(123, mock.LastLogEventId.Id);
            Assert.Equal("event1", mock.LastLogEventId.Name);

            Assert.Equal(2, mock.LastLogProperties.Count);
            Assert.Equal("item1", mock.LastLogProperties[0].Key);
            Assert.Equal(123, mock.LastLogProperties[0].Value);
            Assert.Equal("{OriginalFormat}", mock.LastLogProperties[1].Key);
            Assert.Equal("Hello there", mock.LastLogProperties[1].Value);
        }

        [Fact]
        public void IncludeEmptyMdc()
        {
            // Arrange
            var (logger, mock) = CreateLoggerMock(out var target);
            target.IncludeScopeProperties = true;

            // Act
            logger.Info("Hello there");

            // Assert
            Assert.Equal("Hello there", mock.LastLogMessage);
            Assert.Equal(1, mock.LastLogProperties.Count);
            Assert.Equal("{OriginalFormat}", mock.LastLogProperties[0].Key);
            Assert.Equal("Hello there", mock.LastLogProperties[0].Value);
        }

        [Theory]
        [InlineData("trace", "Trace")]
        [InlineData("debug", "Debug")]
        [InlineData("info", "Information")]
        [InlineData("warn", "Warning")]
        [InlineData("error", "Error")]
        [InlineData("fatal", "Critical")]
        public void TestOnLogLevel(string levelText, string expectedILoggerLogLevelText)
        {
            // Arrange
            var logLevel = LogLevel.FromString(levelText);
            var success = Enum.TryParse<Microsoft.Extensions.Logging.LogLevel>(expectedILoggerLogLevelText, out var expectedLogLevel);
            if (!success)
            {
                throw new ArgumentException($"Invalid log level '{expectedILoggerLogLevelText}'", nameof(expectedILoggerLogLevelText));
            }

            var (logger, mock) = CreateLoggerMock();
            mock.EnableAllLevels = true;

            // Act
            logger.Log(logLevel, "message1");

            // Assert
            Assert.Equal("message1", mock.LastLogMessage);
            Assert.Equal(expectedLogLevel, mock.LastLogLevel);
        }

        [Fact]
        public void LogWitException()
        {
            // Arrange
            var (logger, mock) = CreateLoggerMock();
            var ex = new ArgumentException("a is not b", nameof(mock));

            // Act
            logger.Info(ex, "Hello there");

            // Assert
            Assert.Equal("Hello there", mock.LastLogMessage);
            Assert.Equal(ex, mock.LastLogException);
            Assert.Equal(1, mock.LastLogProperties.Count);
            Assert.Equal("{OriginalFormat}", mock.LastLogProperties[0].Key);
            Assert.Equal("Hello there", mock.LastLogProperties[0].Value);
        }

        private static (Logger, LoggerMock) CreateLoggerMock()
        {
            return CreateLoggerMock(out _);
        }

        private static (Logger, LoggerMock) CreateLoggerMock(out MicrosoftILoggerTarget target)
        {
            var logFactory = new LogFactory();
            var logConfig = new Config.LoggingConfiguration();
            var loggerMock = new LoggerMock("NLog");
            target = new MicrosoftILoggerTarget(loggerMock) { Layout = "${message}" };
            logConfig.AddRuleForAllLevels(target);
            logFactory.Configuration = logConfig;
            var logger = logFactory.GetCurrentClassLogger();
            return (logger, loggerMock);
        }

        private static (Logger, LoggerFactoryMock) CreateLoggerFactoryMock(out MicrosoftILoggerTarget target)
        {
            var logFactory = new LogFactory();
            var logConfig = new Config.LoggingConfiguration();
            var loggerFactoryMock = new LoggerFactoryMock();
            target = new MicrosoftILoggerTarget(loggerFactoryMock) { Layout = "${message}" };
            logConfig.AddRuleForAllLevels(target);
            logFactory.Configuration = logConfig;
            var logger = logFactory.GetCurrentClassLogger();
            return (logger, loggerFactoryMock);
        }

        sealed class LoggerFactoryMock : Microsoft.Extensions.Logging.ILoggerFactory
        {
            public readonly Dictionary<string, LoggerMock> Loggers = new Dictionary<string, LoggerMock>();

            public void AddProvider(ILoggerProvider provider)
            {
                // Nothing to do
            }

            public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
            {
                if (!Loggers.TryGetValue(categoryName, out var logger))
                {
                    logger = new LoggerMock(categoryName);
                    Loggers[categoryName] = logger;
                }
                return logger;
            }

            public void Dispose()
            {
                // Nothing to do
            }
        }

        sealed class LoggerMock : Microsoft.Extensions.Logging.ILogger
        {
            public readonly string CategoryName;
            public Microsoft.Extensions.Logging.LogLevel LastLogLevel;
            public string LastLogMessage;
            public Exception LastLogException;
            public IList<KeyValuePair<string, object>> LastLogProperties;
            public EventId LastLogEventId;
            public bool EnableAllLevels;

            public LoggerMock(string categoryName)
            {
                CategoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
            {
                if (EnableAllLevels)
                {
                    return true;
                }

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

            public override string ToString()
            {
                return CategoryName;
            }
        }
    }
}
