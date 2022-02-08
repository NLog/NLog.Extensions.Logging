using System.Linq;
using System.Text;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Renders output that simulates simple Microsoft Console Logger. Useful for Hosting Lifetime Startup Messages.
    /// </summary>
    [LayoutRenderer("MicrosoftConsoleLayout")]
    [ThreadSafe]
    [ThreadAgnostic]
    class MicrosoftConsoleLayoutRenderer : LayoutRenderer
    {
        private static readonly string[] EventIdMapper = Enumerable.Range(0, 50).Select(id => id.ToString()).ToArray();

        /// <inheritdoc />
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
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
                if (logEvent.Properties.TryGetValue("EventId_Id", out var eventObject))
                {
                    if (eventObject is int eventId)
                        return eventId;
                    else if (eventObject is Microsoft.Extensions.Logging.EventId eventIdStruct)
                        return eventIdStruct.Id;
                }

                if (logEvent.Properties.TryGetValue("EventId", out var eventid))
                {
                    if (eventObject is int eventId)
                        return eventId;
                    else if (eventObject is Microsoft.Extensions.Logging.EventId eventIdStruct)
                        return eventIdStruct.Id;
                }
            }

            return 0;
        }

        string ConvertLogLevel(LogLevel logLevel)
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
