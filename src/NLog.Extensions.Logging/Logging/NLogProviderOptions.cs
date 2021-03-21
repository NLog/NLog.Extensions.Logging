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
        /// Skip allocation of <see cref="LogEventInfo.Properties" />-dictionary
        /// </summary>
        /// <remarks>
        /// using
        ///     <c>default(EventId)</c></remarks>
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
        /// Enable capture of scope information and inject into <see cref="NestedDiagnosticsLogicalContext" /> and <see cref="MappedDiagnosticsLogicalContext" />
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
        /// <remarks>For Net5.0 where these properties are no longer included by default for performance reasons</remarks>
#else
        /// <summary>
        /// Automatically include Activity.SpanId, Activity.TraceId and Activity.ParentId.
        /// </summary>
        /// <remarks>For Net5.0 where these properties are no longer included by default for performance reasons</remarks>
#endif
        public bool IncludeActivtyIdsWithBeginScope { get; set; }

        /// <summary>
        /// Resets the default Microsoft LoggerFactory Filter for the <see cref="NLogLoggerProvider"/>
        /// </summary>
        /// <remarks>This option affects the building of service configuration, so assigning it from appsettings.json has no effect (loaded after).</remarks>
        public bool RemoveLoggerFactoryFilter { get; set; }

        /// <summary>
        /// Replace Microsoft LoggerFactory with a pure <see cref="NLogLoggerFactory" />
        /// </summary>
        /// <remarks>This option affects the building of service configuration, so assigning it from appsettings.json has no effect (loaded after).</remarks>
        public bool ReplaceLoggerFactory { get; set; }

        /// <summary>
        /// Enable additional capture of the entire <see cref="Microsoft.Extensions.Logging.EventId"/> as "EventId"-property.
        /// </summary>
        /// <remarks>
        /// Enabling capture of the entire "EventId" will increase memory allocation and gives a performance hit. Faster to use "EventId_Id" + "EventId_Name".
        /// </remarks>
        public bool CaptureEntireEventId { get; set; }

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
