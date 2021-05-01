namespace NLog.Extensions.Logging
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
#if !NETCORE1_0
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
#endif

    internal static class RegisterNLogLoggingProvider
    {
#if !NETCORE1_0
        internal static void TryAddNLogLoggingProvider(this IServiceCollection services, Action<IServiceCollection, Action<ILoggingBuilder>> addLogging, IConfiguration configuration, NLogProviderOptions options, Func<IServiceProvider, IConfiguration, NLogProviderOptions, NLogLoggerProvider> factory)
        {
            var sharedFactory = factory;

            if (options?.ReplaceLoggerFactory == true)
            {
                NLogLoggerProvider singleInstance = null;   // Ensure that registration of ILoggerFactory and ILoggerProvider shares the same single instance
                sharedFactory = (provider, cfg, opt) => singleInstance ?? (singleInstance = factory(provider, cfg, opt));

                addLogging?.Invoke(services, (builder) => builder?.ClearProviders());  // Cleanup the existing LoggerFactory, before replacing it with NLogLoggerFactory
                services.Replace(ServiceDescriptor.Singleton<ILoggerFactory, NLogLoggerFactory>(serviceProvider => new NLogLoggerFactory(sharedFactory(serviceProvider, configuration, options))));
            }

            services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, NLogLoggerProvider>(serviceProvider => sharedFactory(serviceProvider, configuration, options)));

            if (options?.RemoveLoggerFactoryFilter == true)
            {
                // Will forward all messages to NLog if not specifically overridden by user
                addLogging?.Invoke(services, (builder) => builder?.AddFilter<NLogLoggerProvider>(null, Microsoft.Extensions.Logging.LogLevel.Trace));
            }
        }
#endif

        internal static void TryLoadConfigurationFromSection(this NLogLoggerProvider loggerProvider, IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(loggerProvider.Options.LoggingConfigurationSectionName))
                return;

            var nlogConfig = configuration.GetSection(loggerProvider.Options.LoggingConfigurationSectionName);
            if (nlogConfig?.GetChildren()?.Any() == true)
            {
                loggerProvider.LogFactory.Setup().LoadConfiguration(configBuilder =>
                {
                    if (configBuilder.Configuration.LoggingRules.Count == 0 && configBuilder.Configuration.AllTargets.Count == 0)
                    {
                        configBuilder.Configuration = new NLogLoggingConfiguration(nlogConfig);
                    }
                });
            }
        }
    }
}
