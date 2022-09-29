using Microsoft.Extensions.Configuration;
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
            ConfigSettingLayoutRenderer.DefaultConfiguration = configuration;
            return setupBuilder.RegisterExtensionsLogging(configuration);
        }

        internal static ISetupExtensionsBuilder RegisterExtensionsLogging(this ISetupExtensionsBuilder setupBuilder, IConfiguration configuration)
        {
            if (ConfigSettingLayoutRenderer.DefaultConfiguration is null)
            {
                ConfigSettingLayoutRenderer.DefaultConfiguration = configuration;
            }
            return setupBuilder.RegisterLayoutRenderer<ConfigSettingLayoutRenderer>("configsetting").RegisterLayoutRenderer<MicrosoftConsoleLayoutRenderer>("MicrosoftConsoleLayout");
        }
    }
}
