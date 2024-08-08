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
        public void TestScopeValueTuple()
        {
            var runner = GetRunner();
            using (runner.Logger.BeginScope(("scope1", "Hello")))
            {
                runner.Logger.LogDebug("message with id and {0} parameters", 1);
            }

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters|scope1=Hello", runner.LastTargetMessage);
        }

        [Fact]
        public void TestScopeValueTupleInt()
        {
            var runner = GetRunner();
            using (runner.Logger.BeginScope(("scope1", 42)))
            {
                runner.Logger.LogDebug("message with id and {0} parameters", 1);
            }

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters|scope1=42", runner.LastTargetMessage);
        }

        [Fact]
        public void TestScopeValueTupleList()
        {
            var runner = GetRunner();
            using (runner.Logger.BeginScope(new[] { ("scope1", "Hello") }))
            {
                runner.Logger.LogDebug("message with id and {0} parameters", 1);
            }

            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|message with id and 1 parameters|scope1=Hello", runner.LastTargetMessage);
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
        public void TestInvalidMixFormatString()
        {
            var runner = GetRunner<Runner>();
            runner.Logger.LogDebug("{0}{Mix}", "Mix", "Test");
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|MixTest|0=Mix, Mix=Test", runner.LastTargetMessage);
        }

        [Fact]
        public void TestInvalidMixFormatString2()
        {
            var runner = GetRunner<Runner>(new NLogProviderOptions { CaptureMessageTemplates = false });
            runner.Logger.LogDebug("{0}{Mix}", "Mix", "Test");
            Assert.Equal("NLog.Extensions.Logging.Tests.LoggerTests.Runner|DEBUG|MixTest|0=Mix, Mix=Test", runner.LastTargetMessage);
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

        [Fact]
        public void TestCaptureMessageParameters()
        {
            LogEventInfo logEvent = null;
            var debugTarget = new MethodCallTarget("output", (l, args) => logEvent = l);
            var runner = GetRunner<Runner>(new NLogProviderOptions() { CaptureMessageParameters = true }, debugTarget);
            var messageTemplte = "{Action} {Target}";
            runner.Logger.LogDebug("{Action} {Target}", "Hello", "World");

            Assert.NotNull(logEvent);
            Assert.Equal("Hello World", logEvent.FormattedMessage);
            Assert.Equal(messageTemplte, logEvent.Message);
            Assert.NotNull(logEvent.Parameters);
            Assert.Equal(2, logEvent.Parameters.Length);
            Assert.Equal("Hello", logEvent.Parameters[0]);
            Assert.Equal("World", logEvent.Parameters[1]);

            var messageParameters = logEvent.MessageTemplateParameters;
            Assert.Equal(2, messageParameters.Count);
            Assert.Equal("Hello", messageParameters[0].Value);
            Assert.Equal("World", messageParameters[1].Value);
        }

        [Fact]
        public void TestPositionalCaptureMessageParameters()
        {
            LogEventInfo logEvent = null;
            var debugTarget = new MethodCallTarget("output", (l, args) => logEvent = l);
            var runner = GetRunner<Runner>(new NLogProviderOptions() { CaptureMessageParameters = true }, debugTarget);
            string formatString = "{0,8:S} {1,8:S}";
            runner.Logger.LogDebug(formatString, "Hello", "World");

            Assert.NotNull(logEvent);
            Assert.Equal("   Hello    World", logEvent.FormattedMessage);
            Assert.Equal(formatString, logEvent.Message);
            Assert.NotNull(logEvent.Parameters);
            Assert.Equal(2, logEvent.Parameters.Length);
            Assert.Equal("Hello", logEvent.Parameters[0]);
            Assert.Equal("World", logEvent.Parameters[1]);

            var messageParameters = logEvent.MessageTemplateParameters;
            Assert.True(messageParameters.IsPositional);
            Assert.Equal(2, messageParameters.Count);
            Assert.Equal("Hello", messageParameters[0].Value);
            Assert.Equal("S", messageParameters[0].Format);
            Assert.Equal("World", messageParameters[1].Value);
            Assert.Equal("S", messageParameters[1].Format);
        }

        [Fact]
        public void TestParseMessageTemplates()
        {
            LogEventInfo logEvent = null;
            var debugTarget = new MethodCallTarget("output", (l, args) => logEvent = l);
            var runner = GetRunner<Runner>(new NLogProviderOptions() { ParseMessageTemplates = true }, debugTarget);
            var messageTemplate = "message with {ParameterCount} parameters";
            runner.Logger.LogDebug(messageTemplate, "1");

            Assert.NotNull(logEvent);
            Assert.Equal(messageTemplate, logEvent.Message);
            Assert.NotNull(logEvent.Parameters);
            Assert.Single(logEvent.Parameters);
            Assert.Equal("1", logEvent.Parameters[0]);

            var messageParameters = logEvent.MessageTemplateParameters;
            Assert.False(messageParameters.IsPositional);
            Assert.Equal(1, messageParameters.Count);
            Assert.Equal("ParameterCount", messageParameters[0].Name);
            Assert.Equal("1", messageParameters[0].Value);
        }

        [Fact]
        public void TestPositionalParseMessageTemplates()
        {
            LogEventInfo logEvent = null;
            var debugTarget = new MethodCallTarget("output", (l, args) => logEvent = l);
            var runner = GetRunner<Runner>(new NLogProviderOptions() { ParseMessageTemplates = true }, debugTarget);
            string formatString = "{0,8:S} {1,8:S}";
            runner.Logger.LogDebug(formatString, "Hello", "World");

            Assert.NotNull(logEvent);
            Assert.Equal("   Hello    World", logEvent.FormattedMessage);
            Assert.Equal(formatString, logEvent.Message);
            Assert.NotNull(logEvent.Parameters);
            Assert.Equal(2, logEvent.Parameters.Length);
            Assert.Equal("Hello", logEvent.Parameters[0]);
            Assert.Equal("World", logEvent.Parameters[1]);

            var messageParameters = logEvent.MessageTemplateParameters;
            Assert.True(messageParameters.IsPositional);
            Assert.Equal(2, messageParameters.Count);
            Assert.Equal("Hello", messageParameters[0].Value);
            Assert.Equal("S", messageParameters[0].Format);
            Assert.Equal("World", messageParameters[1].Value);
            Assert.Equal("S", messageParameters[1].Format);
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
