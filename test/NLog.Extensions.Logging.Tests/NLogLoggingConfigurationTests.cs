using System;
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
        const string DefaultSectionName = "NLog";
        const string CustomSectionName = "MyCustomSection";

#if !NETCORE1_0
        [Fact]
        public void LoadSimpleConfig()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:Targets:file:fileName"] = "hello.txt";
           
            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void LoadSimpleConfigWithCustomKey()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule(CustomSectionName);
            memoryConfig[$"{CustomSectionName}:Targets:file:type"] = "File";
            memoryConfig[$"{CustomSectionName}:Targets:file:fileName"] = "hello.txt";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig, CustomSectionName);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void LoadSimpleConfigAndTrimSpace()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:Targets:file:fileName "] = "hello.txt";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void LoadWrapperConfig()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "AsyncWrapper";
            memoryConfig["NLog:Targets:file:target:wrappedFile:type"] = "File";
            memoryConfig["NLog:Targets:file:target:wrappedFile:fileName"] = "hello.txt";
            
            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig);
            
            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(3, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is AsyncTargetWrapper));
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("wrappedFile") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void LoadVariablesConfig()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:Targets:file:fileName"] = "${var_filename}";
            memoryConfig["NLog:Variables:var_filename"] = "hello.txt";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void LoadDefaultWrapperConfig()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:Targets:file:fileName"] = "hello.txt";
            memoryConfig["NLog:Default-wrapper:type"] = "AsyncWrapper";
            memoryConfig["NLog:Default-wrapper:batchSize"] = "1";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, (logConfig.AllTargets.Count(t => t is AsyncTargetWrapper asyncTarget && asyncTarget.BatchSize == 1)));
            Assert.Single(logConfig.AllTargets.Where(t => t is AsyncTargetWrapper asyncTarget && asyncTarget.WrappedTarget is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is AsyncTargetWrapper asyncTarget && asyncTarget.WrappedTarget is ConsoleTarget));
            Assert.Equal("hello.txt", ((logConfig.FindTargetByName("file") as AsyncTargetWrapper)?.WrappedTarget as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void LoadDefaultTargetParametersConfig()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:Default-target-parameters:file:filename"] = "hello.txt";
            memoryConfig["NLog:Default-target-parameters:console:error"] = "true";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName?.Render(LogEventInfo.CreateNullEvent()));
            Assert.True((logConfig.FindTargetByName("console") as ConsoleTarget)?.Error);
        }

        [Fact]
        public void LoadDefaultTargetParametersJsonLayoutConfig()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:default-target-parameters:file:filename"] = "hello.txt";
            memoryConfig["NLog:default-target-parameters:file:layout:type"] = "JsonLayout";
            memoryConfig["NLog:default-target-parameters:file:layout:Attributes:0:name"] = "timestamp";
            memoryConfig["NLog:default-target-parameters:file:layout:Attributes:0:layout"] = "${date:format=o}";
            memoryConfig["NLog:default-target-parameters:file:layout:Attributes:1:name"] = "level";
            memoryConfig["NLog:default-target-parameters:file:layout:Attributes:1:layout"] = "${level}";
            memoryConfig["NLog:default-target-parameters:file:layout:Attributes:2:name"] = "message";
            memoryConfig["NLog:default-target-parameters:file:layout:Attributes:2:layout"] = "${message}";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
            Assert.Equal(3, ((logConfig.FindTargetByName("file") as FileTarget)?.Layout as NLog.Layouts.JsonLayout)?.Attributes?.Count);
        }

        [Fact]
        public void ReloadLogFactoryConfiguration()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:Targets:file:fileName"] = "hello.txt";
            memoryConfig["NLog:AutoReload"] = "true";
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var logFactory = new LogFactory();
            var logConfig = new NLogLoggingConfiguration(configuration.GetSection("NLog"), logFactory);
            logFactory.Configuration = logConfig;
            Assert.Equal(2, logFactory.Configuration.LoggingRules[0].Targets.Count);
            configuration["NLog:Rules:0:writeTo"] = "Console";
            logFactory.Configuration = logConfig.Reload();  // Manual Reload
            Assert.Equal(1, logFactory.Configuration.LoggingRules[0].Targets.Count);
            configuration["NLog:Rules:0:writeTo"] = "File,Console";
            configuration.Reload(); // Automatic Reload
            Assert.Equal(2, logFactory.Configuration.LoggingRules[0].Targets.Count);
            logFactory.Dispose();
            configuration.Reload(); // Nothing should happen
        }

        [Fact]
        public void ReloadLogFactoryConfigurationKeepVariables()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:Targets:file:fileName"] = "${var:var_filename}";
            memoryConfig["NLog:autoreload"] = "true";
            memoryConfig["NLog:KeepVariablesOnReload"] = "true";
            memoryConfig["NLog:variables:var_filename"] = "hello.txt";
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var logFactory = new LogFactory();
            var logConfig = new NLogLoggingConfiguration(configuration.GetSection("NLog"), logFactory);
            logFactory.Configuration = logConfig;
            Assert.Equal("hello.txt", (logFactory.Configuration.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
            logFactory.Configuration.Variables["var_filename"] = "updated.txt";
            Assert.Equal("updated.txt", (logFactory.Configuration.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
            configuration.Reload(); // Automatic Reload
            Assert.Equal("updated.txt", (logFactory.Configuration.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void SetupBuilderLoadConfigurationFromSection()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:Targets:file:fileName"] = "hello.txt";
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();

            var logFactory = new LogFactory();
            logFactory.Setup()
                .SetupExtensions(s => s.AutoLoadAssemblies(false))
                .LoadConfigurationFromSection(configuration);

            Assert.Single(logFactory.Configuration.LoggingRules);
            Assert.Equal(2, logFactory.Configuration.LoggingRules[0].Targets.Count);
        }

        private static NLogLoggingConfiguration CreateNLogLoggingConfigurationWithNLogSection(IDictionary<string, string> memoryConfig, string sectionName = DefaultSectionName)
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var logFactory = new LogFactory();
            var logConfig = new NLogLoggingConfiguration(configuration.GetSection(sectionName), logFactory);
            return logConfig;
        }

        private static Dictionary<string, string> CreateMemoryConfigConsoleTargetAndRule(string sectionName = DefaultSectionName)
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig[$"{sectionName}:Rules:0:logger"] = "*";
            memoryConfig[$"{sectionName}:Rules:0:minLevel"] = "Trace";
            memoryConfig[$"{sectionName}:Rules:0:writeTo"] = "File,Console";
            memoryConfig[$"{sectionName}:Targets:console:type"] = "Console";

            return memoryConfig;
        }
#endif
    }
}
