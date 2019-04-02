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
            var logFactory = new NLog.LogFactory();
            var logConfig = new NLog.Config.LoggingConfiguration();
            var ilogger = new TestLogger();
            logConfig.AddRuleForAllLevels(new MicrosoftILoggerTarget(ilogger) { Layout = "${message}" });
            logFactory.Configuration = logConfig;
            var logger = logFactory.GetCurrentClassLogger();
            logger.Info("Hello World");
            Assert.Equal("Hello World", ilogger.LastLogMessage);
            Assert.Single(ilogger.LastLogProperties);
            Assert.Equal("Hello World", ilogger.LastLogProperties[0].Value);
        }

        [Fact]
        public void FilterILoggerMessageTest()
        {
            var logFactory = new NLog.LogFactory();
            var logConfig = new NLog.Config.LoggingConfiguration();
            var ilogger = new TestLogger();
            logConfig.AddRuleForAllLevels(new MicrosoftILoggerTarget(ilogger) { Layout = "${message}" });
            logFactory.Configuration = logConfig;
            var logger = logFactory.GetCurrentClassLogger();
            logger.Debug("Hello World");
            Assert.Null(ilogger.LastLogMessage);
        }

        [Fact]
        public void StructuredILoggerMessageTest()
        {
            var logFactory = new NLog.LogFactory();
            var logConfig = new NLog.Config.LoggingConfiguration();
            var ilogger = new TestLogger();
            logConfig.AddRuleForAllLevels(new MicrosoftILoggerTarget(ilogger) { Layout = "${message}" });
            logFactory.Configuration = logConfig;
            var logger = logFactory.GetCurrentClassLogger();
            logger.Info("Hello {Planet}", "Earth");
            Assert.Equal("Hello \"Earth\"", ilogger.LastLogMessage);
            Assert.Equal(2, ilogger.LastLogProperties.Count);
            Assert.Equal("Planet", ilogger.LastLogProperties[0].Key);
            Assert.Equal("Earth", ilogger.LastLogProperties[0].Value);
            Assert.Equal("Hello {Planet}", ilogger.LastLogProperties[1].Value);
        }

        [Fact]
        public void ContextPropertiesILoggerTest()
        {
            var logFactory = new NLog.LogFactory();
            var logConfig = new NLog.Config.LoggingConfiguration();
            var ilogger = new TestLogger();
            var target = new MicrosoftILoggerTarget(ilogger) { Layout = "${message}" };
            target.ContextProperties.Add(new Targets.TargetPropertyWithContext() { Name = "ThreadId", Layout = "${threadid}" });
            logConfig.AddRuleForAllLevels(target);
            logFactory.Configuration = logConfig;
            var logger = logFactory.GetCurrentClassLogger();
            logger.Info("Hello {Planet}", "Earth");
            Assert.Equal("Hello \"Earth\"", ilogger.LastLogMessage);
            Assert.Equal(3, ilogger.LastLogProperties.Count);
            Assert.Equal("Planet", ilogger.LastLogProperties[0].Key);
            Assert.Equal("Earth", ilogger.LastLogProperties[0].Value);
            Assert.Equal("ThreadId", ilogger.LastLogProperties[1].Key);
            Assert.Equal(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(), ilogger.LastLogProperties[1].Value);
            Assert.Equal("Hello {Planet}", ilogger.LastLogProperties[2].Value);
        }

        class TestLogger : Microsoft.Extensions.Logging.ILogger
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
