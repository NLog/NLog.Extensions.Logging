using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using NLog.Config;

namespace NLog.Framework.Logging
{
    public static class AspNetExtensions
    {
        public static void ConfigureNLog(this IHostingEnvironment env, string configFileRelativePath)
        {
            var fileName = Path.Combine(Directory.GetParent(env.WebRootPath).ToString(), configFileRelativePath);
            ConfigureNLog(fileName);
        }

        public static void ConfigureNLog(this IApplicationEnvironment appEnv, string configFileRelativePath)
        {
            
            var fileName = Path.Combine(appEnv.ApplicationBasePath, configFileRelativePath);
            ConfigureNLog(fileName);
        }

        public static ILoggerFactory AddNLog(this ILoggerFactory factory)
        {
            //ignore this
            LogManager.AddHiddenAssembly(typeof(AspNetExtensions).Assembly);

            var provider = new NLogLoggerProvider();
            factory.AddProvider(provider);
            return factory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">absolute path</param>
        private static void ConfigureNLog(string fileName)
        {
            LogManager.Configuration = new XmlLoggingConfiguration(fileName, true);
        }
    }
}
