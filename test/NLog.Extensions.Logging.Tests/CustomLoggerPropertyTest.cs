using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Targets;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class CustomLoggerPropertyTest : NLogTestBase
    {
        [Fact]
        public void TestExtraMessageTemplatePropertySayHello()
        {
            var runner = GetRunner();

            runner.SayHello();

            Assert.Single(runner.GetTarget().Logs);
            Assert.Equal(@"Hello ""World""|userid=World, ActivityId=42", runner.GetTarget().Logs[0]);
        }

        [Fact]
        public void TestExtraMessageTemplatePropertySayHigh5()
        {
            var runner = GetRunner();

            runner.SayHigh5();

            Assert.Single(runner.GetTarget().Logs);
            Assert.Equal(@"Hi 5|ActivityId=42", runner.GetTarget().Logs[0]);
        }

        [Fact]
        public void TestExtraMessagePropertySayHi()
        {
            var options = new NLogProviderOptions { CaptureMessageTemplates = false };
            var runner = GetRunner(options);

            runner.SayHigh5();

            Assert.Single(runner.GetTarget().Logs);
            Assert.Equal(@"Hi 5|ActivityId=42, 0=5", runner.GetTarget().Logs[0]);
        }

        private CustomLoggerPropertyTestRunner GetRunner(NLogProviderOptions options = null)
        {
            var target = new Targets.MemoryTarget { Layout = "${message}|${all-event-properties}" };
            return SetupServiceProvider(options, target, configureServices: (s) => s.AddTransient<CustomLoggerPropertyTestRunner>().AddSingleton(typeof(ILogger<>), typeof(SameAssemblyLogger<>))).GetRequiredService<CustomLoggerPropertyTestRunner>();
        }

        public sealed class SameAssemblyLogger<T> : ILogger<T>
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

        public sealed class CustomLoggerPropertyTestRunner
        {
            private readonly ILogger<CustomLoggerPropertyTestRunner> _logger;
            private readonly IServiceProvider _serviceProvider;

            public LogFactory LogFactory => (_serviceProvider.GetRequiredService<ILoggerProvider>() as NLogLoggerProvider)?.LogFactory;

            public MemoryTarget GetTarget() => LogFactory?.Configuration?.AllTargets.OfType<MemoryTarget>().FirstOrDefault();

            public CustomLoggerPropertyTestRunner(ILogger<CustomLoggerPropertyTestRunner> logger, IServiceProvider serviceProvider)
            {
                _logger = logger;
                _serviceProvider = serviceProvider;
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

        sealed class MyLogEvent<TState> : IReadOnlyList<KeyValuePair<string, object>>
        {
            private readonly List<KeyValuePair<string, object>> _properties = new List<KeyValuePair<string, object>>();
            private readonly Func<TState, Exception, string> _originalFormattter;
            private readonly TState _originalState;

            public MyLogEvent(TState state, Func<TState, Exception, string> formatter)
            {
                _originalState = state;
                _originalFormattter = formatter;
                if (_originalState is IReadOnlyList<KeyValuePair<string, object>> customProperties)
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
