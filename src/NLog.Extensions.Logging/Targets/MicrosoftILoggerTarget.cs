﻿using System;
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
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;
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
        /// <param name="logger">Microsoft ILogger singleton instance</param>
        public MicrosoftILoggerTarget(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
            Layout = "${message}";
            OptimizeBufferReuse = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftILoggerTarget" /> class.
        /// </summary>
        /// <param name="loggerFactory">Microsoft ILoggerFactory instance</param>
        public MicrosoftILoggerTarget(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            Layout = "${message}";
            OptimizeBufferReuse = true;
        }

        /// <summary>
        /// Converts NLog-LogEvent into Microsoft Extension Logging LogState
        /// </summary>
        /// <param name="logEvent"></param>
        protected override void Write(LogEventInfo logEvent)
        {
            var ilogger = _logger ?? (string.IsNullOrEmpty(logEvent.LoggerName) ? _loggerFactory.CreateLogger("NLog") : _loggerFactory.CreateLogger(logEvent.LoggerName));

            var logLevel = ConvertToLogLevel(logEvent.Level);
            if (!ilogger.IsEnabled(logLevel))
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

            ilogger.Log(ConvertToLogLevel(logEvent.Level), eventId, new LogState(logEvent, layoutMessage, contextProperties), logEvent.Exception, (s,ex) => LogStateFormatter(s));
        }

        private struct LogState : IReadOnlyList<KeyValuePair<string, object>>, IEquatable<LogState>
        {
            private readonly LogEventInfo _logEvent;
            public readonly string LayoutMessage;
            private readonly IDictionary<string, object> _contextProperties;

            public int Count => (_logEvent.HasProperties ? _logEvent.Properties.Count : 0) + (_contextProperties?.Count ?? 0) + 1;

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (_logEvent.HasProperties && TryGetLogEventProperty(_logEvent.Properties, ref index, out var property))
                    {
                        return property;
                    }
                    if (_contextProperties != null && TryGetContextProperty(_contextProperties, ref index, out var contextProperty))
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
                    allProperties = _logEvent.Properties.Select(p => CreateLogEventProperty(p)).Concat(allProperties);
                }
                return allProperties.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private KeyValuePair<string, object> CreateOriginalFormatProperty()
            {
                return new KeyValuePair<string, object>(NLogLogger.OriginalFormatPropertyName, _logEvent.Message);
            }

            public bool Equals(LogState other)
            {
                return ReferenceEquals(_logEvent, other._logEvent);
            }

            public override bool Equals(object obj)
            {
                return obj is LogState other && Equals(other);
            }

            public override int GetHashCode()
            {
                return _logEvent.GetHashCode();
            }
        }

        private static bool TryGetContextProperty(IDictionary<string, object> contextProperties, ref int index, out KeyValuePair<string, object> contextProperty)
        {
            return TryGetPropertyFromIndex(contextProperties, p => p, ref index, out contextProperty);
        }

        private static bool TryGetLogEventProperty(IDictionary<object, object> logEventProperties, ref int index, out KeyValuePair<string, object> logEventProperty)
        {
            return TryGetPropertyFromIndex(logEventProperties, p => CreateLogEventProperty(p), ref index, out logEventProperty);
        }

        private static bool TryGetPropertyFromIndex<TKey, TValue>(ICollection<KeyValuePair<TKey, TValue>> properties, Func<KeyValuePair<TKey, TValue>, KeyValuePair<string, object>> converter, ref int index, out KeyValuePair<string, object> property) where TKey : class
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

        private static string LogStateFormatter(LogState logState)
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
