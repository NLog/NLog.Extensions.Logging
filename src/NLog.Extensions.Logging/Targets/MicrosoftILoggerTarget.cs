using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog.Layouts;
using NLog.Targets;
using EventId = Microsoft.Extensions.Logging.EventId;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Forwards NLog LogEvents to Microsoft ILogger-interface with support for NLog Layout-features
    /// </summary>
    [Target("MicrosoftILogger")]
    public class MicrosoftILoggerTarget : TargetWithContext
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        /// <summary>
        /// EventId forwarded to ILogger
        /// </summary>
        public Layout EventId { get; set; }

        /// <summary>
        /// EventId-Name forwarded to ILogger
        /// </summary>
        public Layout EventName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftILoggerTarget" /> class.
        /// </summary>
        /// <param name="logger">Microsoft ILogger instance</param>
        public MicrosoftILoggerTarget(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
            Layout = "${message}";
            OptimizeBufferReuse = true;
        }

        /// <summary>
        /// Converts NLog-LogEvent into Microsoft Extension Logging LogState
        /// </summary>
        /// <param name="logEvent"></param>
        protected override void Write(LogEventInfo logEvent)
        {
            var logLevel = ConvertToLogLevel(logEvent.Level);
            if (!_logger.IsEnabled(logLevel))
                return;

            var eventId = default(EventId);
            if (EventId != null)
            {
                var eventIdValue = RenderLogEvent(EventId, logEvent);
                if (!string.IsNullOrEmpty(eventIdValue) && int.TryParse(eventIdValue, out int eventIdParsed) && eventIdParsed != 0)
                    eventId = new EventId(eventIdParsed);
            }
            if (EventName != null)
            {
                var eventNameValue = RenderLogEvent(EventName, logEvent);
                if (!string.IsNullOrEmpty(eventNameValue))
                    eventId = new EventId(eventId.Id, eventNameValue);
            }

            var layoutMessage = RenderLogEvent(Layout, logEvent);
            IDictionary<string, object> contextProperties = null;
            if (ContextProperties.Count > 0 || IncludeMdlc || IncludeMdc || IncludeGdc)
            {
                contextProperties = GetContextProperties(logEvent);
                if (contextProperties?.Count == 0)
                    contextProperties = null;
            }

            _logger.Log(ConvertToLogLevel(logEvent.Level), eventId, new LogState(logEvent, layoutMessage, contextProperties), logEvent.Exception, LogStateFormatter);
        }

        struct LogState : IReadOnlyList<KeyValuePair<string, object>>
        {
            public readonly LogEventInfo LogEvent;
            public readonly string LayoutMessage;
            public readonly IDictionary<string, object> ContextProperties;

            public int Count => (LogEvent.HasProperties ? LogEvent.Properties.Count : 0) + (ContextProperties?.Count ?? 0) + 1;

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (LogEvent.HasProperties)
                    {
                        if (TryGetPropertyFromIndex(LogEvent.Properties, CreateLogEventProperty, ref index, out var property))
                            return property;
                    }
                    if (ContextProperties != null)
                    {
                        if (TryGetPropertyFromIndex(ContextProperties, p => p, ref index, out var property))
                            return property;
                    }
                    if (index != 0)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return CreateOriginalFormatProperty();
                }
            }

            public LogState(LogEventInfo logEvent, string layoutMessage, IDictionary<string, object> contextProperties)
            {
                LogEvent = logEvent;
                LayoutMessage = layoutMessage;
                ContextProperties = contextProperties;
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                IList<KeyValuePair<string, object>> originalMessage = new[] { CreateOriginalFormatProperty() };
                IEnumerable<KeyValuePair<string, object>> allProperties = ContextProperties?.Concat(originalMessage) ?? originalMessage;
                if (LogEvent.HasProperties)
                {
                    allProperties = LogEvent.Properties.Select(prop => CreateLogEventProperty(prop)).Concat(allProperties);
                }
                return allProperties.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private static bool TryGetPropertyFromIndex<TKey, TValue>(ICollection<KeyValuePair<TKey, TValue>> properties, Func<KeyValuePair<TKey, TValue>, KeyValuePair<string, object>> converter, ref int index, out KeyValuePair<string, object> property)
            {
                if (index < properties.Count)
                {
                    foreach (var prop in properties)
                    {
                        if (index-- == 0)
                        {
                            property = converter(prop);
                            return true;
                        }
                    }
                }
                else
                {
                    index -= properties.Count;
                }

                property = default(KeyValuePair<string, object>);
                return false;
            }

            private static KeyValuePair<string, object> CreateLogEventProperty(KeyValuePair<object, object> prop)
            {
                return new KeyValuePair<string, object>(prop.Key.ToString(), prop.Value);
            }

            private KeyValuePair<string, object> CreateOriginalFormatProperty()
            {
                return new KeyValuePair<string, object>(NLogLogger.OriginalFormatPropertyName, LogEvent.Message);
            }
        }

        static string LogStateFormatter(LogState logState, Exception _)
        {
            return logState.LayoutMessage;
        }

        static Microsoft.Extensions.Logging.LogLevel ConvertToLogLevel(NLog.LogLevel logLevel)
        {
            if (logLevel == NLog.LogLevel.Trace)
                return Microsoft.Extensions.Logging.LogLevel.Trace;
            else if (logLevel == NLog.LogLevel.Debug)
                return Microsoft.Extensions.Logging.LogLevel.Debug;
            else if (logLevel == NLog.LogLevel.Info)
                return Microsoft.Extensions.Logging.LogLevel.Information;
            else if (logLevel == NLog.LogLevel.Warn)
                return Microsoft.Extensions.Logging.LogLevel.Warning;
            else if (logLevel == NLog.LogLevel.Error)
                return Microsoft.Extensions.Logging.LogLevel.Error;
            else // if (logLevel == NLog.LogLevel.Fatal)
                return Microsoft.Extensions.Logging.LogLevel.Critical;
        }
    }
}
