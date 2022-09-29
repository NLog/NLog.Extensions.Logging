using System.Linq;
using NLog.Config;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Extension methods to setup LogFactory options
    /// </summary>
    public static class SetupBuilderExtensions
    {
        /// <summary>
        /// Loads NLog LoggingConfiguration from appsettings.json from NLog-section
        /// </summary>
        public static ISetupBuilder LoadConfigurationFromSection(this ISetupBuilder setupBuilder, Microsoft.Extensions.Configuration.IConfiguration configuration, string configSection = "NLog")
        {
            if (!string.IsNullOrEmpty(configSection))
            {
                var nlogConfig = configuration.GetSection(configSection);
                if (nlogConfig?.GetChildren()?.Any() == true)
                {
                    setupBuilder.LogFactory.Configuration = new NLogLoggingConfiguration(nlogConfig, setupBuilder.LogFactory);
                }
                else
                {
                    Common.InternalLogger.Debug("Skip loading NLogLoggingConfiguration from empty config section: {0}", configSection);
                }
            }
            return setupBuilder;
        }
    }
}
