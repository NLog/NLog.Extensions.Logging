﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Forwards NLog LogEvents to Microsoft ILogger-interface with support for NLog Layout-features
    /// </summary>
    [Target("MicrosoftLogger")]
    public class MicrosoftLoggerTarget : TargetWithLayout
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
        /// Initializes a new instance of the <see cref="MicrosoftLoggerTarget" /> class.
        /// </summary>
        /// <param name="logger">Microsoft ILogger instance</param>
        public MicrosoftLoggerTarget(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
            Layout = "${longdate}|${level:uppercase=true}|${logger}|${message:withException=true:exceptionSeparator=|}";
            OptimizeBufferReuse = true;
        }

        /// <summary>
        /// Converts NLog-LogEvent into Microsoft Extension Logging LogState
        /// </summary>
        /// <param name="logEvent"></param>
        protected override void Write(LogEventInfo logEvent)
        {
            var logLevel = LookupLogLevel(logEvent.Level);
            if (!_logger.IsEnabled(logLevel))
                return;

            var eventId = default(Microsoft.Extensions.Logging.EventId);
            if (EventId != null)
            {
                var eventIdValue = RenderLogEvent(EventId, logEvent);
                if (!string.IsNullOrEmpty(eventIdValue) && int.TryParse(eventIdValue, out int eventIdParsed) && eventIdParsed != 0)
                    eventId = new Microsoft.Extensions.Logging.EventId(eventIdParsed);
            }
            if (EventName != null)
            {
                var eventNameValue = RenderLogEvent(EventName, logEvent);
                if (!string.IsNullOrEmpty(eventNameValue))
                    eventId = new Microsoft.Extensions.Logging.EventId(eventId.Id, eventNameValue);
            }

            var layoutMessage = RenderLogEvent(Layout, logEvent);
            _logger.Log(LookupLogLevel(logEvent.Level), eventId, new LogState(logEvent, layoutMessage), logEvent.Exception, LogStateFormatter);
        }

        struct LogState : IReadOnlyList<KeyValuePair<string, object>>
        {
            public int Count => (LogEvent.HasProperties ? LogEvent.Properties.Count : 0) + 1;

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (LogEvent.HasProperties)
                    {
                        foreach (var prop in LogEvent.Properties)
                        {
                            if (index-- == 0)
                                return new KeyValuePair<string, object>(prop.Key.ToString(), prop.Value);
                        }
                    }
                    if (index != 0)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return CreateOriginalFormatProperty();
                }
            }

            private KeyValuePair<string, object> CreateOriginalFormatProperty()
            {
                return new KeyValuePair<string, object>(NLogLogger.OriginalFormatPropertyName, LogEvent.Message);
            }

            public readonly LogEventInfo LogEvent;
            public readonly string LayoutMessage;

            public LogState(LogEventInfo logEvent, string layoutMessage)
            {
                LogEvent = logEvent;
                LayoutMessage = layoutMessage;
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                if (LogEvent.HasProperties)
                {
                    return LogEvent.Properties.Select(prop => new KeyValuePair<string, object>(prop.Key.ToString(), prop.Value)).Concat(new[] { CreateOriginalFormatProperty() }).GetEnumerator();
                }
                else
                {
                    IList<KeyValuePair<string, object>> originalMessage = new[] { CreateOriginalFormatProperty() };
                    return originalMessage.GetEnumerator();
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        static string LogStateFormatter(LogState logState, Exception _)
        {
            return logState.LayoutMessage;
        }

        Microsoft.Extensions.Logging.LogLevel LookupLogLevel(NLog.LogLevel logLevel)
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
