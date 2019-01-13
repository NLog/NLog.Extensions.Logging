using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
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
        public static ILoggerFactory AddNLog(this ILoggerFactory factory)
        {
            return factory.AddNLog(NLogProviderOptions.Default);
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="options">NLog options</param>
        /// <returns>ILoggerFactory for chaining</returns>
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
        public static ILoggerFactory AddNLog(this ILoggerFactory factory, IConfiguration configuration)
        {
            var provider = CreateNLogProvider(configuration);
            factory.AddProvider(provider);
            return factory;
        }

#if !NETCORE1_0
        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="factory"></param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder factory)
        {
            return factory.AddNLog(NLogProviderOptions.Default);
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="configuration">Configuration</param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder factory, IConfiguration configuration)
        {
            var provider = CreateNLogProvider(configuration);
            factory.AddProvider(provider);
            return factory;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="options">NLog options</param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder factory, NLogProviderOptions options)
        {
            factory.AddProvider(new NLogLoggerProvider(options));
            return factory;
        }
#endif

        /// <summary>
        /// Apply NLog configuration from XML config.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="configFileRelativePath">relative path to NLog configuration file.</param>
        /// <returns>Current configuration for chaining.</returns>
        [Obsolete("Instead use NLog.LogManager.LoadConfiguration()")]
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
            if (configurationSection == null)
                return nlogProvider;

            var configProps = nlogProvider.Options.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.SetMethod?.IsPublic == true).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var configValue in configurationSection.GetChildren())
            {
                if (configProps.TryGetValue(configValue.Key, out var propertyInfo))
                {
                    try
                    {
                        var result = Convert.ChangeType(configValue.Value, propertyInfo.PropertyType);
                        propertyInfo.SetMethod.Invoke(nlogProvider.Options, new[] { result });
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Warn(ex, "NLogProviderOptions: Property {0} could not be assigned value: {1}", configValue.Key, configValue.Value);
                    }
                }
            }

            return nlogProvider;
        }

        private static NLogLoggerProvider CreateNLogProvider(IConfiguration configuration)
        {
            var provider = new NLogLoggerProvider(new NLogProviderOptions());
            if (configuration != null)
            {
                // TODO ConfigSettingLayoutRenderer.DefaultConfiguration = configuration;
                provider.Configure(configuration.GetSection("Logging:NLog"));
            }

            return provider;
        }
    }
}
