using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
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
        /// <param name="builder"></param>
        /// <returns>ILoggingBuilder for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder)
        {
            return AddNLog(builder, null);
        }

        /// <summary>
        /// Enable NLog as logging provider in .NET Core.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">NLog options</param>
        /// <returns>ILoggerFactory for chaining</returns>
        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, NLogProviderOptions options)
        {
            //ignore this
            LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging")));
            LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging.Abstractions")));

            try
            {
                //try the Filter ext
                var filterAssembly = Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging.Filter"));
                LogManager.AddHiddenAssembly(filterAssembly);
            }
            catch (Exception)
            {
                //ignore
            }
          
            LogManager.AddHiddenAssembly(typeof(ConfigureExtensions).GetTypeInfo().Assembly);

            using (var provider = new NLogLoggerProvider(options))
            {
                builder.AddProvider(provider);
            }
            return builder;
        }

        /// <summary>
        /// Apply NLog configuration from XML config.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configFileRelativePath">relative path to NLog configuration file.</param>
          /// <returns>Current configuration for chaining.</returns>
        public static LoggingConfiguration ConfigureNLog(this ILoggerFactory env, string configFileRelativePath)
        {
#if NETCORE
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
       /// <param name="env"></param>
       /// <param name="config">New NLog config.</param>
       /// <returns>Current configuration for chaining.</returns>
       public static LoggingConfiguration ConfigureNLog(this ILoggerFactory env, LoggingConfiguration config)
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
