using System.Linq;
using System.Text;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Renders output that simulates simple Microsoft Console Logger. Useful for Hosting Lifetime Startup Messages.
    /// </summary>
    /// <seealso href="https://github.com/NLog/NLog/wiki/MicrosoftConsoleLayout">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("MicrosoftConsoleLayout")]
    [ThreadAgnostic]
    class MicrosoftConsoleLayoutRenderer : LayoutRenderer
    {
        private static readonly string[] EventIdMapper = Enumerable.Range(0, 512).Select(id => id.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray();

        /// <summary>
        /// Gets or sets format string used to format timestamp in logging messages. Defaults to <c>null</c>.
        /// </summary>
        public string TimestampFormat
        {
            get => _timestampFormat;
            set
            {
                _timestampFormat = value;
                _timestampFormatString = string.IsNullOrEmpty(value) ? string.Empty : $"{{0:{value}}}";
            }
        }
        private string _timestampFormat = string.Empty;
        private string _timestampFormatString = string.Empty;

        /// <summary>
        /// Gets or sets indication whether or not UTC timezone should be used to format timestamps in logging messages. Defaults to <c>false</c>.
        /// </summary>
        public bool UseUtcTimestamp { get; set; }

        /// <inheritdoc />
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            string timestampFormatString = _timestampFormatString;
            if (!string.IsNullOrEmpty(timestampFormatString))
            {
                var timestamp = UseUtcTimestamp ? logEvent.TimeStamp.ToUniversalTime() : logEvent.TimeStamp;
                builder.AppendFormat(UseUtcTimestamp ? System.Globalization.CultureInfo.InvariantCulture : System.Globalization.CultureInfo.CurrentCulture, timestampFormatString, timestamp);
                builder.Append(' ');
            }

            var microsoftLogLevel = ConvertLogLevel(logEvent.Level);
            builder.Append(microsoftLogLevel);
            builder.Append(": ");
            builder.Append(logEvent.LoggerName);
            builder.Append('[');
            AppendEventId(LookupEventId(logEvent), builder);
            builder.Append(']');
            builder.Append(System.Environment.NewLine);
            builder.Append("      ");
            builder.Append(logEvent.FormattedMessage);
            if (logEvent.Exception != null)
            {
                builder.Append(System.Environment.NewLine);
                builder.Append(logEvent.Exception.ToString());
            }
        }

        private static void AppendEventId(int eventId, StringBuilder builder)
        {
            if (eventId == 0)
                builder.Append('0');
            else if (eventId > 0 && eventId < EventIdMapper.Length)
                builder.Append(EventIdMapper[eventId]);
            else
                builder.Append(eventId);    // .NET5 (and newer) can append integer without string-allocation (using span)
        }

        private static int LookupEventId(LogEventInfo logEvent)
        {
            if (logEvent.HasProperties)
            {
                if (logEvent.Properties.TryGetValue(nameof(EventIdCaptureType.EventId), out var eventObject))
                {
                    if (eventObject is int eventId)
                        return eventId;
                    else if (eventObject is Microsoft.Extensions.Logging.EventId eventIdStruct)
                        return eventIdStruct.Id;
                }

                if (logEvent.Properties.TryGetValue(nameof(EventIdCaptureType.EventId_Id), out var eventid) && eventid is int)
                {
                    return (int)eventid;
                }
            }

            return 0;
        }

        private static string ConvertLogLevel(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Trace)
                return "trce";
            else if (logLevel == LogLevel.Debug)
                return "dbug";
            else if (logLevel == LogLevel.Info)
                return "info";
            else if (logLevel == LogLevel.Warn)
                return "warn";
            else if (logLevel == LogLevel.Error)
                return "fail";
            else
                return "crit";  // Fatal
        }
    }
}
