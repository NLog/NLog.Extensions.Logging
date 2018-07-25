using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class CustomLoggerPropertyTest : NLogTestBase
    {
        [Fact]
        public void TestExtraMessageTemplatePropertySayHello()
        {
            ConfigureServiceProvider<CustomLoggerPropertyTestRunner>((s) => s.AddSingleton(typeof(ILogger<>), typeof(SameAssemblyLogger<>)));
            var runner = GetRunner<CustomLoggerPropertyTestRunner>();

            var target = new NLog.Targets.MemoryTarget() { Layout = "${message}|${all-event-properties}"};
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayHello();
            Assert.Single(target.Logs);
            Assert.Equal(@"Hello ""World""|userid=World, ActivityId=42", target.Logs[0]);
        }

        [Fact]
        public void TestExtraMessageTemplatePropertySayHigh5()
        {
            ConfigureServiceProvider<CustomLoggerPropertyTestRunner>((s) => s.AddSingleton(typeof(ILogger<>), typeof(SameAssemblyLogger<>)));
            var runner = GetRunner<CustomLoggerPropertyTestRunner>();

            var target = new NLog.Targets.MemoryTarget() { Layout = "${message}|${all-event-properties}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayHigh5();
            Assert.Single(target.Logs);
            Assert.Equal(@"Hi 5|ActivityId=42", target.Logs[0]);
        }

        [Fact]
        public void TestExtraMessagePropertySayHi()
        {
            ConfigureServiceProvider<CustomLoggerPropertyTestRunner>((s) => s.AddSingleton(typeof(ILogger<>), typeof(SameAssemblyLogger<>)), new NLogProviderOptions() { CaptureMessageTemplates = false });
            var runner = GetRunner<CustomLoggerPropertyTestRunner>();

            var target = new NLog.Targets.MemoryTarget() { Layout = "${message}|${all-event-properties}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayHigh5();
            Assert.Single(target.Logs);
            Assert.Equal(@"Hi 5|ActivityId=42, 0=5", target.Logs[0]);
        }

        public class SameAssemblyLogger<T> : ILogger<T>
        {
            private readonly Microsoft.Extensions.Logging.ILogger _logger;

            public SameAssemblyLogger(ILoggerFactory loggerFactory)
            {
                _logger = loggerFactory.CreateLogger<T>();
            }

            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                _logger.Log(logLevel, eventId, new MyLogEvent<TState>(state, formatter).AddProp("ActivityId", 42), exception, MyLogEvent<TState>.Formatter);
            }

            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
            {
                return _logger.IsEnabled(logLevel);
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return _logger.BeginScope(state);
            }
        }

        public class CustomLoggerPropertyTestRunner
        {
            private readonly ILogger<CustomLoggerPropertyTestRunner> _logger;

            public CustomLoggerPropertyTestRunner(ILogger<CustomLoggerPropertyTestRunner> logger)
            {
                _logger = logger;
            }

            public void SayHello()
            {
                _logger.LogInformation("Hello {$userid}", "World");
            }

            public void SayHigh5()
            {
                _logger.LogInformation("Hi {0}", 5);
            }
        }

        class MyLogEvent<TState> : IReadOnlyList<KeyValuePair<string, object>>
        {
            List<KeyValuePair<string, object>> _properties = new List<KeyValuePair<string, object>>();
            Func<TState, Exception, string> _originalFormattter;
            TState _originalState;

            public MyLogEvent(TState state, Func<TState, Exception, string> formatter)
            {
                _originalState = state;
                _originalFormattter = formatter;
                if (_originalState is IReadOnlyList<KeyValuePair<string,object>> customProperties)
                {
                    _properties.AddRange(customProperties);
                }
            }

            public MyLogEvent<TState> AddProp<T>(string name, T value)
            {
                _properties.Insert(0, new KeyValuePair<string, object>(name, value));
                return this;
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                return ((IReadOnlyList<KeyValuePair<string, object>>)_properties).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IReadOnlyList<KeyValuePair<string, object>>)_properties).GetEnumerator();
            }

            public static Func<MyLogEvent<TState>, Exception, string> Formatter { get; } = (l, e) => l._originalFormattter(l._originalState, e);

            public int Count => ((IReadOnlyList<KeyValuePair<string, object>>)_properties).Count;

            public KeyValuePair<string, object> this[int index] => ((IReadOnlyList<KeyValuePair<string, object>>)_properties)[index];
        }
    }
}
