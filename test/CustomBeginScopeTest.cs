using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            var target = new NLog.Targets.MemoryTarget() { Layout = "${message} ${mdlc:World}. Welcome ${ndlc}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayHello().Wait();
            Assert.Single(target.Logs);
            Assert.Equal("Hello Earth. Welcome Earth People", target.Logs[0]);
        }

        [Fact]
        public void TestNonSerializableSayNothing()
        {
            var runner = GetRunner<CustomBeginScopeTestRunner>(new NLogProviderOptions() { IncludeScopes = false });
            var target = new NLog.Targets.MemoryTarget() { Layout = "${message} ${mdlc:World}. Welcome ${ndlc}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayHello().Wait();
            Assert.Single(target.Logs);
            Assert.Equal("Hello . Welcome ", target.Logs[0]);
        }

        [Fact]
        public void TestNonSerializableSayHi()
        {
            var runner = GetRunner<CustomBeginScopeTestRunner>();
            var target = new NLog.Targets.MemoryTarget() { Layout = "${message} ${mdlc:World}. Welcome ${ndlc}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target);
            runner.SayHi().Wait();
            Assert.Single(target.Logs);
            Assert.Equal("Hi Earth. Welcome Earth People", target.Logs[0]);
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
                using (_logger.BeginScope(new ActionLogScope("Earth")))
                {
                    await Task.Yield();
                    _logger.LogInformation("Hello");
                }
            }

            public async Task SayHi()
            {
                using (_logger.BeginScope("{World} People", "Earth"))
                {
                    await Task.Yield();
                    _logger.LogInformation("Hi");
                }
            }
        }

        private class ActionLogScope : IReadOnlyList<KeyValuePair<string, object>>
        {
            private readonly string _world;

            public ActionLogScope(string world)
            {
                if (world == null)
                {
                    throw new ArgumentNullException(nameof(world));
                }

                _world = world;
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return new KeyValuePair<string, object>("World", _world);
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
                return string.Concat(_world, " People");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
