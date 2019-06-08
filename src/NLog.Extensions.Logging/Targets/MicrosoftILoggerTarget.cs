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

        private struct LogState : IReadOnlyList<KeyValuePair<string, object>>
        {
            private readonly LogEventInfo _logEvent;
            public readonly string LayoutMessage;
            private readonly IDictionary<string, object> _contextProperties;

            public int Count => (_logEvent.HasProperties ? _logEvent.Properties.Count : 0) + (_contextProperties?.Count ?? 0) + 1;

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (_logEvent.HasProperties && TryGetPropertyFromIndex(_logEvent.Properties, CreateLogEventProperty, ref index, out var property))
                    {
                        return property;
                    }
                    if (_contextProperties != null && TryGetPropertyFromIndex(_contextProperties, p => p, ref index, out var contextProperty))
                    {
                        return contextProperty;
                    }
                    if (index != 0)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return CreateOriginalFormatProperty();
                }
            }

            public LogState(LogEventInfo logEvent, string layoutMessage, IDictionary<string, object> contextProperties)
            {
                _logEvent = logEvent;
                LayoutMessage = layoutMessage;
                _contextProperties = contextProperties;
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                IList<KeyValuePair<string, object>> originalMessage = new[] { CreateOriginalFormatProperty() };
                IEnumerable<KeyValuePair<string, object>> allProperties = _contextProperties?.Concat(originalMessage) ?? originalMessage;
                if (_logEvent.HasProperties)
                {
                    allProperties = _logEvent.Properties.Select(CreateLogEventProperty).Concat(allProperties);
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

                property = default;
                return false;
            }

            private static KeyValuePair<string, object> CreateLogEventProperty(KeyValuePair<object, object> prop)
            {
                return new KeyValuePair<string, object>(prop.Key.ToString(), prop.Value);
            }

            private KeyValuePair<string, object> CreateOriginalFormatProperty()
            {
                return new KeyValuePair<string, object>(NLogLogger.OriginalFormatPropertyName, _logEvent.Message);
            }
        }

        private static string LogStateFormatter(LogState logState, Exception _)
        {
            return logState.LayoutMessage;
        }

        private static Microsoft.Extensions.Logging.LogLevel ConvertToLogLevel(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Trace)
                return Microsoft.Extensions.Logging.LogLevel.Trace;
            if (logLevel == LogLevel.Debug)
                return Microsoft.Extensions.Logging.LogLevel.Debug;
            if (logLevel == LogLevel.Info)
                return Microsoft.Extensions.Logging.LogLevel.Information;
            if (logLevel == LogLevel.Warn)
                return Microsoft.Extensions.Logging.LogLevel.Warning;
            if (logLevel == LogLevel.Error)
                return Microsoft.Extensions.Logging.LogLevel.Error;
            return Microsoft.Extensions.Logging.LogLevel.Critical;
        }
    }
}
