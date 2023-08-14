using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Targets;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class LoggerTests : NLogTestBase
    {
        [Fact]
        public void TestInit()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug("init runner");

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|init runner|", runner.LastTargetMessage);
        }

        [Fact]
        public void TestEventId()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug(20, "message with id");

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id|EventId=20", runner.LastTargetMessage);
        }

        [Fact]
        public void TestParameters()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug("message with id and {0} parameters", "1");

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters|", runner.LastTargetMessage);
        }

        [Fact]
        public void TestTwoParameters()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug("message with {0} and {1} parameters", "id", "2");

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 2 parameters|", runner.LastTargetMessage);
        }

        [Fact]
        public void TestTwoReverseParameters()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug("message with {1} and {0} parameters", "2", "id"); // NLog will fix it

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 2 parameters|", runner.LastTargetMessage);
        }

        [Fact]
        public void TestStructuredLogging()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug("message with id and {ParameterCount} parameters", "1");

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters|ParameterCount=1", runner.LastTargetMessage);
        }

        [Fact]
        public void TestStructuredLoggingFormatter()
        {
            var runner = GetRunner();
            var testObject = new TestObject();
            runner.Logger.LogDebug("message with id and {@ObjectParameter} parameters", testObject);

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and {\"TestValue\":\"This is the test value\"} parameters|ObjectParameter=" + testObject.ToString(), runner.LastTargetMessage);
        }

        [Fact]
        public void TestSimulateStructuredLogging()
        {
            var runner = GetRunner();
            runner.Logger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, default(EventId), new List<KeyValuePair<string, object>>(new[] { new KeyValuePair<string, object>("{OriginalFormat}", "message with id and {ParameterCount} property"), new KeyValuePair<string, object>("ParameterCount", 1) }), null, (s, ex) => "message with id and 1 property");

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 property|ParameterCount=1", runner.LastTargetMessage);
        }

        [Fact]
        public void TestMessageProperties()
        {
            var runner = GetRunner();
            runner.Logger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, default(EventId), new Dictionary<string, object> { { "ParameterCount", "1" } }, null, (s, ex) => "message with id and 1 property");

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 property|ParameterCount=1", runner.LastTargetMessage);
        }

        [Fact]
        public void TestMessagePropertiesList()
        {
            var runner = GetRunner();
            runner.Logger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, default(EventId), new List<KeyValuePair<string, object>>(new[] { new KeyValuePair<string, object>("ParameterCount", "1") }), null, (s, ex) => "message with id and 1 property");

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 property|ParameterCount=1", runner.LastTargetMessage);
        }

        [Fact]
        public void TestScopeParameter()
        {
            var runner = GetRunner();
            using (runner.Logger.BeginScope("scope1"))
            {
                runner.Logger.LogDebug(20, "message with id and {0} parameters", 1);
            }

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters|EventId=20", runner.LastTargetMessage);
        }

        [Fact]
        public void TestScopeProperty()
        {
            var runner = GetRunner();
            using (runner.Logger.BeginScope(new KeyValuePair<string, string>("scope1", "Hello")))
            {
                runner.Logger.LogDebug("message with id and {0} parameters", 1);
            }

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters|scope1=Hello", runner.LastTargetMessage);
        }

        [Fact]
        public void TestScopePropertyInt()
        {
            var runner = GetRunner();
            using (runner.Logger.BeginScope(new KeyValuePair<string, int>("scope1", 42)))
            {
                runner.Logger.LogDebug("message with id and {0} parameters", 1);
            }

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters|scope1=42", runner.LastTargetMessage);
        }

        [Fact]
        public void TestScopePropertyList()
        {
            var runner = GetRunner();
            using (runner.Logger.BeginScope(new[] { new KeyValuePair<string, object>("scope1", "Hello") }))
            {
                runner.Logger.LogDebug("message with id and {0} parameters", 1);
            }

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters|scope1=Hello", runner.LastTargetMessage);
        }

        [Fact]
        public void TestScopeIntPropertyList()
        {
            var runner = GetRunner();
            using (runner.Logger.BeginScope(new[] { new KeyValuePair<string, int>("scope1", 42) }))
            {
                runner.Logger.LogDebug("message with id and {0} parameters", 1);
            }

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters|scope1=42", runner.LastTargetMessage);
        }

        [Fact]
        public void TestScopePropertyDictionary()
        {
            var runner = GetRunner();
            using (runner.Logger.BeginScope(new Dictionary<string, string> { ["scope1"] = "Hello" }))
            {
                runner.Logger.LogDebug("message with id and {0} parameters", 1);
            }

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters|scope1=Hello", runner.LastTargetMessage);
        }

        [Fact]
        public void TestInvalidFormatString()
        {
            var runner = GetRunner<Runner>();
            var ex = Assert.Throws<AggregateException>(() => runner.Logger.LogDebug("{0}{1}", "Test"));
            Assert.IsType<FormatException>(ex.InnerException);
        }

        [Fact]
        public void TestInvalidFormatString2()
        {
            var runner = GetRunner<Runner>(new NLogProviderOptions { CaptureMessageTemplates = false });
            var ex = Assert.Throws<AggregateException>(() => runner.Logger.LogDebug("{0}{1}", "Test"));
            Assert.IsType<FormatException>(ex.InnerException);
        }

        [Fact]
        public void TestExceptionWithMessage()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug(20, new Exception(), "message");
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message|EventId=20|System.Exception: Exception of type 'System.Exception' was thrown.", runner.LastTargetMessage);
        }

        [Fact]
        public void TestExceptionWithEmptyMessage()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug(20, new Exception(), string.Empty);
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG||EventId=20|System.Exception: Exception of type 'System.Exception' was thrown.", runner.LastTargetMessage);
        }

        [Fact]
        public void TestExceptionWithNullMessage()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug(20, new Exception(), (string)null);
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|[null]|EventId=20|System.Exception: Exception of type 'System.Exception' was thrown.", runner.LastTargetMessage);
        }

        [Fact]
        public void TestMessageWithNullException()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug(20, (Exception)null, "message");
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message|EventId=20", runner.LastTargetMessage);
        }

        [Fact]
        public void TestWithNullMessageAndNullException()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug(20, (Exception)null, (string)null);

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|[null]|EventId=20", runner.LastTargetMessage);
        }

        [Fact]
        public void TestWithEmptyMessageAndNullException()
        {
            var runner = GetRunner();
            runner.Logger.LogDebug(20, (Exception)null, string.Empty);

            var target = runner.GetTarget();
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG||EventId=20", runner.LastTargetMessage);
        }

        private Runner GetRunner()
        {
            return base.GetRunner<Runner>();
        }

        public sealed class Runner
        {
            private readonly ILogger<Runner> _logger;
            private readonly IServiceProvider _serviceProvider;

            public Microsoft.Extensions.Logging.ILogger Logger => _logger;

            public LogFactory LogFactory => (_serviceProvider.GetRequiredService<ILoggerProvider>() as NLogLoggerProvider)?.LogFactory;

            public MemoryTarget GetTarget() => LogFactory?.Configuration?.AllTargets.OfType<MemoryTarget>().FirstOrDefault();

            public string LastTargetMessage => GetTarget()?.Logs?.LastOrDefault();

            public Runner(ILogger<Runner> logger, IServiceProvider serviceProvider)
            {
                _logger = logger;
                _serviceProvider = serviceProvider;
            }
        }

        public class TestObject
        {
            public string TestValue { get; set; } = "This is the test value";
        }
    }
}
