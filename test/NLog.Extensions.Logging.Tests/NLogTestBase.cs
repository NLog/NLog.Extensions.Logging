using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NLog.Extensions.Logging.Tests
{
    public abstract class NLogTestBase
    {
        private IServiceProvider _serviceProvider;

        protected NLogTestBase()
        {
            LogManager.ThrowExceptions = true;
        }

        protected IServiceProvider SetupServiceProvider(NLogProviderOptions options = null, Targets.Target target = null, Action<ServiceCollection> configureServices = null)
        {
            IServiceProvider serviceProvider = _serviceProvider;

            if (serviceProvider is null || options != null || target != null)
            {
                var configTarget = target ?? new Targets.MemoryTarget("output") { Layout = "${logger}|${level:uppercase=true}|${message}|${all-event-properties:includeScopeProperties=true}${onexception:|}${exception:format=tostring}" };

                var services = new ServiceCollection();
                services.AddLogging(builder => builder.AddNLog(options ?? new NLogProviderOptions(), provider => new LogFactory()));
                configureServices?.Invoke(services);
                serviceProvider = services.BuildServiceProvider();
                if (options is null && target is null)
                {
                    _serviceProvider = serviceProvider;
                    var loggerProvider = serviceProvider.GetRequiredService<ILoggerProvider>() as NLogLoggerProvider;
                    var nlogConfig = new Config.LoggingConfiguration(loggerProvider.LogFactory);
                    nlogConfig.AddRuleForAllLevels(configTarget);
                    loggerProvider.LogFactory.Configuration = nlogConfig;
                }
                else
                {
                    var loggerProvider = serviceProvider.GetRequiredService<ILoggerProvider>() as NLogLoggerProvider;
                    var nlogConfig = new Config.LoggingConfiguration(loggerProvider.LogFactory);
                    nlogConfig.AddRuleForAllLevels(configTarget);
                    loggerProvider.LogFactory.Configuration = nlogConfig;
                }
            }

            return serviceProvider;
        }

        protected T GetRunner<T>(NLogProviderOptions options = null, Targets.Target target = null) where T : class
        {
            // Start program
            return SetupServiceProvider(options, target, configureServices: (s) => s.AddTransient<T>()).GetRequiredService<T>();
        }
    }
}
