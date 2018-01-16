using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NLog.Common;
using NLog.Config;

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
            ConfigureHiddenAssemblies();

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
            ConfigureHiddenAssemblies();

            using (var provider = new NLogLoggerProvider(options))
            {
                factory.AddProvider(provider);
            }
            return factory;
        }
#endif

        /// <summary>
        /// Ignore assemblies for ${callsite}
        /// </summary>
        private static void ConfigureHiddenAssemblies()
        {

            InternalLogger.Trace("Hide assemblies for callsite");

#if NETCORE1_0
            SafeAddHiddenAssembly("Microsoft.Logging");
#endif

            SafeAddHiddenAssembly("Microsoft.Extensions.Logging");
            SafeAddHiddenAssembly("Microsoft.Extensions.Logging.Abstractions");
            SafeAddHiddenAssembly("NLog.Extensions.Logging");

            //try the Filter ext, this one is not mandatory so could fail
            SafeAddHiddenAssembly("Microsoft.Extensions.Logging.Filter", false);

        }

        private static void SafeAddHiddenAssembly(string assemblyName, bool logOnException = true)
        {
            try
            {
                InternalLogger.Trace("Hide {0}", assemblyName);
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
                LogManager.AddHiddenAssembly(assembly);
            }
            catch (Exception ex)
            {
                if (logOnException)
                {
                    InternalLogger.Debug(ex, "Hiding assembly {0} failed. This could influence the ${callsite}", assemblyName);
                }
            }
        }

        /// <summary>
        /// Apply NLog configuration from XML config.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="configFileRelativePath">relative path to NLog configuration file.</param>
        /// <returns>Current configuration for chaining.</returns>
        public static LoggingConfiguration ConfigureNLog(this ILoggerFactory loggerFactory, string configFileRelativePath)
        {
#if NETCORE1_0 && !NET451
            var rootPath = System.AppContext.BaseDirectory;
#else
            var rootPath = AppDomain.CurrentDomain.BaseDirectory;
#endif

            var fileName = Path.Combine(rootPath, configFileRelativePath);
            return ConfigureNLog(fileName);
        }

        /// <summary>
        /// Apply NLog configuration from config object.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="config">New NLog config.</param>
        /// <returns>Current configuration for chaining.</returns>
        public static LoggingConfiguration ConfigureNLog(this ILoggerFactory loggerFactory, LoggingConfiguration config)
        {
            LogManager.Configuration = config;

            return config;
        }

#if !NETCORE1_0
        /// <summary>
        /// Apply NLog configuration from XML config.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="configFileRelativePath">relative path to NLog configuration file.</param>
        /// <returns>Current configuration for chaining.</returns>
        public static LoggingConfiguration ConfigureNLog(this ILoggingBuilder loggerFactory, string configFileRelativePath)
        {
#if NETCORE1_0 && !NET451
            var rootPath = System.AppContext.BaseDirectory;
#else
            var rootPath = AppDomain.CurrentDomain.BaseDirectory;
#endif

            var fileName = Path.Combine(rootPath, configFileRelativePath);
            return ConfigureNLog(fileName);
        }

        /// <summary>
        /// Apply NLog configuration from config object.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="config">New NLog config.</param>
        /// <returns>Current configuration for chaining.</returns>
        public static LoggingConfiguration ConfigureNLog(this ILoggingBuilder loggerFactory, LoggingConfiguration config)
        {
            LogManager.Configuration = config;

            return config;
        }
#endif

        /// <summary>
        /// Apply NLog configuration from XML config.
        /// </summary>
        /// <param name="fileName">absolute path  NLog configuration file.</param>
        /// <returns>Current configuration for chaining.</returns>
        private static LoggingConfiguration ConfigureNLog(string fileName)
        {
            var config = new XmlLoggingConfiguration(fileName, true);
            LogManager.Configuration = config;
            return config;
        }
    }
}
