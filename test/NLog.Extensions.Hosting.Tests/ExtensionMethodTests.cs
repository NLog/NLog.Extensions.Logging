using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Hosting.Tests
{
    public class ExtensionMethodTests
    {
        [Fact]
        public void UseNLog_noParams_WorksWithNLog()
        {
            var actual = new HostBuilder().UseNLog().Build();
            TestHostingResult(actual, true);
        }

        [Fact]
        public void UseNLog_withOptionsParam_WorksWithNLog()
        {
            var someParam = new NLogProviderOptions {CaptureMessageProperties = false, CaptureMessageTemplates = false};
            var actual = new HostBuilder().UseNLog(someParam).Build();
            TestHostingResult(actual, true);
        }

        [Fact]
        public void UseNLog_withConfiguration_WorksWithNLog()
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig["NLog:CaptureMessageProperties"] = "true";
            memoryConfig["NLog:CaptureMessageTemplates"] = "false";
            memoryConfig["NLog:IgnoreScopes"] = "false";

            var someParam = new NLogProviderOptions { CaptureMessageProperties = false, CaptureMessageTemplates = false };
            var actual = new HostBuilder().ConfigureHostConfiguration(config => config.AddInMemoryCollection(memoryConfig)).UseNLog(someParam).Build();
            TestHostingResult(actual, true);
        }

        [Fact]
        public void AddNLog_withShutdownOnDispose_worksWithNLog()
        {
            var someParam = new NLogProviderOptions { ShutdownOnDispose = true };
            var actual = new HostBuilder().ConfigureLogging(l => l.AddNLog(someParam)).Build();
            try
            {
                TestHostingResult(actual, false);
                Assert.NotNull(LogManager.Configuration);
            }
            finally
            {
                actual.Dispose();
                Assert.Null(LogManager.Configuration);
            }
        }

        [Fact]
        public void UseNLog_withAddNLog_worksWithNLog()
        {
            var actual = new HostBuilder().UseNLog().ConfigureServices((h,s) => s.AddLogging(l => l.AddNLog())).Build();
            TestHostingResult(actual, true);
        }

        private static void TestHostingResult(IHost host, bool resetConfiguration)
        {
            try
            {
                var nlogTarget = new Targets.MemoryTarget() { Name = "Output" };
                Config.SimpleConfigurator.ConfigureForTargetLogging(nlogTarget, LogLevel.Fatal);

                var loggerFactory = host.Services.GetService<ILoggerFactory>();
                Assert.NotNull(loggerFactory);

                var logger = loggerFactory.CreateLogger("Hello");
                logger.LogCritical("World");
                Assert.Single(nlogTarget.Logs);
            }
            finally
            {
                if (resetConfiguration)
                    LogManager.Configuration = null;
            }
        }
    }
}