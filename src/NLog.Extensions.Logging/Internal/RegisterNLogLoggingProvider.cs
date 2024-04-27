namespace NLog.Extensions.Logging
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
#if !NETCORE1_0
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
#endif

    internal static class RegisterNLogLoggingProvider
    {
#if !NETCORE1_0
        internal static void TryAddNLogLoggingProvider(this IServiceCollection services, Action<IServiceCollection, Action<ILoggingBuilder>> addLogging, IConfiguration hostConfiguration, NLogProviderOptions options, Func<IServiceProvider, IConfiguration, NLogProviderOptions, NLogLoggerProvider> factory)
        {
            var sharedFactory = factory;

            options = options.Configure(hostConfiguration?.GetSection("Logging:NLog"));

            if (options.ReplaceLoggerFactory)
            {
                NLogLoggerProvider singleInstance = null;   // Ensure that registration of ILoggerFactory and ILoggerProvider shares the same single instance
                sharedFactory = (provider, cfg, opt) => singleInstance ?? (singleInstance = factory(provider, cfg, opt));

                addLogging?.Invoke(services, (builder) => builder?.ClearProviders());  // Cleanup the existing LoggerFactory, before replacing it with NLogLoggerFactory
                services.Replace(ServiceDescriptor.Singleton<ILoggerFactory, NLogLoggerFactory>(serviceProvider => new NLogLoggerFactory(sharedFactory(serviceProvider, hostConfiguration, options))));
            }

            services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, NLogLoggerProvider>(serviceProvider => sharedFactory(serviceProvider, hostConfiguration, options)));

            if (options.RemoveLoggerFactoryFilter)
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
                        configBuilder.Configuration = new NLogLoggingConfiguration(nlogConfig, loggerProvider.LogFactory);
                    }
                });
            }
            else
            {
                Common.InternalLogger.Debug("Skip loading NLogLoggingConfiguration from empty config section: {0}", loggerProvider.Options.LoggingConfigurationSectionName);
            }
        }

        internal static NLogLoggerProvider CreateNLogLoggerProvider(this IServiceProvider serviceProvider, IConfiguration hostConfiguration, NLogProviderOptions options, LogFactory logFactory)
        {
            var provider = new NLogLoggerProvider(options, logFactory);

            var configuration = serviceProvider.SetupNLogConfigSettings(hostConfiguration, provider.LogFactory);

            if (configuration != null && (!ReferenceEquals(configuration, hostConfiguration) || options is null))
            {
                provider.Configure(configuration.GetSection("Logging:NLog"));
            }

            if (serviceProvider != null && provider.Options.RegisterServiceProvider)
            {
                provider.LogFactory.ServiceRepository.RegisterService(typeof(IServiceProvider), serviceProvider);
            }

            if (configuration != null)
            {
                provider.TryLoadConfigurationFromSection(configuration);
            }

            if (provider.Options.ShutdownOnDispose || !provider.Options.AutoShutdown)
            {
                provider.LogFactory.AutoShutdown = false;
            }

            return provider;
        }

        internal static IConfiguration SetupNLogConfigSettings(this IServiceProvider serviceProvider, IConfiguration configuration, LogFactory logFactory)
        {
            configuration = configuration ?? (serviceProvider?.GetService(typeof(IConfiguration)) as IConfiguration);
            logFactory.Setup()
                .SetupExtensions(ext => ext.RegisterConfigSettings(configuration))
                .SetupLogFactory(ext =>
                {
                    ext.AddCallSiteHiddenAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
                    ext.AddCallSiteHiddenAssembly(typeof(Microsoft.Extensions.Logging.ILogger).GetTypeInfo().Assembly);
#if !NETCORE1_0
                    ext.AddCallSiteHiddenAssembly(typeof(Microsoft.Extensions.Logging.LoggerFactory).GetTypeInfo().Assembly);
#else
                    var loggingAssembly = SafeLoadHiddenAssembly("Microsoft.Logging");
                    ext.AddCallSiteHiddenAssembly(loggingAssembly ?? typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
                    var extensionAssembly = SafeLoadHiddenAssembly("Microsoft.Extensions.Logging");
                    ext.AddCallSiteHiddenAssembly(extensionAssembly ?? typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
                    var filterAssembly = SafeLoadHiddenAssembly("Microsoft.Extensions.Logging.Filter", false);
                    ext.AddCallSiteHiddenAssembly(filterAssembly ?? typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
#endif
                });
            return configuration;
        }

#if NETCORE1_0
        private static Assembly SafeLoadHiddenAssembly(string assemblyName, bool logOnException = true)
        {
            try
            {
                Common.InternalLogger.Debug("Loading Assembly {0} to mark it as hidden for callsite", assemblyName);
                return Assembly.Load(new AssemblyName(assemblyName));
            }
            catch (Exception ex)
            {
                if (logOnException)
                {
                    Common.InternalLogger.Debug(ex, "Failed loading Loading Assembly {0} to mark it as hidden for callsite", assemblyName);
                }
                return null;
            }
        }
#endif
    }
}
