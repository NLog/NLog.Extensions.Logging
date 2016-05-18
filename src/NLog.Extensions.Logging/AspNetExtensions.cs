﻿using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using NLog.Config;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Helpers for ASP.NET
    /// </summary>
    public static class AspNetExtensions
    {

        /// <summary>
        /// Enable NLog as logging provider in ASP.NET 5.
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
        public static void ConfigureNLog(this IHostingEnvironment env, string configFileRelativePath)
        {
            var fileName = Path.Combine(Directory.GetParent(env.WebRootPath).FullName, configFileRelativePath);
            ConfigureNLog(fileName);
        }

        /// <summary>
        /// Apply NLog configuration from XML config.
        /// </summary>
        /// <param name="fileName">absolute path  NLog configuration file.</param>
        private static void ConfigureNLog(string fileName)
        {
            LogManager.Configuration = new XmlLoggingConfiguration(fileName, true);
        }
    }
}
