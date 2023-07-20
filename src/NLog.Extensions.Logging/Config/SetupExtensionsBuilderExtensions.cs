using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using NLog.Common;
using NLog.Config;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Extension methods to setup NLog extensions, so they are known when loading NLog LoggingConfiguration
    /// </summary>
    public static class SetupExtensionsBuilderExtensions
    {
        /// <summary>
        /// Setup the MEL-configuration for the ${configsetting} layoutrenderer
        /// </summary>
        public static ISetupExtensionsBuilder RegisterConfigSettings(this ISetupExtensionsBuilder setupBuilder, IConfiguration configuration)
        {
            RegisterHiddenAssembliesForCallSite();
            ConfigSettingLayoutRenderer.DefaultConfiguration = configuration ?? ConfigSettingLayoutRenderer.DefaultConfiguration;
            return setupBuilder.RegisterLayoutRenderer<ConfigSettingLayoutRenderer>("configsetting")
                               .RegisterLayoutRenderer<MicrosoftConsoleLayoutRenderer>("MicrosoftConsoleLayout")
                               .RegisterLayout<MicrosoftConsoleJsonLayout>("MicrosoftConsoleJsonLayout")
                               .RegisterTarget<MicrosoftILoggerTarget>("MicrosoftILogger");
        }

        internal static void RegisterHiddenAssembliesForCallSite()
        {
            InternalLogger.Debug("Hide assemblies for callsite");
            LogManager.AddHiddenAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
            LogManager.AddHiddenAssembly(typeof(Microsoft.Extensions.Logging.ILogger).GetTypeInfo().Assembly);
#if !NETCORE1_0
            LogManager.AddHiddenAssembly(typeof(Microsoft.Extensions.Logging.LoggerFactory).GetTypeInfo().Assembly);
#else
            SafeAddHiddenAssembly("Microsoft.Logging");
            SafeAddHiddenAssembly("Microsoft.Extensions.Logging");

            //try the Filter ext, this one is not mandatory so could fail
            SafeAddHiddenAssembly("Microsoft.Extensions.Logging.Filter", false);
#endif
        }

#if NETCORE1_0
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
                    InternalLogger.Debug(ex, "Hiding assembly {0} failed. This could influence the ${{callsite}}", assemblyName);
                }
            }
        }
#endif
    }
}
