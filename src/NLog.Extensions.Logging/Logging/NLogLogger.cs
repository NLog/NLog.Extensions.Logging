using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NLog.Common;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Wrap NLog's Logger in a Microsoft.Extensions.Logging's interface <see cref="Microsoft.Extensions.Logging.ILogger"/>.
    /// </summary>
    internal class NLogLogger : Microsoft.Extensions.Logging.ILogger
    {
        private readonly Logger _logger;
        private readonly NLogProviderOptions _options;

        internal const string OriginalFormatPropertyName = "{OriginalFormat}";
        private static readonly object EmptyEventId = default(EventId);    // Cache boxing of empty EventId-struct
        private static readonly object ZeroEventId = default(EventId).Id;  // Cache boxing of zero EventId-Value
        private Tuple<string, string, string> _eventIdPropertyNames;

        public NLogLogger(Logger logger, NLogProviderOptions options)
        {
            _logger = logger;
            _options = options ?? NLogProviderOptions.Default;
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var nLogLogLevel = ConvertLogLevel(logLevel);
            if (!IsEnabled(nLogLogLevel))
            {
                return;
            }
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);
            var messageTemplate = _options.CaptureMessageTemplates ? state as IReadOnlyList<KeyValuePair<string, object>> : null;
            LogEventInfo eventInfo = CreateLogEventInfo(nLogLogLevel, message, messageTemplate);
            if (exception != null)
            {
                eventInfo.Exception = exception;
            }

            CaptureEventId(eventInfo, eventId);

            if (messageTemplate == null)
            {
                CaptureMessageProperties(eventInfo, state);
            }

            _logger.Log(eventInfo);
        }


        private LogEventInfo CreateLogEventInfo(LogLevel nLogLogLevel, string message, IReadOnlyList<KeyValuePair<string, object>> parameterList)
        {
            if (parameterList != null && parameterList.Count > 0 && IsNonDigitValue(parameterList[0].Key))
            {
                return CreateLogEventInfoWithMultipleParameters(nLogLogLevel, message, parameterList);
            }
            return LogEventInfo.Create(nLogLogLevel, _logger.Name, message);
        }

        private static bool IsNonDigitValue(string value)
        {
            return !String.IsNullOrEmpty(value) && (value.Length != 1 || !Char.IsDigit(value[0]));
        }

        /// <summary>
        /// Create Log Event with multiple parameters (last parameter is the {OriginalFormat})
        /// </summary>
        private LogEventInfo CreateLogEventInfoWithMultipleParameters(LogLevel nLogLogLevel, string message, IReadOnlyList<KeyValuePair<string, object>> parameterList)
        {
            var messageTemplateParameters = new NLogMessageParameterList(parameterList);
            var originalMessage = messageTemplateParameters.OriginalMessage as string;
            var logEvent = new LogEventInfo(nLogLogLevel, _logger.Name, originalMessage ?? message, messageTemplateParameters);
            if (originalMessage != null)
            {
                SetLogEventMessageFormatter(logEvent, messageTemplateParameters, message);
            }
            return logEvent;
        }

        private static void SetLogEventMessageFormatter(LogEventInfo logEvent, NLogMessageParameterList messageTemplateParameters, string formattedMessage)
        {
            var parameters = new object[messageTemplateParameters.Count + 1];
            for (int i = 0; i < parameters.Length - 1; ++i)
                parameters[i] = messageTemplateParameters[i].Value;
            parameters[parameters.Length - 1] = formattedMessage;
            logEvent.Parameters = parameters;
            logEvent.MessageFormatter = (l) => (string)l.Parameters[l.Parameters.Length - 1];
        }

        private void CaptureEventId(LogEventInfo eventInfo, EventId eventId)
        {
            if (!_options.IgnoreEmptyEventId || eventId.Id != 0 || !String.IsNullOrEmpty(eventId.Name))
            {
                // Attempt to reuse the same string-allocations based on the current <see cref="NLogProviderOptions.EventIdSeparator"/>
                var eventIdPropertyNames = _eventIdPropertyNames ?? new Tuple<string, string, string>(null, null, null);
                var eventIdSeparator = _options.EventIdSeparator ?? String.Empty;
                if (!ReferenceEquals(eventIdPropertyNames.Item1, eventIdSeparator))
                {
                    // Perform atomic cache update of the string-allocations matching the current separator
                    _eventIdPropertyNames = eventIdPropertyNames = CreateEventIdPropertyNames(eventIdSeparator);
                }

                var idIsZero = eventId.Id == 0;
                eventInfo.Properties[eventIdPropertyNames.Item2] = idIsZero ? ZeroEventId : eventId.Id;
                eventInfo.Properties[eventIdPropertyNames.Item3] = eventId.Name;
                eventInfo.Properties["EventId"] = idIsZero && eventId.Name == null ? EmptyEventId : eventId;
            }
        }

        private static Tuple<string, string, string> CreateEventIdPropertyNames(string eventIdSeparator)
        {
            var eventIdPropertyNames = new Tuple<string, string, string>(
                eventIdSeparator,
                String.Concat("EventId", eventIdSeparator, "Id"),
                String.Concat("EventId", eventIdSeparator, "Name"));
            return eventIdPropertyNames;
        }

        private void CaptureMessageProperties<TState>(LogEventInfo eventInfo, TState state)
        {
            if (_options.CaptureMessageProperties && state is IEnumerable<KeyValuePair<string, object>> messageProperties)
            {
                foreach (var property in messageProperties)
                {
                    if (String.IsNullOrEmpty(property.Key))
                        continue;

                    eventInfo.Properties[property.Key] = property.Value;
                }
            }
        }

        /// <summary>
        /// Is logging enabled for this logger at this <paramref name="logLevel"/>?
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            var convertLogLevel = ConvertLogLevel(logLevel);
            return IsEnabled(convertLogLevel);
        }

        /// <summary>
        /// Is logging enabled for this logger at this <paramref name="logLevel"/>?
        /// </summary>
        private bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        /// <summary>
        /// Convert loglevel to NLog variant.
        /// </summary>
        /// <param name="logLevel">level to be converted.</param>
        /// <returns></returns>
        private static LogLevel ConvertLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return LogLevel.Trace;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return LogLevel.Debug;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return LogLevel.Info;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return LogLevel.Warn;
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return LogLevel.Error;
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return LogLevel.Fatal;
                case Microsoft.Extensions.Logging.LogLevel.None:
                    return LogLevel.Off;
                default:
                    return LogLevel.Debug;
            }
        }

        class ScopeProperties : IDisposable
        {
            List<IDisposable> _properties;
            List<IDisposable> Properties => _properties ?? (_properties = new List<IDisposable>());


            public static IDisposable CreateFromState<TState>(TState state, IEnumerable<KeyValuePair<string, object>> messageProperties)
            {
                ScopeProperties scope = new ScopeProperties();

                foreach (var property in messageProperties)
                {
                    if (String.IsNullOrEmpty(property.Key))
                        continue;

                    scope.AddProperty(property.Key, property.Value);
                }

                scope.AddDispose(NestedDiagnosticsLogicalContext.Push(state));
                return scope;
            }

            public void AddDispose(IDisposable disposable)
            {
                Properties.Add(disposable);
            }

            public void AddProperty(string key, object value)
            {
                AddDispose(new ScopeProperty(key, value));
            }

            public void Dispose()
            {
                var properties = _properties;
                if (properties != null)
                {
                    _properties = null;
                    foreach (var property in properties)
                    {
                        try
                        {
                            property.Dispose();
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Trace(ex, "Exception in Dispose property {0}", property);
                        }
                    }
                }
            }

            class ScopeProperty : IDisposable
            {
                string _key;

                public ScopeProperty(string key, object value)
                {
                    _key = key;
                    MappedDiagnosticsLogicalContext.Set(key, value);
                }

                public void Dispose()
                {
                    MappedDiagnosticsLogicalContext.Remove(_key);
                }
            }

        }

        /// <summary>
        /// Begin a scope. Use in config with ${ndlc} 
        /// </summary>
        /// <param name="state">The state (message)</param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (_options.CaptureMessageProperties && state is IEnumerable<KeyValuePair<string, object>> messageProperties)
            {
                return ScopeProperties.CreateFromState(state, messageProperties);
            }

            return NestedDiagnosticsLogicalContext.Push(state);
        }
    }
}
