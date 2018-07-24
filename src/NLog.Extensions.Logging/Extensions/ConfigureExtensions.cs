using Microsoft.Extensions.Logging;
using NLog.Config;
using System;
using System.Reflection;

#if !NETCORE1_0
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#endif

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Helpers for .NET Core
    /// </summary>
    public static class ConfigureExtensions
    {
        /// <summary>
        /// Enable NLog as logging provider in .NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggerFactory AddNLog(this ILoggerFactory factory)
        {
            return AddNLog(factory, null);
        }

        /// <summary>
        /// Enable NLog as logging provider in .NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="options">NLog options</param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggerFactory AddNLog(this ILoggerFactory factory, NLogProviderOptions options)
        {
            using (var provider = new NLogLoggerProvider(options))
            {
                factory.AddProvider(provider);
            }
            return factory;
        }

#if !NETCORE1_0
        /// <summary>
        /// Enable NLog as logging provider in .NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder factory)
        {
            return AddNLog(factory, null);
        }

        /// <summary>
        /// Enable NLog as logging provider in .NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="options">NLog options</param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder factory, NLogProviderOptions options)
        {
            using (var provider = new NLogLoggerProvider(options))
            {
                factory.AddProvider(provider);
            }
            return factory;
        }

        /// <summary>
        /// Enable and configure NLog as a logging provider for buildable generic host (.NET Core 2.1+).
        /// Can be used in discrete containers as well. 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>IHostBuilder for chaining</returns>
        public static IHostBuilder UseNLog(this IHostBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.UseNLog(new NLogProviderOptions
            {
                CaptureMessageTemplates = true, 
                CaptureMessageProperties = true
            });
        }

        /// <summary>
        /// Enable and configure NLog as a logging provider for buildable generic host (.NET Core 2.1+).
        /// Can be used in discrete containers as well. 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns>IHostBuilder for chaining</returns>
        public static IHostBuilder UseNLog(this IHostBuilder builder, NLogProviderOptions options)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(services =>
            {
                ConfigurationItemFactory.Default.RegisterItemsFromAssembly(
                    typeof(ConfigureExtensions).GetTypeInfo().Assembly);

                LogManager.AddHiddenAssembly(typeof(ConfigureExtensions).GetTypeInfo().Assembly);

                using (var factory = new LoggerFactory())
                {
                    services.AddSingleton(factory.AddNLog(options));
                }
            });

            return builder;
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
    }
}
