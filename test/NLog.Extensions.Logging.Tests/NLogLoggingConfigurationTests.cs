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
        [Theory]
        [InlineData(DefaultSectionName)]
        [InlineData(CustomSectionName)]
        public void LoadSimpleConfig(string sectionName)
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule(sectionName);
            memoryConfig[$"{sectionName}:Targets:file:type"] = "File";
            memoryConfig[$"{sectionName}:Targets:file:fileName"] = "hello.txt";
           
            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig, sectionName);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Theory]
        [InlineData(DefaultSectionName)]
        [InlineData(CustomSectionName)]
        public void LoadSimpleConfigAndTrimSpace(string sectionName)
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule(sectionName);
            memoryConfig[$"{sectionName}:Targets:file:type"] = "File";
            memoryConfig[$"{sectionName}:Targets:file:fileName "] = "hello.txt";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig, sectionName);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Theory]
        [InlineData(DefaultSectionName)]
        [InlineData(CustomSectionName)]
        public void LoadWrapperConfig(string sectionName)
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule(sectionName);
            memoryConfig[$"{sectionName}:Targets:file:type"] = "AsyncWrapper";
            memoryConfig[$"{sectionName}:Targets:file:target:wrappedFile:type"] = "File";
            memoryConfig[$"{sectionName}:Targets:file:target:wrappedFile:fileName"] = "hello.txt";
            
            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig, sectionName);
            
            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(3, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is AsyncTargetWrapper));
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("wrappedFile") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Theory]
        [InlineData(DefaultSectionName)]
        [InlineData(CustomSectionName)]
        public void LoadVariablesConfig(string sectionName)
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule(sectionName);
            memoryConfig[$"{sectionName}:Targets:file:type"] = "File";
            memoryConfig[$"{sectionName}:Targets:file:fileName"] = "${var_filename}";
            memoryConfig[$"{sectionName}:Variables:var_filename"] = "hello.txt";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig, sectionName);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Theory]
        [InlineData(DefaultSectionName)]
        [InlineData(CustomSectionName)]
        public void LoadDefaultWrapperConfig(string sectionName)
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule(sectionName);
            memoryConfig[$"{sectionName}:Targets:file:type"] = "File";
            memoryConfig[$"{sectionName}:Targets:file:fileName"] = "hello.txt";
            memoryConfig[$"{sectionName}:Default-wrapper:type"] = "AsyncWrapper";
            memoryConfig[$"{sectionName}:Default-wrapper:batchSize"] = "1";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig, sectionName);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, (logConfig.AllTargets.Count(t => t is AsyncTargetWrapper asyncTarget && asyncTarget.BatchSize == 1)));
            Assert.Single(logConfig.AllTargets.Where(t => t is AsyncTargetWrapper asyncTarget && asyncTarget.WrappedTarget is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is AsyncTargetWrapper asyncTarget && asyncTarget.WrappedTarget is ConsoleTarget));
            Assert.Equal("hello.txt", ((logConfig.FindTargetByName("file") as AsyncTargetWrapper)?.WrappedTarget as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Theory]
        [InlineData(DefaultSectionName)]
        [InlineData(CustomSectionName)]
        public void LoadDefaultTargetParametersConfig(string sectionName)
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule(sectionName);
            memoryConfig[$"{sectionName}:Targets:file:type"] = "File";
            memoryConfig[$"{sectionName}:Default-target-parameters:file:filename"] = "hello.txt";
            memoryConfig[$"{sectionName}:Default-target-parameters:console:error"] = "true";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig, sectionName);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName?.Render(LogEventInfo.CreateNullEvent()));
            Assert.True((logConfig.FindTargetByName("console") as ConsoleTarget)?.Error);
        }

        [Theory]
        [InlineData(DefaultSectionName)]
        [InlineData(CustomSectionName)]
        public void LoadDefaultTargetParametersJsonLayoutConfig(string sectionName)
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule(sectionName);
            memoryConfig[$"{sectionName}:Targets:file:type"] = "File";
            memoryConfig[$"{sectionName}:default-target-parameters:file:filename"] = "hello.txt";
            memoryConfig[$"{sectionName}:default-target-parameters:file:layout:type"] = "JsonLayout";
            memoryConfig[$"{sectionName}:default-target-parameters:file:layout:Attributes:0:name"] = "timestamp";
            memoryConfig[$"{sectionName}:default-target-parameters:file:layout:Attributes:0:layout"] = "${date:format=o}";
            memoryConfig[$"{sectionName}:default-target-parameters:file:layout:Attributes:1:name"] = "level";
            memoryConfig[$"{sectionName}:default-target-parameters:file:layout:Attributes:1:layout"] = "${level}";
            memoryConfig[$"{sectionName}:default-target-parameters:file:layout:Attributes:2:name"] = "message";
            memoryConfig[$"{sectionName}:default-target-parameters:file:layout:Attributes:2:layout"] = "${message}";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig, sectionName);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
            Assert.Equal(3, ((logConfig.FindTargetByName("file") as FileTarget)?.Layout as NLog.Layouts.JsonLayout)?.Attributes?.Count);
        }

        [Theory]
        [InlineData(DefaultSectionName)]
        [InlineData(CustomSectionName)]
        public void ReloadLogFactoryConfiguration(string sectionName)
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule(sectionName);
            memoryConfig[$"{sectionName}:Targets:file:type"] = "File";
            memoryConfig[$"{sectionName}:Targets:file:fileName"] = "hello.txt";
            memoryConfig[$"{sectionName}:AutoReload"] = "true";
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var logFactory = new LogFactory();
            var logConfig = new NLogLoggingConfiguration(configuration.GetSection(sectionName), logFactory);
            logFactory.Configuration = logConfig;
            Assert.Equal(2, logFactory.Configuration.LoggingRules[0].Targets.Count);
            configuration[$"{sectionName}:Rules:0:writeTo"] = "Console";
            logFactory.Configuration = logConfig.Reload();  // Manual Reload
            Assert.Equal(1, logFactory.Configuration.LoggingRules[0].Targets.Count);
            configuration[$"{sectionName}:Rules:0:writeTo"] = "File,Console";
            configuration.Reload(); // Automatic Reload
            Assert.Equal(2, logFactory.Configuration.LoggingRules[0].Targets.Count);
            logFactory.Dispose();
            configuration.Reload(); // Nothing should happen
        }

        [Theory]
        [InlineData(DefaultSectionName)]
        [InlineData(CustomSectionName)]
        public void ReloadLogFactoryConfigurationKeepVariables(string sectionName)
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule(sectionName);
            memoryConfig[$"{sectionName}:Targets:file:type"] = "File";
            memoryConfig[$"{sectionName}:Targets:file:fileName"] = "${var:var_filename}";
            memoryConfig[$"{sectionName}:autoreload"] = "true";
            memoryConfig[$"{sectionName}:KeepVariablesOnReload"] = "true";
            memoryConfig[$"{sectionName}:variables:var_filename"] = "hello.txt";
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var logFactory = new LogFactory();
            var logConfig = new NLogLoggingConfiguration(configuration.GetSection(sectionName), logFactory);
            logFactory.Configuration = logConfig;
            Assert.Equal("hello.txt", (logFactory.Configuration.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
            logFactory.Configuration.Variables["var_filename"] = "updated.txt";
            Assert.Equal("updated.txt", (logFactory.Configuration.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
            configuration.Reload(); // Automatic Reload
            Assert.Equal("updated.txt", (logFactory.Configuration.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
        }

        [Theory]
        [InlineData(DefaultSectionName)]
        [InlineData(CustomSectionName)]
        public void SetupBuilderLoadConfigurationFromSection(string sectionName)
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule(sectionName);
            memoryConfig[$"{sectionName}:Targets:file:type"] = "File";
            memoryConfig[$"{sectionName}:Targets:file:fileName"] = "hello.txt";
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();

            var logFactory = new LogFactory();
            logFactory.Setup()
                .SetupExtensions(s => s.AutoLoadAssemblies(false))
                .LoadConfigurationFromSection(configuration, sectionName);

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
