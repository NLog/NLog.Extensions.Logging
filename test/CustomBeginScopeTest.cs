using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class CustomBeginScopeTest : NLogTestBase
    {
        [Fact]
        public void TestCallSiteSayHello()
        {
            var runner = GetRunner<CustomBeginScopeTestRunner>();
            var target = new NLog.Targets.MemoryTarget() { Layout = "${message} ${ndlc}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayHello();
            Assert.Single(target.Logs);
            Assert.Contains("Hello World", target.Logs[0]);
        }


        public class CustomBeginScopeTestRunner
        {
            private readonly ILogger<CustomBeginScopeTestRunner> _logger;

            public CustomBeginScopeTestRunner(ILogger<CustomBeginScopeTestRunner> logger)
            {
                _logger = logger;
            }

            public void SayHello()
            {
                using (_logger.BeginScope(new ActionLogScope("World")))
                {
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
