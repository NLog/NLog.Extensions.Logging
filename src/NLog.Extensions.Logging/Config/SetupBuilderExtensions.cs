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
        public static ISetupBuilder LoadNLogConfigFromSection(this ISetupBuilder setupBuilder, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            setupBuilder.SetupExtensions(s => s.RegisterConfigSettings(configuration));
            var nlogConfig = configuration.GetSection("NLog");
            if (nlogConfig != null && nlogConfig.GetChildren().Any())
            {
                setupBuilder.LogFactory.Configuration = new NLogLoggingConfiguration(nlogConfig, setupBuilder.LogFactory);
            }
            return setupBuilder;
        }
    }
}
