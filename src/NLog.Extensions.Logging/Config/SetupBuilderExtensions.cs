using System;
using NLog.Config;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Extension methods to setup LogFactory options
    /// </summary>
    public static class SetupBuilderExtensions
    {
        /// <summary>
        /// Setup of LogFactory integration with Microsoft Extension Logging
        /// </summary>
        public static ISetupBuilder SetupExtensionLogging(this ISetupBuilder setupBuilder, Action<ISetupExtensionLoggingBuilder> extensionsBuilder)
        {
            extensionsBuilder(new SetupExtensionLoggingBuilder(setupBuilder.LogFactory));
            return setupBuilder;
        }

        private class SetupExtensionLoggingBuilder : ISetupExtensionLoggingBuilder
        {
            public SetupExtensionLoggingBuilder(LogFactory logFactory)
            {
                LogFactory = logFactory;
            }

            public LogFactory LogFactory { get; }
        }
    }
}
