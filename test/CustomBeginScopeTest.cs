using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class CustomBeginScopeTest : NLogTestBase
    {
        [Fact]
        public void TestNonSerializableSayHello()
        {
            var runner = GetRunner<CustomBeginScopeTestRunner>();
            var target = new NLog.Targets.MemoryTarget() { Layout = "${message} ${ndlc}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayHello().Wait();
            Assert.Single(target.Logs);
            Assert.Contains("Hello World", target.Logs[0]);
        }

        [Fact]
        public void TestNonSerializableSayNothing()
        {
            var runner = GetRunner<CustomBeginScopeTestRunner>(new NLogProviderOptions() { IncludeScopes = false });
            var target = new NLog.Targets.MemoryTarget() { Layout = "${message} ${ndlc}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayHello().Wait();
            Assert.Single(target.Logs);
            Assert.Contains("Hello ", target.Logs[0]);
        }

        public class CustomBeginScopeTestRunner
        {
            private readonly ILogger<CustomBeginScopeTestRunner> _logger;

            public CustomBeginScopeTestRunner(ILogger<CustomBeginScopeTestRunner> logger)
            {
                _logger = logger;
            }

            public async Task SayHello()
            {
                using (_logger.BeginScope(new ActionLogScope("World")))
                {
                    await Task.Yield();
                    _logger.LogInformation("Hello");
                }
            }
        }

        private class ActionLogScope : IReadOnlyList<KeyValuePair<string, object>>
        {
            private readonly string _action;

            public ActionLogScope(string action)
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }

                _action = action;
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return new KeyValuePair<string, object>("ActionId", _action);
                    }
                    throw new IndexOutOfRangeException(nameof(index));
                }
            }

            public int Count => 1;

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (var i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            public override string ToString()
            {
                // We don't include the _action.Id here because it's just an opaque guid, and if
                // you have text logging, you can already use the requestId for correlation.
                return _action;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
