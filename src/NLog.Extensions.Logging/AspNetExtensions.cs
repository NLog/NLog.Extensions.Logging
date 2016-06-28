using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Config;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Helpers for ASP.NET Core
    /// </summary>
    public static class AspNetExtensions
    {

        /// <summary>
        /// Enable NLog as logging provider in ASP.NET Core.
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static ILoggerFactory AddNLog(this ILoggerFactory factory)
        {
            //ignore this
            LogManager.AddHiddenAssembly(typeof(AspNetExtensions).GetTypeInfo().Assembly);

            using (var provider = new NLogLoggerProvider())
            {
                factory.AddProvider(provider);
            }
            return factory;
        }

        /// <summary>
        /// Apply NLog configuration from XML config.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configFileRelativePath">relative path to NLog configuration file.</param>
        /// <returns>Current configuration for chaining.</returns>
        public static LoggingConfiguration ConfigureNLog(this IHostingEnvironment env, string configFileRelativePath)
        {
            var fileName = Path.Combine(Directory.GetParent(env.WebRootPath).FullName, configFileRelativePath);
            return ConfigureNLog(fileName);
        }

        /// <summary>
        /// Apply NLog configuration from config object.
        /// </summary>
        /// <param name="env"></param>
        /// <param name="config">New NLog config.</param>
        /// <returns>Current configuration for chaining.</returns>
        public static LoggingConfiguration ConfigureNLog(this IHostingEnvironment env, LoggingConfiguration config)
        {
            LogManager.Configuration = config;

            return config;
        }

        /// <summary>
        /// Start NLog configuration.
        /// </summary>
        /// <param name="env"></param>
        /// <returns>Current configuration for chaining.</returns>
        public static LoggingConfiguration ConfigureNLog(this IHostingEnvironment env)
        {
            return LogManager.Configuration;
        }

        /// <summary>
        /// Apply NLog configuration from XML config.
        /// </summary>
        /// <param name="fileName">absolute path  NLog configuration file.</param>
        private static LoggingConfiguration ConfigureNLog(string fileName)
        {
            var configuration = new XmlLoggingConfiguration(fileName, true);
            LogManager.Configuration = configuration;
            return configuration;
        }
    }
}
