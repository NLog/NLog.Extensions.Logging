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
            builder.Append("[");
            int eventId = 0;
            if (logEvent.HasProperties && logEvent.Properties.TryGetValue("EventId_Id", out var eventIdValue))
            {
                if (eventIdValue is int)
                    eventId = (int)eventIdValue;
                else if (!int.TryParse(eventIdValue?.ToString() ?? string.Empty, out eventId))
                    eventId = 0;
            }
            else
            {
                eventId = 0;
            }
            builder.Append(ConvertEventId(eventId));
            builder.Append("]");
            builder.Append(System.Environment.NewLine);
            builder.Append("      ");
            builder.Append(logEvent.FormattedMessage);
            if (logEvent.Exception != null)
            {
                builder.Append(System.Environment.NewLine);
                builder.Append(logEvent.Exception.ToString());
            }
        }

        static string ConvertEventId(int eventId)
        {
            if (eventId == 0)
                return "0";
            else if (eventId > 0 || eventId < EventIdMapper.Length)
                return EventIdMapper[eventId];
            else
                return eventId.ToString();
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
