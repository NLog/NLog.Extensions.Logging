using System;
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
        public void UseNLog_ArgumentNullException()
        {
            IHostBuilder hostBuilder = null;
            var argNulLException = Assert.Throws<ArgumentNullException>(() => hostBuilder.UseNLog());
            Assert.Equal("builder", argNulLException.ParamName);
        }

        [Fact]
        public void UseNLog_noParams_WorksWithNLog()
        {
            var actual = new HostBuilder().UseNLog().Build();
            TestHostingResult(actual, true);
        }

        [Fact]
        public void UseNLog_withOptionsParam_WorksWithNLog()
        {
            var someParam = new NLogProviderOptions { CaptureMessageProperties = false, CaptureMessageTemplates = false };
            var actual = new HostBuilder().UseNLog(someParam).Build();
            TestHostingResult(actual, true);
        }

        [Fact]
        public void UseNLog_ShouldLogToNLog()
        {
            var nlogTarget = new Targets.MemoryTarget() { Name = "Output" };
            var builder = new HostBuilder().UseNLog(null, (serviceProvider) =>
            {
                var nLogFactory = new LogFactory().Setup().LoadConfiguration(c => c.ForLogger().WriteTo(nlogTarget)).LogFactory;
                return nLogFactory;
            });

            var loggerFactory = builder.Build().Services.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);

            var logger = loggerFactory.CreateLogger("Hello");
            logger.LogCritical("World");
            Assert.Single(nlogTarget.Logs);
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
                NLog.LogManager.Setup().LoadConfiguration(c => c.ForLogger().WriteTo(nlogTarget));

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

        [Fact]
        public void UseNLog_ReplaceLoggerFactory()
        {
            var actual = new HostBuilder().ConfigureServices(svc => svc.AddLogging()).UseNLog(new NLogProviderOptions() { ReplaceLoggerFactory = true, RemoveLoggerFactoryFilter = true }).Build();

            var loggerFactory = actual.Services.GetService<ILoggerFactory>();

            Assert.Equal(typeof(NLogLoggerFactory), loggerFactory.GetType());
        }

        [Fact]
        public void UseNLog_LoadConfigurationFromSection()
        {
            var host = new HostBuilder().ConfigureAppConfiguration((context, config) =>
            {
                var memoryConfig = new Dictionary<string, string>();
                memoryConfig["NLog:Rules:0:logger"] = "*";
                memoryConfig["NLog:Rules:0:minLevel"] = "Trace";
                memoryConfig["NLog:Rules:0:writeTo"] = "inMemory";
                memoryConfig["NLog:Targets:inMemory:type"] = "Memory";
                memoryConfig["NLog:Targets:inMemory:layout"] = "${logger}|${message}|${configsetting:NLog.Targets.inMemory.type}";
                config.AddInMemoryCollection(memoryConfig);
            }).UseNLog(new NLogProviderOptions() { LoggingConfigurationSectionName = "NLog", ReplaceLoggerFactory = true }).Build();

            var loggerFact = host.Services.GetService<ILoggerFactory>();
            var logger = loggerFact.CreateLogger("logger1");

            ConfigSettingLayoutRenderer.DefaultConfiguration = null;    // See dependency resolving is working

            logger.LogError("error1");

            var loggerProvider = host.Services.GetService<ILoggerProvider>() as NLogLoggerProvider;
            var logged = loggerProvider.LogFactory.Configuration.FindTargetByName<Targets.MemoryTarget>("inMemory").Logs;

            Assert.Single(logged);
            Assert.Equal("logger1|error1|Memory", logged[0]);
            //Reset
            LogManager.Configuration = null;
        }

#if !NET6_0
        [Fact]
        public void IHostApplicationBuilder_UseNLog_ArgumentNullException()
        {
            IHostApplicationBuilder hostBuilder = null;
            var argNulLException = Assert.Throws<ArgumentNullException>(() => hostBuilder.UseNLog());
            Assert.Equal("builder", argNulLException.ParamName);
        }

        [Fact]
        public void IHostApplicationBuilder_UseNLog_noParams_WorksWithNLog()
        {
            var builder = new HostApplicationBuilder();
            builder.UseNLog();

            var actual = builder.Build();
            TestHostingResult(actual, true);
        }

        [Fact]
        public void IHostApplicationBuilder_UseNLog_withOptionsParam_WorksWithNLog()
        {
            var someParam = new NLogProviderOptions { CaptureMessageProperties = false, CaptureMessageTemplates = false };

            var builder = new HostApplicationBuilder();
            builder.UseNLog(someParam);

            var actual = builder.Build();
            TestHostingResult(actual, true);
        }

        [Fact]
        public void IHostApplicationBuilder_UseNLog_withConfiguration_WorksWithNLog()
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig["NLog:CaptureMessageProperties"] = "true";
            memoryConfig["NLog:CaptureMessageTemplates"] = "false";
            memoryConfig["NLog:IgnoreScopes"] = "false";

            var someParam = new NLogProviderOptions { CaptureMessageProperties = false, CaptureMessageTemplates = false };

            var builder = new HostApplicationBuilder();
            builder.Configuration.AddInMemoryCollection(memoryConfig);
            builder.UseNLog(someParam);

            var actual = builder.Build();
            TestHostingResult(actual, true);
        }

        [Fact]
        public void IHostApplicationBuilder_AddNLog_withShutdownOnDispose_worksWithNLog()
        {
            var someParam = new NLogProviderOptions { ShutdownOnDispose = true };

            var builder = new HostApplicationBuilder();
            builder.Logging.AddNLog(someParam);

            var actual = builder.Build();
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
        public void IHostApplicationBuilder_UseNLog_withAddNLog_worksWithNLog()
        {
            var builder = new HostApplicationBuilder();
            builder.UseNLog();
            builder.Logging.AddNLog();

            var actual = builder.Build();
            TestHostingResult(actual, true);
        }

        [Fact]
        public void IHostApplicationBuilder_UseNLog_ReplaceLoggerFactory()
        {
            var builder = new HostApplicationBuilder();
            builder.Services.AddLogging();
            builder.UseNLog(new NLogProviderOptions() { ReplaceLoggerFactory = true, RemoveLoggerFactoryFilter = true });

            var actual = builder.Build();

            var loggerFactory = actual.Services.GetService<ILoggerFactory>();

            Assert.Equal(typeof(NLogLoggerFactory), loggerFactory.GetType());
        }

        [Fact]
        public void IHostApplicationBuilder_UseNLog_LoadConfigurationFromSection()
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig["NLog:Rules:0:logger"] = "*";
            memoryConfig["NLog:Rules:0:minLevel"] = "Trace";
            memoryConfig["NLog:Rules:0:writeTo"] = "inMemory";
            memoryConfig["NLog:Targets:inMemory:type"] = "Memory";
            memoryConfig["NLog:Targets:inMemory:layout"] = "${logger}|${message}|${configsetting:NLog.Targets.inMemory.type}";

            var builder = new HostApplicationBuilder();
            builder.Configuration.AddInMemoryCollection(memoryConfig);
            builder.UseNLog(new NLogProviderOptions() { LoggingConfigurationSectionName = "NLog", ReplaceLoggerFactory = true });

            var host = builder.Build();

            var loggerFact = host.Services.GetService<ILoggerFactory>();
            var logger = loggerFact.CreateLogger("logger1");

            ConfigSettingLayoutRenderer.DefaultConfiguration = null;    // See dependency resolving is working

            logger.LogError("error1");

            var loggerProvider = host.Services.GetService<ILoggerProvider>() as NLogLoggerProvider;
            var logged = loggerProvider.LogFactory.Configuration.FindTargetByName<Targets.MemoryTarget>("inMemory").Logs;

            Assert.Single(logged);
            Assert.Equal("logger1|error1|Memory", logged[0]);
            //Reset
            LogManager.Configuration = null;
        }

        [Fact]
        public void IHostApplicationBuilder_UseNLog_ShouldLogToNLog()
        {
            var nlogTarget = new Targets.MemoryTarget() { Name = "Output" };
            var builder = new HostApplicationBuilder();
            builder.UseNLog(null, (ServiceProvider) =>
            {
                var nLogFactory = new LogFactory().Setup().LoadConfiguration(c => c.ForLogger().WriteTo(nlogTarget)).LogFactory;
                return nLogFactory;
            });

            var actual = builder.Build();

            var loggerFactory = actual.Services.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);

            var logger = loggerFactory.CreateLogger("Hello");
            logger.LogCritical("World");
            Assert.Single(nlogTarget.Logs);
        }
#endif
    }
}