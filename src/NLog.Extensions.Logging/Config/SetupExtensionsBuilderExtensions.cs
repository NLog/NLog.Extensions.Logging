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
        /// Replace with version from NLog.Extension.Logging when it has been released with NLog 4.7
        /// </summary>
        internal static ISetupExtensionsBuilder RegisterConfigSettings(this ISetupExtensionsBuilder setupBuilder, IConfiguration configuration)
        {
            ConfigSettingLayoutRenderer.DefaultConfiguration = configuration;
            ConfigurationItemFactory.Default.RegisterType(typeof(ConfigSettingLayoutRenderer), string.Empty);
            return setupBuilder;
        }
    }
}
