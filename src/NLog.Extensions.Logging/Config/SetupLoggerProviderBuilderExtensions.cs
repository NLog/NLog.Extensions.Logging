using System.Reflection;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Extension methods to setup NLog together with Microsoft Extension Logging
    /// </summary>
    public static class SetupLoggerProviderBuilderExtensions
    {
        /// <summary>
        /// Enables lookup of settings from appsettings.json using ${configsetting}
        /// </summary>
        public static ISetupExtensionLoggingBuilder SetupConfigSettings(this ISetupExtensionLoggingBuilder setupBuilder, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            ConfigSettingLayoutRenderer.DefaultConfiguration = configuration;
            NLog.Config.ConfigurationItemFactory.Default.RegisterType(typeof(ConfigSettingLayoutRenderer), string.Empty);
            return setupBuilder;
        }

        /// <summary>
        /// Loads NLog LoggingConfiguration from appsettings.json from NLog-section
        /// </summary>
        public static ISetupExtensionLoggingBuilder LoadNLogLoggingConfiguration(this ISetupExtensionLoggingBuilder setupBuilder, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            SetupConfigSettings(setupBuilder, configuration);
            setupBuilder.LogFactory.Configuration = new NLogLoggingConfiguration(configuration.GetSection("NLog"), setupBuilder.LogFactory);
            return setupBuilder;
        }
    }
}
