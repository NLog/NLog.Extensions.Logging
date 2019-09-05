using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NLog.Extensions.Logging.Tests
{
    public class NLogTestBase
    {
        IServiceProvider _serviceProvider;
        protected NLogLoggerProvider _nlogProvider;

        protected NLogLoggerProvider ConfigureLoggerProvider(NLogProviderOptions options = null, Action<ServiceCollection> configureServices = null)
        {
            if (_serviceProvider == null)
            {
                var logFactory = new LogFactory();
                _nlogProvider = new NLogLoggerProvider(options ?? new NLogProviderOptions() { CaptureMessageTemplates = true, CaptureMessageProperties = true }, logFactory);
                var services = new ServiceCollection();
                services.AddSingleton<ILoggerFactory, LoggerFactory>();
                services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                configureServices?.Invoke(services);
                _serviceProvider = services.BuildServiceProvider();
                var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
                loggerFactory.AddProvider(_nlogProvider);
            }
            return _nlogProvider;
        }

        protected IServiceProvider ConfigureTransientService<T>(Action<ServiceCollection> configureServices = null, NLogProviderOptions options = null) where T : class
        {
            if (_serviceProvider == null)
                ConfigureLoggerProvider(options, s => { s.AddTransient<T>(); configureServices?.Invoke(s); });
            return _serviceProvider;
        }

        protected void ConfigureNLog(NLog.Targets.Target nlogTarget = null)
        {
            nlogTarget = nlogTarget ?? new NLog.Targets.MemoryTarget("output") { Layout = "${message}" };
            var nlogConfig = new NLog.Config.LoggingConfiguration(_nlogProvider.LogFactory);
            nlogConfig.AddRuleForAllLevels(nlogTarget);
            if (nlogTarget is NLog.Targets.Wrappers.WrapperTargetBase wrapperTarget)
                nlogConfig.AddTarget(wrapperTarget.WrappedTarget);
            _nlogProvider.LogFactory.Configuration = nlogConfig;
        }

        protected T GetRunner<T>(NLogProviderOptions options = null) where T : class
        {
            // Start program
            var runner = ConfigureTransientService<T>(null, options).GetRequiredService<T>();
            return runner;
        }
    }
}
