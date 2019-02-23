using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class NLogLoggingConfigurationTests
    {
#if !NETCORE1_0
        [Fact]
        public void LoadSimpleConfig()
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:Targets:file:fileName"] = "hello.txt";
            memoryConfig["NLog:Targets:console:type"] = "Console";
            memoryConfig["NLog:Rules:0:logger"] = "*";
            memoryConfig["NLog:Rules:0:minLevel"] = "Trace";
            memoryConfig["NLog:Rules:0:writeTo"] = "File,Console";
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var logFactory = new LogFactory();
            var logConfig = new NLogLoggingConfiguration(configuration.GetSection("NLog"), logFactory);
            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
        }

        [Fact]
        public void LoadWrapperConfig()
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig["NLog:Targets:file:type"] = "AsyncWrapper";
            memoryConfig["NLog:Targets:file:target:wrappedFile:type"] = "File";
            memoryConfig["NLog:Targets:file:target:wrappedFile:fileName"] = "hello.txt";
            memoryConfig["NLog:Targets:console:type"] = "Console";
            memoryConfig["NLog:Rules:0:logger"] = "*";
            memoryConfig["NLog:Rules:0:minLevel"] = "Trace";
            memoryConfig["NLog:Rules:0:writeTo"] = "File,Console";
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var logFactory = new LogFactory();
            var logConfig = new NLogLoggingConfiguration(configuration.GetSection("NLog"), logFactory);
            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(3, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is AsyncTargetWrapper));
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
        }
#endif
    }
}
