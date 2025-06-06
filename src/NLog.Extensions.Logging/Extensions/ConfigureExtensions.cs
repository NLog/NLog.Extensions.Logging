using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        /// <param name="builder"></param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder)
        {
            return builder.AddNLog(NLogProviderOptions.Default);
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">Override configuration and not use default Host Builder Configuration</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, IConfiguration configuration)
        {
            return AddNLog(builder, configuration, NLogProviderOptions.Default);
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">Override configuration and not use default Host Builder Configuration</param>
        /// <param name="options">NLog Logging Provider options</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, IConfiguration configuration, NLogProviderOptions options)
        {
            Guard.ThrowIfNull(builder);
            AddNLogLoggerProvider(builder, configuration, options, CreateNLogLoggerProvider);
            return builder;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">NLog Logging Provider options</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, NLogProviderOptions options)
        {
            Guard.ThrowIfNull(builder);
            AddNLogLoggerProvider(builder, null, options, CreateNLogLoggerProvider);
            return builder;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging, and loads the new NLog <paramref name="configuration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">New NLog config to be loaded.</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, LoggingConfiguration configuration)
        {
            return AddNLog(builder, configuration, NLogProviderOptions.Default);
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging, and loads the new NLog <paramref name="configuration"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">New NLog config to be loaded.</param>
        /// <param name="options">NLog Logging Provider options</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, LoggingConfiguration configuration, NLogProviderOptions options)
        {
            Guard.ThrowIfNull(builder);
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
        /// Enable NLog as logging provider for Microsoft Extension Logging, and loads NLog config from <paramref name="configFileRelativePath"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configFileRelativePath">relative file-path to NLog configuration file.</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, string configFileRelativePath)
        {
            Guard.ThrowIfNull(builder);
            AddNLogLoggerProvider(builder, null, NLogProviderOptions.Default, (serviceProvider, config, options) =>
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
        /// <param name="factoryBuilder">Initialize NLog LogFactory with NLog LoggingConfiguration.</param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, Func<IServiceProvider, LogFactory> factoryBuilder)
        {
            return AddNLog(builder, NLogProviderOptions.Default, factoryBuilder);
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
            Guard.ThrowIfNull(builder);
            Guard.ThrowIfNull(factoryBuilder);
            AddNLogLoggerProvider(builder, null, options, (serviceProvider, config, options) =>
            {
                config = serviceProvider.SetupNLogConfigSettings(config, LogManager.LogFactory);

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
        /// <param name="collection"></param>
        /// <returns>IServiceCollection for chaining</returns>
        public static IServiceCollection AddNLog(this IServiceCollection collection)
        {
            return AddNLog(collection, NLogProviderOptions.Default);
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="options">NLog Logging Provider options</param>
        /// <returns>IServiceCollection for chaining</returns>
        public static IServiceCollection AddNLog(this IServiceCollection collection, NLogProviderOptions options)
        {
            Guard.ThrowIfNull(collection);
            collection.TryAddNLogLoggingProvider((svc, addlogging) => svc.AddLogging(addlogging), null, options, CreateNLogLoggerProvider);
            return collection;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="options">NLog Logging Provider options</param>
        /// <param name="factoryBuilder">Initialize NLog LogFactory with NLog LoggingConfiguration.</param>
        /// <returns>IServiceCollection for chaining</returns>
        public static IServiceCollection AddNLog(this IServiceCollection collection, NLogProviderOptions options, Func<IServiceProvider, LogFactory> factoryBuilder)
        {
            Guard.ThrowIfNull(collection);
            Guard.ThrowIfNull(factoryBuilder);
            collection.TryAddNLogLoggingProvider((svc, addlogging) => svc.AddLogging(addlogging), null, options, (serviceProvider, config, opt) =>
            {
                config = serviceProvider.SetupNLogConfigSettings(config, LogManager.LogFactory);

                // Delay initialization of targets until we have loaded config-settings
                var logFactory = factoryBuilder(serviceProvider);
                var provider = CreateNLogLoggerProvider(serviceProvider, config, opt, logFactory);
                return provider;
            });
            return collection;
        }

        private static void AddNLogLoggerProvider(ILoggingBuilder builder, IConfiguration? hostConfiguration, NLogProviderOptions options, Func<IServiceProvider, IConfiguration?, NLogProviderOptions, NLogLoggerProvider> factory)
        {
            builder.Services.TryAddNLogLoggingProvider((svc, addlogging) => addlogging(builder), hostConfiguration, options, factory);
        }

        /// <summary>
        /// Factory method for <see cref="NLogLoggerProvider"/>
        /// </summary>
        /// <param name="nlogProvider"></param>
        /// <param name="configurationSection">Microsoft Extension Configuration</param>
        /// <returns></returns>
        public static NLogLoggerProvider? Configure(this NLogLoggerProvider? nlogProvider, IConfigurationSection configurationSection)
        {
            if (nlogProvider is null || configurationSection is null)
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
        public static NLogProviderOptions Configure(this NLogProviderOptions? options, IConfigurationSection? configurationSection)
        {
            options = options ?? NLogProviderOptions.Default;

            if (configurationSection is null || !(configurationSection.GetChildren()?.Any() ?? false))
                return options;

            var configProps = typeof(NLogProviderOptions).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.SetMethod?.IsPublic == true).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var configValue in configurationSection.GetChildren())
            {
                if (configProps.TryGetValue(configValue.Key, out var propertyInfo))
                {
                    try
                    {
                        var result = Convert.ChangeType(configValue.Value, propertyInfo.PropertyType, System.Globalization.CultureInfo.InvariantCulture);
                        propertyInfo.SetMethod?.Invoke(options, new[] { result });
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Warn(ex, "NLogProviderOptions: Property {0} could not be assigned value: {1}", configValue.Key, configValue.Value);
                    }
                }
            }
            return options;
        }

        private static NLogLoggerProvider CreateNLogLoggerProvider(IServiceProvider serviceProvider, IConfiguration? hostConfiguration, NLogProviderOptions options)
        {
            return CreateNLogLoggerProvider(serviceProvider, hostConfiguration, options, LogManager.LogFactory);
        }

        private static NLogLoggerProvider CreateNLogLoggerProvider(IServiceProvider serviceProvider, IConfiguration? hostConfiguration, NLogProviderOptions options, LogFactory logFactory)
        {
            return serviceProvider.CreateNLogLoggerProvider(hostConfiguration, options, logFactory);
        }
    }
}
