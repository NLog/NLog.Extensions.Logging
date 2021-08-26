using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NLog.Targets;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class LoggerTests : NLogTestBase
    {
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
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|init runner |", target.Logs.FirstOrDefault());
        }

        [Fact]
        public void TestEventId()
        {
            GetRunner().LogDebugWithId();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id |20", target.Logs.FirstOrDefault());
        }

        [Fact]
        public void TestParameters()
        {
            GetRunner().LogDebugWithParameters();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters |", target.Logs.FirstOrDefault());
        }

        [Fact]
        public void TestStructuredLogging()
        {
            GetRunner().LogDebugWithStructuredParameters();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters |1", target.Logs.FirstOrDefault());
        }

        [Fact]
        public void TestStructuredLoggingFormatter()
        {
            GetRunner().LogDebugWithStructuredParameterFormater();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and {\"TestValue\":\"This is the test value\"} parameters |", target.Logs.FirstOrDefault());
        }

        [Fact]
        public void TestSimulateStructuredLogging()
        {
            GetRunner().LogDebugWithSimulatedStructuredParameters();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 property |1", target.Logs.FirstOrDefault());
        }

        [Fact]
        public void TestMessageProperties()
        {
            GetRunner().LogDebugWithMessageProperties();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 property |1", target.Logs.FirstOrDefault());
        }

        [Fact]
        public void TestMessagePropertiesList()
        {
            GetRunner().LogDebugWithMessagePropertiesList();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 property |1", target.Logs.FirstOrDefault());
        }

        [Fact]
        public void TestScopeProperty()
        {
            GetRunner().LogWithScopeParameter();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters |Hello", target.Logs.LastOrDefault());
        }

        [Fact]
        public void TestScopePropertyList()
        {
            GetRunner().LogWithScopeParameterList();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters |Hello", target.Logs.LastOrDefault());
        }

        [Fact]
        public void TestScopePropertyDictionary()
        {
            GetRunner().LogWithScopeParameterDictionary();

            var target = GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters |Hello", target.Logs.LastOrDefault());
        }

        [Fact]
        public void TestInvalidFormatString()
        {
            var runner = GetRunner<Runner>();
            var ex = Assert.Throws<AggregateException>(() => runner.Log(Microsoft.Extensions.Logging.LogLevel.Information, 0, null, "{0}{1}", "Test"));
            Assert.IsType<FormatException>(ex.InnerException);
        }

        [Fact]
        public void TestInvalidFormatString2()
        {
            var runner = GetRunner<Runner>(new NLogProviderOptions { CaptureMessageTemplates = false });
            var ex = Assert.Throws<AggregateException>(() => runner.Log(Microsoft.Extensions.Logging.LogLevel.Information, 0, null, "{0}{1}", "Test"));
            Assert.IsType<FormatException>(ex.InnerException);
        }

        [Theory]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Critical, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|FATAL|message System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Debug, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Error, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|ERROR|message System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Information, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|INFO|message System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Trace, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|TRACE|message System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Warning, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|WARN|message System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        public void TestExceptionWithMessage(Microsoft.Extensions.Logging.LogLevel logLevel, string expectedLogMessage)
        {
            GetRunner().Log(logLevel, 20, new Exception(), "message");

            var target = GetTarget();
            Assert.Equal(expectedLogMessage, target.Logs.FirstOrDefault());
        }

        [Theory]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Critical, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|FATAL| System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Debug, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG| System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Error, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|ERROR| System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Information, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|INFO| System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Trace, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|TRACE| System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Warning, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|WARN| System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        public void TestExceptionWithEmptyMessage(Microsoft.Extensions.Logging.LogLevel logLevel, string expectedLogMessage)
        {
            GetRunner().Log(logLevel, 20, new Exception(), string.Empty);

            var target = GetTarget();
            Assert.Equal(expectedLogMessage, target.Logs.FirstOrDefault());
        }

        [Theory]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Critical, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|FATAL|[null] System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Debug, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|[null] System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Error, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|ERROR|[null] System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Information, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|INFO|[null] System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Trace, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|TRACE|[null] System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Warning, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|WARN|[null] System.Exception: Exception of type 'System.Exception' was thrown.|20")]
        public void TestExceptionWithNullMessage(Microsoft.Extensions.Logging.LogLevel logLevel, string expectedLogMessage)
        {
            GetRunner().Log(logLevel, 20, new Exception(), null);

            var target = GetTarget();
            Assert.Equal(expectedLogMessage, target.Logs.FirstOrDefault());
        }

        [Theory]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Critical, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|FATAL|message |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Debug, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Error, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|ERROR|message |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Information, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|INFO|message |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Trace, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|TRACE|message |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Warning, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|WARN|message |20")]
        public void TestMessageWithNullException(Microsoft.Extensions.Logging.LogLevel logLevel, string expectedLogMessage)
        {
            GetRunner<Runner>().Log(logLevel, 20, null, "message");

            var target = GetTarget();
            Assert.Equal(expectedLogMessage, target.Logs.FirstOrDefault());
        }

        [Theory]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Critical, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|FATAL|[null] |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Debug, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|[null] |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Error, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|ERROR|[null] |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Information, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|INFO|[null] |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Trace, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|TRACE|[null] |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Warning, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|WARN|[null] |20")]
        public void TestWithNullMessageAndNullException(Microsoft.Extensions.Logging.LogLevel logLevel, string expectedLogMessage)
        {
            GetRunner().Log(logLevel, 20, null, null);

            var target = GetTarget();
            Assert.Equal(expectedLogMessage, target.Logs.FirstOrDefault());
        }

        [Theory]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Critical, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|FATAL| |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Debug, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG| |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Error, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|ERROR| |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Information, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|INFO| |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Trace, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|TRACE| |20")]
        [InlineData(Microsoft.Extensions.Logging.LogLevel.Warning, "NLog.Extensions.Logging.Tests.LoggerTests.Runner|WARN| |20")]
        public void TestWithEmptyMessageAndNullException(Microsoft.Extensions.Logging.LogLevel logLevel, string expectedLogMessage)
        {
            GetRunner().Log(logLevel, 20, null, string.Empty);

            var target = GetTarget();
            Assert.Equal(expectedLogMessage, target.Logs.FirstOrDefault());
        }

        private MemoryTarget GetTarget()
        {
            var target = LoggerProvider?.LogFactory?.Configuration?.FindTargetByName<MemoryTarget>("target1");
            return target;
        }

        private Runner GetRunner()
        {
            ConfigureTransientService<Runner>((s) => LoggerProvider.LogFactory.LoadConfiguration("nlog.config"));
            return base.GetRunner<Runner>();
        }

        public class Runner
        {
            private readonly ILogger<Runner> _logger;

            public Runner(ILogger<Runner> logger)
            {
                _logger = logger;
            }

            public void LogDebugWithId()
            {
                _logger.LogDebug(20, "message with id");
            }

            public void Log(Microsoft.Extensions.Logging.LogLevel logLevel, int eventId, Exception exception, string message, params object[] args)
            {
                switch (logLevel)
                {
                    case Microsoft.Extensions.Logging.LogLevel.Trace:
                        _logger.LogTrace(eventId, exception, message, args);
                        break;
                    case Microsoft.Extensions.Logging.LogLevel.Debug:
                        _logger.LogDebug(eventId, exception, message, args);
                        break;
                    case Microsoft.Extensions.Logging.LogLevel.Information:
                        _logger.LogInformation(eventId, exception, message, args);
                        break;
                    case Microsoft.Extensions.Logging.LogLevel.Warning:
                        _logger.LogWarning(eventId, exception, message, args);
                        break;
                    case Microsoft.Extensions.Logging.LogLevel.Error:
                        _logger.LogError(eventId, exception, message, args);
                        break;
                    case Microsoft.Extensions.Logging.LogLevel.Critical:
                        _logger.LogCritical(eventId, exception, message, args);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
                }
            }

            public void LogDebugWithParameters()
            {
                _logger.LogDebug("message with id and {0} parameters", "1");
            }

            public void LogDebugWithStructuredParameters()
            {
                _logger.LogDebug("message with id and {ParameterCount} parameters", "1");
            }

            public void LogDebugWithStructuredParameterFormater()
            {
                _logger.LogDebug("message with id and {@ObjectParameter} parameters", new TestObject());
            }

            public void LogDebugWithSimulatedStructuredParameters()
            {
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, default(EventId), new List<KeyValuePair<string, object>>(new[] { new KeyValuePair<string, object>("{OriginalFormat}", "message with id and {ParameterCount} property"), new KeyValuePair<string, object>("ParameterCount", 1) }), null, (s, ex) => "message with id and 1 property");
            }

            public void LogDebugWithMessageProperties()
            {
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, default(EventId), new Dictionary<string, object> { { "ParameterCount", "1" } }, null, (s, ex) => "message with id and 1 property");
            }

            public void LogDebugWithMessagePropertiesList()
            {
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, default(EventId), new List<KeyValuePair<string, object>>(new[] { new KeyValuePair<string, object>("ParameterCount", "1") }), null, (s, ex) => "message with id and 1 property");
            }

            public void LogWithScope()
            {
                using (_logger.BeginScope("scope1"))
                {
                    _logger.LogDebug(20, "message with id and {0} parameters", 1);
                }
            }

            public void LogWithScopeParameter()
            {
                using (_logger.BeginScope(new KeyValuePair<string, string>("scope1", "Hello")))
                {
                    _logger.LogDebug("message with id and {0} parameters", 1);
                }
            }

            public void LogWithScopeParameterList()
            {
                using (_logger.BeginScope(new[] { new KeyValuePair<string, object>("scope1", "Hello") }))
                {
                    _logger.LogDebug("message with id and {0} parameters", 1);
                }
            }

            public void LogWithScopeParameterDictionary()
            {
                using (_logger.BeginScope(new Dictionary<string, string> { ["scope1"] = "Hello" }))
                {
                    _logger.LogDebug("message with id and {0} parameters", 1);
                }
            }

            public void Init()
            {
                _logger.LogDebug("init runner");
            }
        }

        public class TestObject
        {
            public string TestValue { get; set; } = "This is the test value";
        }
    }
}
