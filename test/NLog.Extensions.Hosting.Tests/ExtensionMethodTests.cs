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
            TestHostingResult(actual);
        }

        [Fact]
        public void UseNLog_withOptionsParam_WorksWithNLog()
        {
            var someParam = new NLogProviderOptions {CaptureMessageProperties = false, CaptureMessageTemplates = false};
            var actual = new HostBuilder().UseNLog(someParam).Build();
            TestHostingResult(actual);
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
            TestHostingResult(actual);
        }

        private static void TestHostingResult(IHost host)
        {
            try
            {
                var nlogTarget = new Targets.MemoryTarget() { Name = "Output" };
                Config.SimpleConfigurator.ConfigureForTargetLogging(nlogTarget);

                var loggerFactory = host.Services.GetService<ILoggerFactory>();
                Assert.NotNull(loggerFactory);

                var logger = loggerFactory.CreateLogger("Hello");
                logger.LogError("World");
                Assert.NotEmpty(nlogTarget.Logs);
            }
            finally
            {
                LogManager.Configuration = null;
            }
        }
    }
}