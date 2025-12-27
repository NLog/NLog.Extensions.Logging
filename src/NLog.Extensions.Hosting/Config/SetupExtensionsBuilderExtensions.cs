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

        /// <summary>
        /// Write to <see cref="NLog.Targets.FileTarget"/> using <see cref="MicrosoftConsoleJsonLayout"/>
        /// </summary>
        public static ISetupConfigurationTargetBuilder WriteToJsonFile(this ISetupConfigurationTargetBuilder configBuilder, NLog.Layouts.Layout fileName, bool includeScopes = false)
        {
            var jsonLayout = new MicrosoftConsoleJsonLayout();
            if (includeScopes)
                jsonLayout.IncludeScopes = includeScopes;
            return configBuilder.WriteToFile(fileName, jsonLayout);
        }

        /// <summary>
        /// Write to <see cref="NLog.Targets.ConsoleTarget"/> using <see cref="MicrosoftConsoleJsonLayout"/> (similar to Microsoft AddJsonConsole)
        /// </summary>
        public static ISetupConfigurationTargetBuilder WriteToJsonConsole(this ISetupConfigurationTargetBuilder configBuilder, bool includeScopes = false)
        {
            var jsonLayout = new MicrosoftConsoleJsonLayout();
            if (includeScopes)
                jsonLayout.IncludeScopes = includeScopes;
            return configBuilder.WriteToConsole(jsonLayout);
        }
    }
}
