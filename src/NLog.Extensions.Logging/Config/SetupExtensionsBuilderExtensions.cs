﻿using System;
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
            return setupBuilder.RegisterLayoutRenderer<ConfigSettingLayoutRenderer>("configsetting")
                               .RegisterLayoutRenderer<MicrosoftConsoleLayoutRenderer>("MicrosoftConsoleLayout")
                               .RegisterLayout<MicrosoftConsoleJsonLayout>("MicrosoftConsoleJsonLayout")
                               .RegisterTarget<MicrosoftILoggerTarget>("MicrosoftILogger");
        }
    }
}
