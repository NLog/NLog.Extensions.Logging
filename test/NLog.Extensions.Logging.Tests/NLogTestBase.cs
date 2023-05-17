using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NLog.Extensions.Logging.Tests
{
    public abstract class NLogTestBase
    {
        private IServiceProvider _serviceProvider;
        protected NLogLoggerProvider LoggerProvider { get; private set; }

        protected NLogLoggerProvider ConfigureLoggerProvider(NLogProviderOptions options = null, Action<ServiceCollection> configureServices = null)
        {
            if (_serviceProvider is null)
            {
                var services = new ServiceCollection();
                services.AddLogging(builder => builder.AddNLog(options ?? new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true }, provider => new LogFactory()));
                configureServices?.Invoke(services);
                _serviceProvider = services.BuildServiceProvider();
                LoggerProvider = _serviceProvider.GetRequiredService<ILoggerProvider>() as NLogLoggerProvider;
            }
            return LoggerProvider;
        }

        protected IServiceProvider ConfigureTransientService<T>(Action<ServiceCollection> configureServices = null, NLogProviderOptions options = null) where T : class
        {
            if (_serviceProvider is null)
                ConfigureLoggerProvider(options, s => { s.AddTransient<T>(); configureServices?.Invoke(s); });
            return _serviceProvider;
        }

        protected void ConfigureNLog(Targets.Target target = null)
        {
            target = target ?? new Targets.MemoryTarget("output") { Layout = "${message}" };
            var nlogConfig = new Config.LoggingConfiguration(LoggerProvider.LogFactory);
            nlogConfig.AddRuleForAllLevels(target);
            if (target is Targets.Wrappers.WrapperTargetBase wrapperTarget)
                nlogConfig.AddTarget(wrapperTarget.WrappedTarget);
            LoggerProvider.LogFactory.Configuration = nlogConfig;
        }

        protected T GetRunner<T>(NLogProviderOptions options = null) where T : class
        {
            // Start program
            var runner = ConfigureTransientService<T>(null, options).GetRequiredService<T>();
            return runner;
        }

        protected void SetupTestRunner<TRunner>(Type implType, NLogProviderOptions options = null) where TRunner : class
        {
            ConfigureTransientService<TRunner>(s => s.AddSingleton(typeof(ILogger<>), implType), options);
        }
    }
}
