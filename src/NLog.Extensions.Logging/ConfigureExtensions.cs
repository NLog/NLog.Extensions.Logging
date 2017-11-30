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

        private static void ConfigureHiddenAssemblies()
        {
            try
            {
                //ignore these assemblies for ${callsite}
#if !NETCORE1_0
                LogManager.AddHiddenAssembly(typeof(Microsoft.Extensions.Logging.ILoggingBuilder).GetTypeInfo().Assembly); //Microsoft.Logging
#endif
                LogManager.AddHiddenAssembly(typeof(Microsoft.Extensions.Logging.LoggerFactoryExtensions).GetTypeInfo().Assembly); //Microsoft.Extensions.Logging
                LogManager.AddHiddenAssembly(typeof(Microsoft.Extensions.Logging.ILogger).GetTypeInfo().Assembly); // Microsoft.Extensions.Logging.Abstractions
                LogManager.AddHiddenAssembly(typeof(NLog.Extensions.Logging.ConfigureExtensions).GetTypeInfo().Assembly); //NLog.Extensions.Logging

                try
                {
                    //try the Filter ext
                    var filterAssembly = Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging.Filter"));
                    LogManager.AddHiddenAssembly(filterAssembly);

                }
                catch (Exception ex)
                {
                    InternalLogger.Trace(ex, "filtering Microsoft.Extensions.Logging.Filter failed. Not an issue probably");
                }
                
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, "failure in ignoring assemblies. This could influence the ${callsite}");
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
