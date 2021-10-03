using System;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Options for logging to NLog with 
    /// </summary>
    public class NLogProviderOptions
    {
        /// <summary>
        /// Separator between for EventId.Id and EventId.Name. Default to _
        /// </summary>
        public string EventIdSeparator { get; set; } = "_";

        /// <summary>
        /// Skip creating "EventId_Id" and "EventId_Name" as <see cref="LogEventInfo.Properties" /> when <c>default(EventId)</c>
        /// </summary>
        public bool IgnoreEmptyEventId { get; set; } = true;

        /// <summary>
        /// Enable structured logging by capturing message template parameters with support for "@" and "$". Enables use of ${message:raw=true}
        /// </summary>
        public bool CaptureMessageTemplates { get; set; } = true;

        /// <summary>
        /// Enable capture of properties from the ILogger-State-object, both in <see cref="Microsoft.Extensions.Logging.ILogger.Log{TState}"/> and <see cref="Microsoft.Extensions.Logging.ILogger.BeginScope{TState}"/>
        /// </summary>
        public bool CaptureMessageProperties { get; set; } = true;

        /// <summary>
        /// Use the NLog engine for parsing the message template (again) and format using the NLog formatter
        /// </summary>
        public bool ParseMessageTemplates { get; set; }

        /// <summary>
        /// Enable capture of scope information and inject into <see cref="NLog.ScopeContext" />
        /// </summary>
        public bool IncludeScopes { get; set; } = true;

        /// <summary>
        /// Shutdown NLog on dispose of the <see cref="NLogLoggerProvider"/>
        /// </summary>
        public bool ShutdownOnDispose { get; set; }

#if NET5_0
        /// <summary>
        /// Automatically include <see cref="System.Diagnostics.Activity.SpanId"/>, <see cref="System.Diagnostics.Activity.TraceId"/> and <see cref="System.Diagnostics.Activity.ParentId"/>
        /// </summary>
        /// <remarks>
        /// Intended for Net5.0 where these properties are no longer included by default for performance reasons
        /// 
        /// Consider using <a href="https://www.nuget.org/packages/NLog.DiagnosticSource/">${activity}</a> as alternative
        /// </remarks>
#else
        /// <summary>
        /// Automatically include Activity.SpanId, Activity.TraceId and Activity.ParentId.
        /// </summary>
        /// <remarks>
        /// Intended for Net5.0 where these properties are no longer included by default for performance reasons
        /// 
        /// Consider using <a href="https://www.nuget.org/packages/NLog.DiagnosticSource/">${activity}</a> as alternative
        /// </remarks>
#endif
        public bool IncludeActivityIdsWithBeginScope { get; set; }

        /// <summary>
        /// See <see cref="IncludeActivityIdsWithBeginScope"/> for documentation
        /// </summary>
        [Obsolete("Fixed spelling, so use IncludeActivityIdsWithBeginScope instead. Marked obsolete with NLog 5.0")]
        public bool IncludeActivtyIdsWithBeginScope { get => IncludeActivityIdsWithBeginScope; set => IncludeActivityIdsWithBeginScope = value; }

        /// <summary>
        /// Resets the default Microsoft LoggerFactory Filter for the <see cref="NLogLoggerProvider"/>, and instead only uses NLog LoggingRules.
        /// </summary>
        /// <remarks>This option affects the building of service configuration, so assigning it from appsettings.json has no effect (loaded after).</remarks>
        public bool RemoveLoggerFactoryFilter { get; set; } = true;

        /// <summary>
        /// Replace Microsoft LoggerFactory with a pure <see cref="NLogLoggerFactory" />, and disables Microsoft Filter Logic and removes other LoggingProviders.
        /// </summary>
        /// <remarks>This option affects the building of service configuration, so assigning it from appsettings.json has no effect (loaded after).</remarks>
        public bool ReplaceLoggerFactory { get; set; }

        /// <summary>
        /// Checks the Host Configuration for the specified section-name, and tries to load NLog-LoggingConfiguration after creation of NLogLoggerProvider
        /// </summary>
        /// <remarks>Will only attempt to load NLog-LoggingConfiguration if valid section-name, and NLog-LoggingConfiguration has not been loaded already.</remarks>
        public string LoggingConfigurationSectionName { get; set; } = "NLog";

        /// <summary>
        /// Enable additional capture of the entire <see cref="Microsoft.Extensions.Logging.EventId"/> as "EventId"-property.
        /// </summary>
        /// <remarks>
        /// Enabling capture of the entire "EventId" will increase memory allocation and gives a performance hit. Faster to use "EventId_Id" + "EventId_Name".
        /// </remarks>
        public bool CaptureEntireEventId { get; set; }

        /// <summary>
        /// Enable NLog Targets and Layouts to perform dependency lookup using the Microsoft Dependency Injection IServiceProvider
        /// </summary>
        public bool RegisterServiceProvider { get; set; } = true;

        /// <summary>Initializes a new instance NLogProviderOptions with default values.</summary>
        public NLogProviderOptions()
        {
        }

        /// <summary>
        /// Default options
        /// </summary>
        internal static readonly NLogProviderOptions Default = new NLogProviderOptions();
    }
}
