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
            ConfigSettingLayoutRenderer.DefaultConfiguration = configuration ?? ConfigSettingLayoutRenderer.DefaultConfiguration;
            setupBuilder.LogFactory.Setup().SetupLogFactory(ext =>
            {
                ext.AddCallSiteHiddenAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
                ext.AddCallSiteHiddenAssembly(typeof(Microsoft.Extensions.Logging.ILogger).GetTypeInfo().Assembly);
#if !NETCORE1_0
                ext.AddCallSiteHiddenAssembly(typeof(Microsoft.Extensions.Logging.LoggerFactory).GetTypeInfo().Assembly);
#else
                var loggingAssembly = SafeLoadHiddenAssembly("Microsoft.Logging");
                ext.AddCallSiteHiddenAssembly(loggingAssembly ?? typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
                var extensionAssembly = SafeLoadHiddenAssembly("Microsoft.Extensions.Logging");
                ext.AddCallSiteHiddenAssembly(extensionAssembly ?? typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
                var filterAssembly = SafeLoadHiddenAssembly("Microsoft.Extensions.Logging.Filter", false);
                ext.AddCallSiteHiddenAssembly(filterAssembly ?? typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
#endif
            });
            return setupBuilder.RegisterLayoutRenderer<ConfigSettingLayoutRenderer>("configsetting")
                               .RegisterLayoutRenderer<MicrosoftConsoleLayoutRenderer>("MicrosoftConsoleLayout")
                               .RegisterLayout<MicrosoftConsoleJsonLayout>("MicrosoftConsoleJsonLayout")
                               .RegisterTarget<MicrosoftILoggerTarget>("MicrosoftILogger");
        }

#if NETCORE1_0
        private static Assembly SafeLoadHiddenAssembly(string assemblyName, bool logOnException = true)
        {
            try
            {
                InternalLogger.Debug("Loading Assembly {0} to mark it as hidden for callsite", assemblyName);
                return Assembly.Load(new AssemblyName(assemblyName));
            }
            catch (Exception ex)
            {
                if (logOnException)
                {
                    InternalLogger.Debug(ex, "Failed loading Loading Assembly {0} to mark it as hidden for callsite", assemblyName);
                }
                return null;
            }
        }
#endif
    }
}
