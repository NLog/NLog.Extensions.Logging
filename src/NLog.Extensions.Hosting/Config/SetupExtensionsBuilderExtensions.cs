using Microsoft.Extensions.Configuration;
using NLog.Config;
using NLog.Extensions.Logging;

namespace NLog.Extensions.Hosting
{
    /// <summary>
    /// Extension methods to setup NLog extensions, so they are known when loading NLog LoggingConfiguration
    /// </summary>
    public static class SetupExtensionsBuilderExtensions
    {
        /// <summary>
        /// Setup the MEL-configuration for the ${configsetting} layoutrenderer
        /// </summary>
        public static ISetupExtensionsBuilder RegisterHostSettings(this ISetupExtensionsBuilder setupBuilder, IConfiguration? configuration)
        {
            setupBuilder.RegisterConfigSettings(configuration);
            return setupBuilder.RegisterLayoutRenderer<HostAppNameLayoutRenderer>("host-appname")
                        .RegisterLayoutRenderer<HostRootDirLayoutRenderer>("host-rootdir")
                        .RegisterLayoutRenderer<HostEnvironmentLayoutRenderer>("host-environment");
        }
    }
}
