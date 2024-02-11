using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public void LoadConfigShouldIgnoreEmptySections()
        {
            var appSettings = @"
{
  ""NLog"": {
    ""throwConfigExceptions"": true,
    ""variables"": {},
    ""extensions"": [],
    ""targets"": {},
    ""rules"": [],
    ""keyThatPretendsToBeAComplexStructure"": {}
  }
}";
            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(appSettings);
            
            Assert.False(logConfig.LoggingRules.Any());
            Assert.False(logConfig.AllTargets.Any());
            Assert.False(logConfig.Variables.Any());
        }
        
        [Fact]
        public void LoadConfigShouldThrowForUnrecognisedSections()
        {
            var appSettings = @"
{
  ""NLog"": {
    ""throwConfigExceptions"": true,
    ""someRandomKey"": null
  }
}";
            var ex = Assert.Throws<NLog.NLogConfigurationException>(() =>
                CreateNLogLoggingConfigurationWithNLogSection(appSettings));
            Assert.Equal("Unrecognized value 'someRandomKey'='' for element 'NLog'", ex.Message); 
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
        public void LoadWrapperConfigExplicitName()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "AsyncWrapper";
            memoryConfig["NLog:Targets:file:target:type"] = "Memory";
            memoryConfig["NLog:Targets:file:target:name"] = "wrappedMem";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(3, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is AsyncTargetWrapper));
            Assert.Single(logConfig.AllTargets.Where(t => t is MemoryTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.NotNull(logConfig.FindTargetByName("wrappedMem") as MemoryTarget);
        }

        [Fact]
        public void LoadWrapperConfigWithoutName()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "AsyncWrapper";
            memoryConfig["NLog:Targets:file:target:type"] = "Memory";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is AsyncTargetWrapper));
            Assert.True(logConfig.FindTargetByName<AsyncTargetWrapper>("file")?.WrappedTarget is MemoryTarget);
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
        public void LoadVariableJsonLayoutConfig()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:Targets:file:fileName"] = "hello.txt";
            memoryConfig["NLog:Targets:file:layout"] = "${my_json}";
            memoryConfig["NLog:Targets:console:layout"] = "${my_json}";
            memoryConfig["NLog:Variables:my_json:type"] = "JsonLayout";
            memoryConfig["NLog:Variables:my_json:attributes:0:name"] = "message";
            memoryConfig["NLog:Variables:my_json:attributes:0:layout"] = "${message}";
            memoryConfig["NLog:Variables:my_json:attributes:1:name"] = "logger";
            memoryConfig["NLog:Variables:my_json:attributes:1:layout"] = "${logger}";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(1, logConfig.Variables.Count);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal(2, logConfig.AllTargets.Count(t => (t as TargetWithLayout)?.Layout is NLog.Layouts.JsonLayout));
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
        public void LoadTargetDefaultWrapperConfig()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:Targets:file:fileName"] = "hello.txt";
            memoryConfig["NLog:TargetDefaultWrapper:type"] = "AsyncWrapper";
            memoryConfig["NLog:TargetDefaultWrapper:batchSize"] = "1";

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
            Assert.True((logConfig.FindTargetByName("console") as ConsoleTarget)?.StdErr);
        }

        [Fact]
        public void LoadTargetDefaultParametersConfig()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:TargetDefaultParameters:file:filename"] = "hello.txt";
            memoryConfig["NLog:TargetDefaultParameters:console:error"] = "true";

            var logConfig = CreateNLogLoggingConfigurationWithNLogSection(memoryConfig);

            Assert.Single(logConfig.LoggingRules);
            Assert.Equal(2, logConfig.LoggingRules[0].Targets.Count);
            Assert.Equal(2, logConfig.AllTargets.Count);
            Assert.Single(logConfig.AllTargets.Where(t => t is FileTarget));
            Assert.Single(logConfig.AllTargets.Where(t => t is ConsoleTarget));
            Assert.Equal("hello.txt", (logConfig.FindTargetByName("file") as FileTarget)?.FileName?.Render(LogEventInfo.CreateNullEvent()));
            Assert.True((logConfig.FindTargetByName("console") as ConsoleTarget)?.StdErr);
        }

        [Fact]
        public void LoadDefaultTargetParametersJsonLayoutConfig()
        {
            var memoryConfig = CreateMemoryConfigConsoleTargetAndRule();
            memoryConfig["NLog:Targets:file:type"] = "File";
            memoryConfig["NLog:TargetDefaultParameters:file:filename"] = "hello.txt";
            memoryConfig["NLog:TargetDefaultParameters:file:layout:type"] = "JsonLayout";
            memoryConfig["NLog:TargetDefaultParameters:file:layout:Attributes:0:name"] = "timestamp";
            memoryConfig["NLog:TargetDefaultParameters:file:layout:Attributes:0:layout"] = "${date:format=o}";
            memoryConfig["NLog:TargetDefaultParameters:file:layout:Attributes:1:name"] = "level";
            memoryConfig["NLog:TargetDefaultParameters:file:layout:Attributes:1:layout"] = "${level}";
            memoryConfig["NLog:TargetDefaultParameters:file:layout:Attributes:2:name"] = "message";
            memoryConfig["NLog:TargetDefaultParameters:file:layout:Attributes:2:layout"] = "${message}";

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
            memoryConfig["NLog:Targets:file:fileName"] = "${var_filename}";
            memoryConfig["NLog:autoreload"] = "true";
            memoryConfig["NLog:variables:var_filename"] = "hello.txt";
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var logFactory = new LogFactory();
            var logConfig = new NLogLoggingConfiguration(configuration.GetSection("NLog"), logFactory);
            logFactory.Configuration = logConfig;
            Assert.Equal("hello.txt", (logFactory.Configuration.FindTargetByName("file") as FileTarget)?.FileName.Render(LogEventInfo.CreateNullEvent()));
            logFactory.Configuration.Variables["var_filename"] = "updated.txt";
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
        
        private static NLogLoggingConfiguration CreateNLogLoggingConfigurationWithNLogSection(string appSettingsContent, string sectionName = DefaultSectionName)
        {
            var configuration = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettingsContent))).Build();
            var logFactory = new LogFactory();
            var logConfig = new NLogLoggingConfiguration(configuration.GetSection(sectionName), logFactory);
            return logConfig;
        }

        private static Dictionary<string, string> CreateMemoryConfigConsoleTargetAndRule(string sectionName = DefaultSectionName)
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig[$"{sectionName}:throwConfigExceptions"] = "true";
            memoryConfig[$"{sectionName}:Rules:0:logger"] = "*";
            memoryConfig[$"{sectionName}:Rules:0:minLevel"] = "Trace";
            memoryConfig[$"{sectionName}:Rules:0:writeTo"] = "File,Console";
            memoryConfig[$"{sectionName}:Targets:console:type"] = "Console";

            return memoryConfig;
        }
    }
}
