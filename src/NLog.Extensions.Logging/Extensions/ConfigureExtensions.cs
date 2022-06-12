using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
#if !NETCORE1_0
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
#endif
using Microsoft.Extensions.Logging;
using NLog.Common;
using NLog.Config;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Helpers for configuring NLog for Microsoft Extension Logging (MEL)
    /// </summary>
    public static class ConfigureExtensions
    {
        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="factory"></param>
        /// <returns>ILoggerFactory for chaining</returns>
#if !NETCORE1_0
        [Obsolete("Instead use ILoggingBuilder.AddNLog() or IHostBuilder.UseNLog()")]
#endif
        public static ILoggerFactory AddNLog(this ILoggerFactory factory)
        {
            return factory.AddNLog(NLogProviderOptions.Default);
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="options">NLog Logging Provider options</param>
        /// <returns>ILoggerFactory for chaining</returns>
#if !NETCORE1_0
        [Obsolete("Instead use ILoggingBuilder.AddNLog() or IHostBuilder.UseNLog()")]
#endif
        public static ILoggerFactory AddNLog(this ILoggerFactory factory, NLogProviderOptions options)
        {
            factory.AddProvider(new NLogLoggerProvider(options));
            return factory;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="configuration"></param>
        /// <returns>ILoggerFactory for chaining</returns>
#if !NETCORE1_0
        [Obsolete("Instead use ILoggingBuilder.AddNLog() or IHostBuilder.UseNLog()")]
#endif
        public static ILoggerFactory AddNLog(this ILoggerFactory factory, IConfiguration configuration)
        {
            var provider = CreateNLogLoggerProvider(null, configuration, null);
            factory.AddProvider(provider);
            return factory;
        }

#if !NETCORE1_0
        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="factory"></param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder factory)
        {
            return factory.AddNLog((NLogProviderOptions)null);
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="configuration">Configuration</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder factory, IConfiguration configuration)
        {
            AddNLogLoggerProvider(factory, configuration, null, CreateNLogLoggerProvider);
            return factory;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="configuration">Configuration</param>
        /// <param name="options">NLog Logging Provider options</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder factory, IConfiguration configuration, NLogProviderOptions options)
        {
            AddNLogLoggerProvider(factory, configuration, options, CreateNLogLoggerProvider);
            return factory;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="options">NLog Logging Provider options</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder factory, NLogProviderOptions options)
        {
            AddNLogLoggerProvider(factory, null, options, CreateNLogLoggerProvider);
            return factory;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">New NLog config.</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, LoggingConfiguration configuration)
        {
            return AddNLog(builder, configuration, null);
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">New NLog config.</param>
        /// <param name="options">NLog Logging Provider options</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, LoggingConfiguration configuration, NLogProviderOptions options)
        {
            AddNLogLoggerProvider(builder, null, options, (serviceProvider, config, options) =>
            {
                var logFactory = configuration?.LogFactory ?? LogManager.LogFactory;
                var provider = CreateNLogLoggerProvider(serviceProvider, config, options, logFactory);
                // Delay initialization of targets until we have loaded config-settings
                logFactory.Configuration = configuration;
                return provider;
            });
            return builder;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configFileRelativePath">relative path to NLog configuration file.</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, string configFileRelativePath)
        {
            AddNLogLoggerProvider(builder, null, null, (serviceProvider, config, options) =>
            {
                var provider = CreateNLogLoggerProvider(serviceProvider, config, options);
                // Delay initialization of targets until we have loaded config-settings
                provider.LogFactory.Setup().LoadConfigurationFromFile(configFileRelativePath);
                return provider;
            });
            return builder;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">NLog Logging Provider options</param>
        /// <param name="factoryBuilder">Initialize NLog LogFactory with NLog LoggingConfiguration.</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, NLogProviderOptions options, Func<IServiceProvider, LogFactory> factoryBuilder)
        {
            AddNLogLoggerProvider(builder, null, options, (serviceProvider, config, options) =>
            {
                serviceProvider.SetupNLogConfigSettings(config);

                // Delay initialization of targets until we have loaded config-settings
                var logFactory = factoryBuilder(serviceProvider);
                var provider = CreateNLogLoggerProvider(serviceProvider, config, options, logFactory);
                return provider;
            });
            return builder;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="factoryBuilder">Initialize NLog LogFactory with NLog LoggingConfiguration.</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, Func<IServiceProvider, LogFactory> factoryBuilder)
        {
            AddNLogLoggerProvider(builder, null, null, (serviceProvider, config, options) =>
            {
                serviceProvider.SetupNLogConfigSettings(config);

                // Delay initialization of targets until we have loaded config-settings
                var logFactory = factoryBuilder(serviceProvider);
                var provider = CreateNLogLoggerProvider(serviceProvider, config, options, logFactory);
                return provider;
            });
            return builder;
        }

        private static void AddNLogLoggerProvider(ILoggingBuilder builder, IConfiguration hostConfiguration, NLogProviderOptions options, Func<IServiceProvider, IConfiguration, NLogProviderOptions, NLogLoggerProvider> factory)
        {
            builder.Services.TryAddNLogLoggingProvider((svc, addlogging) => addlogging(builder), hostConfiguration, options, factory);
        }
#endif

        /// <summary>
        /// Apply NLog configuration from XML config.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="configFileRelativePath">relative path to NLog configuration file.</param>
        /// <returns>Current configuration for chaining.</returns>
        [Obsolete("Instead use NLog.LogManager.Setup().LoadConfigurationFromFile()")]
        public static LoggingConfiguration ConfigureNLog(this ILoggerFactory loggerFactory, string configFileRelativePath)
        {
            LogManager.AddHiddenAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
            return LogManager.LoadConfiguration(configFileRelativePath).Configuration;
        }

        /// <summary>
        /// Apply NLog configuration from config object.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="config">New NLog config.</param>
        /// <returns>Current configuration for chaining.</returns>
        [Obsolete("Instead assign property NLog.LogManager.Configuration")]
        public static LoggingConfiguration ConfigureNLog(this ILoggerFactory loggerFactory, LoggingConfiguration config)
        {
            LogManager.AddHiddenAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
            LogManager.Configuration = config;
            return config;
        }

        /// <summary>
        /// Factory method for <see cref="NLogLoggerProvider"/>
        /// </summary>
        /// <param name="nlogProvider"></param>
        /// <param name="configurationSection">Microsoft Extension Configuration</param>
        /// <returns></returns>
        public static NLogLoggerProvider Configure(this NLogLoggerProvider nlogProvider, IConfigurationSection configurationSection)
        {
            if (nlogProvider == null || configurationSection == null)
                return nlogProvider;

            Configure(nlogProvider.Options, configurationSection);
            return nlogProvider;
        }

        /// <summary>
        /// Factory method for <see cref="NLogProviderOptions"/>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configurationSection">Microsoft Extension Configuration</param>
        /// <returns></returns>
        public static NLogProviderOptions Configure(this NLogProviderOptions options, IConfigurationSection configurationSection)
        {
            options = options ?? NLogProviderOptions.Default;

            if (configurationSection == null || !(configurationSection.GetChildren()?.Any() ?? false))
                return options;

            var configProps = options.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.SetMethod?.IsPublic == true).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var configValue in configurationSection.GetChildren())
            {
                if (configProps.TryGetValue(configValue.Key, out var propertyInfo))
                {
                    try
                    {
                        var result = Convert.ChangeType(configValue.Value, propertyInfo.PropertyType, System.Globalization.CultureInfo.InvariantCulture);
                        propertyInfo.SetMethod.Invoke(options, new[] { result });
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Warn(ex, "NLogProviderOptions: Property {0} could not be assigned value: {1}", configValue.Key, configValue.Value);
                    }
                }
            }
            return options;
        }

        private static NLogLoggerProvider CreateNLogLoggerProvider(IServiceProvider serviceProvider, IConfiguration hostConfiguration, NLogProviderOptions options)
        {
            return CreateNLogLoggerProvider(serviceProvider, hostConfiguration, options, null);
        }

        private static NLogLoggerProvider CreateNLogLoggerProvider(IServiceProvider serviceProvider, IConfiguration hostConfiguration, NLogProviderOptions options, LogFactory logFactory)
        {
            return serviceProvider.CreateNLogLoggerProvider(hostConfiguration, options, logFactory);
        }
    }
}
