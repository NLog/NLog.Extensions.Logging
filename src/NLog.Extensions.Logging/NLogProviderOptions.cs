using System;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Options for logging to NLog with 
    /// </summary>
    public class NLogProviderOptions
    {
        /// <summary>
        /// Separator between for EventId.Id and EventId.Name. Default to .
        /// </summary>
        public string EventIdSeparator { get; set; }

        /// <summary>
        /// Skip allocation of <see cref="LogEventInfo.Properties" />-dictionary
        /// </summary>
        /// <remarks>
        /// using
        ///     <c>default(EventId)</c></remarks>
        public bool IgnoreEmptyEventId { get; set; }

        /// <summary>
        /// Enable structured logging by capturing message template parameters and inject into the <see cref="LogEventInfo.Properties" />-dictionary
        /// </summary>
        public bool CaptureMessageTemplates { get; set; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public NLogProviderOptions()
        {
            EventIdSeparator = ".";
        }

        /// <summary>
        /// Default options
        /// </summary>
        internal static NLogProviderOptions Default = new NLogProviderOptions();
    }
}
