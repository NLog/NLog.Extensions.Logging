namespace NLog.Extensions.Logging
{
#if !NETCORE1_0
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;

    internal static class RegisterNLogLoggingProvider
    {
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
    }
#endif
}
