using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NLog.MessageTemplates;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Wrap NLog's Logger in a Microsoft.Extensions.Logging's interface <see cref="Microsoft.Extensions.Logging.ILogger"/>.
    /// </summary>
    internal class NLogLogger : Microsoft.Extensions.Logging.ILogger
    {
        private readonly Logger _logger;
        private readonly NLogProviderOptions _options;
        private readonly NLogBeginScopeParser _beginScopeParser;
        internal string LoggerName => _logger?.Name;

        internal const string OriginalFormatPropertyName = "{OriginalFormat}";
        private static readonly object ZeroEventId = default(EventId).Id;  // Cache boxing of zero EventId-Value
        private static readonly object[] EventIdBoxing = Enumerable.Range(0, 50).Select(v => (object)v).ToArray();  // Most EventIds in the ASP.NET Core Engine is below 50
        private Tuple<string, string, string> _eventIdPropertyNames;

        public NLogLogger(Logger logger, NLogProviderOptions options, NLogBeginScopeParser beginScopeParser)
        {
            _logger = logger;
            _options = options ?? NLogProviderOptions.Default;
            _beginScopeParser = beginScopeParser;
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsLogLevelEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            LogEventInfo eventInfo = CreateLogEventInfo(ConvertLogLevel(logLevel), eventId, state, exception, formatter);

            if (exception != null)
            {
                eventInfo.Exception = exception;
            }

            _logger.Log(typeof(Microsoft.Extensions.Logging.ILogger), eventInfo);
        }

        private LogEventInfo CreateLogEventInfo<TState>(LogLevel nLogLogLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if ((_options.CaptureMessageTemplates || _options.CaptureMessageProperties) && state is IReadOnlyList<KeyValuePair<string, object>> messagePropertyList)
            {
                if (_options.CaptureMessageTemplates)
                {
                    var messageParameters = NLogMessageParameterList.TryParse(messagePropertyList);
                    if (messageParameters.Count == 0)
                    {
                        var logEvent = LogEventInfo.Create(nLogLogLevel, _logger.Name, formatter(state, exception));
                        CaptureEventIdProperties(logEvent, eventId);
                        return logEvent;
                    }
                    else
                    {
                        var logEvent = TryParseMessageTemplate(nLogLogLevel, messagePropertyList, messageParameters)
                            ?? CaptureMessageTemplate(nLogLogLevel, formatter(state, exception), messagePropertyList, messageParameters);
                        CaptureEventIdProperties(logEvent, eventId);
                        return logEvent;
                    }
                }
                else
                {
                    var logEvent = LogEventInfo.Create(nLogLogLevel, _logger.Name, formatter(state, exception));
                    CaptureMessagePropertiesList(logEvent, messagePropertyList);
                    CaptureEventIdProperties(logEvent, eventId);
                    return logEvent;
                }
            }
            else
            {
                var logEvent = LogEventInfo.Create(nLogLogLevel, _logger.Name, formatter(state, exception));
                if (_options.CaptureMessageProperties)
                {
                    if (state is IEnumerable<KeyValuePair<string, object>> messageProperties)
                        CaptureMessageProperties(logEvent, messageProperties);
                    CaptureEventIdProperties(logEvent, eventId);
                }
                return logEvent;
            }
        }

        /// <summary>
        /// Checks if the already parsed input message-parameters must be sent through
        /// the NLog MessageTemplate Parser for proper handling of message-template syntax (Ex. @)
        /// </summary>
        /// <remarks>
        /// Using the NLog MessageTemplate Parser will hurt performance: 1 x Microsoft Parser - 2 x NLog Parser - 1 x NLog Formatter
        /// </remarks>
        private LogEventInfo TryParseMessageTemplate(LogLevel nLogLogLevel, IReadOnlyList<KeyValuePair<string, object>> messageProperties, NLogMessageParameterList messageParameters)
        {
            if (messageParameters?.HasMessageTemplateSyntax(_options.ParseMessageTemplates) == true)
            {
                var originalMessage = messageParameters.GetOriginalMessage(messageProperties);
                var logEvent = new LogEventInfo(nLogLogLevel, _logger.Name, null, originalMessage, SingleItemArray);
                var messageTemplateParameters = logEvent.MessageTemplateParameters;   // Forces parsing of OriginalMessage
                if (messageTemplateParameters.Count > 0)
                {
                    // We have parsed the message and found parameters, now we need to do the parameter mapping
                    CaptureLogEventInfoParameters(logEvent, messageParameters, messageTemplateParameters);
                    return logEvent;
                }

                return null;    // Parsing not possible
            }

            return null;    // Parsing not needed
        }

        private LogEventInfo CaptureMessageTemplate(LogLevel nLogLogLevel, string message, IReadOnlyList<KeyValuePair<string, object>> messageProperties, NLogMessageParameterList messageParameters)
        {
            // Parsing not needed, we take the fast route 
            var originalMessage = messageParameters.GetOriginalMessage(messageProperties) ?? message;
            var logEvent = new LogEventInfo(nLogLogLevel, _logger.Name, originalMessage, messageParameters.IsPositional ? EmptyParameterArray : messageParameters);
            if (!ReferenceEquals(originalMessage, message))
            {
                SetLogEventMessageFormatter(logEvent, messageParameters, message);
            }
            return logEvent;
        }

        /// <summary>
        /// Allocates object[]-array for <see cref="LogEventInfo.Parameters"/> after checking
        /// for mismatch between Microsoft Extension Logging and NLog Message Template Parser
        /// </summary>
        /// <remarks>
        /// Cannot trust the parameters received from Microsoft Extension Logging, as extra parameters can be injected
        /// </remarks>
        private static void CaptureLogEventInfoParameters(LogEventInfo logEvent, NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters)
        {
            if (messageTemplateParameters.IsPositional)
            {
                logEvent.Parameters = CreatePositionalLogEventInfoParameters(messageParameters, messageTemplateParameters, out var extraProperties);
                if (extraProperties?.Count > 0)
                    CaptureMessagePropertiesList(logEvent, extraProperties);
            }
            else if (!AllParameterCorrectlyPositionalMapped(messageParameters, messageTemplateParameters))
            {
                // Resolves mismatch between the input from Microsoft Extension Logging TState and NLog Message Template Parser
                logEvent.Parameters = CreateStructuredLogEventInfoParameters(messageParameters, messageTemplateParameters, out var extraProperties);
                if (extraProperties?.Count > 0)
                    CaptureMessagePropertiesList(logEvent, extraProperties);
            }
            else
            {
                // Everything is mapped correctly, inject messageParameters directly as params-array
                var paramsArray = new object[messageTemplateParameters.Count];
                for (int i = 0; i < paramsArray.Length; ++i)
                    paramsArray[i] = messageParameters[i].Value;
                logEvent.Parameters = paramsArray;
            }
        }

        private static readonly object[] SingleItemArray = { null };
        private static readonly IList<MessageTemplateParameter> EmptyParameterArray = Array.Empty<MessageTemplateParameter>();

        /// <summary>
        /// Are all parameters positional and correctly mapped?
        /// </summary>
        /// <param name="messageParameters"></param>
        /// <param name="messageTemplateParameters"></param>
        /// <returns>true if correct</returns>
        private static bool AllParameterCorrectlyPositionalMapped(NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters)
        {
            if (messageTemplateParameters.Count != messageParameters.Count)
            {
                return false;
            }

            for (int i = 0; i < messageTemplateParameters.Count; ++i)
            {
                if (!messageParameters[i].Name.Equals(messageTemplateParameters[i].Name))
                {
                    return false;
                }
            }

            return true;
        }

        private static object[] CreateStructuredLogEventInfoParameters(NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters, out List<KeyValuePair<string, object>> extraProperties)
        {
            extraProperties = null;

            var paramsArray = new object[messageTemplateParameters.Count];
            int startPos = 0;
            int messageParameterCount = messageParameters.Count;
            for (int i = 0; i < messageParameterCount; ++i)
            {
                var propertyName = messageParameters[i].Name;
                
                bool extraProperty = true;
                for (int j = startPos; j < messageTemplateParameters.Count; ++j)
                {
                    if (propertyName.Equals(messageTemplateParameters[j].Name))
                    {
                        extraProperty = false;
                        paramsArray[j] = messageParameters[i].Value;
                        if (startPos == j)
                            startPos++;
                        break;
                    }
                }

                if (extraProperty)
                {
                    extraProperties = AddExtraProperty(extraProperties, messageParameters[i]);
                }
            }

            return paramsArray;
        }

        private static object[] CreatePositionalLogEventInfoParameters(NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters, out List<KeyValuePair<string, object>> extraProperties)
        {
            extraProperties = null;

            var maxIndex = FindMaxIndex(messageTemplateParameters);
            object[] paramsArray = null;
            int messageParameterCount = messageParameters.Count;
            for (int i = 0; i < messageParameterCount; ++i)
            {
                // First positional name is the startPos
                if (char.IsDigit(messageParameters[i].Name[0]) && paramsArray == null)
                {
                    paramsArray = new object[maxIndex + 1];
                    for (int j = 0; j <= maxIndex; ++j)
                    {
                        if (i + j < messageParameterCount)
                            paramsArray[j] = messageParameters[i + j].Value;
                    }
                    i += maxIndex;
                }
                else
                {
                    extraProperties = AddExtraProperty(extraProperties, messageParameters[i]);
                }
            }

            return paramsArray ?? new object[maxIndex + 1];
        }

        /// <summary>
        /// Add Property and init list if needed
        /// </summary>
        /// <param name="extraProperties"></param>
        /// <param name="item"></param>
        /// <returns>list with at least one item</returns>
        private static List<KeyValuePair<string, object>> AddExtraProperty(List<KeyValuePair<string, object>> extraProperties, MessageTemplateParameter item)
        {
            extraProperties = extraProperties ?? new List<KeyValuePair<string, object>>();
            extraProperties.Add(new KeyValuePair<string, object>(item.Name, item.Value));
            return extraProperties;
        }

        /// <summary>
        /// Find max index of the parameters
        /// </summary>
        /// <param name="messageTemplateParameters"></param>
        /// <returns>index, 0 or higher</returns>
        private static int FindMaxIndex(MessageTemplateParameters messageTemplateParameters)
        {
            int maxIndex = 0;
            for (int i = 0; i < messageTemplateParameters.Count; ++i)
            {
                if (messageTemplateParameters[i].Name.Length == 1)
                    maxIndex = Math.Max(maxIndex, messageTemplateParameters[i].Name[0] - '0');
                else
                    maxIndex = Math.Max(maxIndex, int.Parse(messageTemplateParameters[i].Name, System.Globalization.CultureInfo.InvariantCulture));
            }

            return maxIndex;
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

        private void CaptureEventIdProperties(LogEventInfo logEvent, EventId eventId)
        {
            var captureEventId = _options.CaptureEventId;
            if (captureEventId != EventIdCaptureType.None && IncludeEventIdProperties(eventId))
            {
                // Attempt to reuse the same string-allocations based on the current <see cref="NLogProviderOptions.EventIdSeparator"/>
                var eventIdPropertyNames = _eventIdPropertyNames ?? new Tuple<string, string, string>(null, null, null);
                var eventIdSeparator = _options.EventIdSeparator ?? String.Empty;
                if (!ReferenceEquals(eventIdPropertyNames.Item1, eventIdSeparator))
                {
                    // Perform atomic cache update of the string-allocations matching the current separator
                    _eventIdPropertyNames = eventIdPropertyNames = CreateEventIdPropertyNames(eventIdSeparator);
                }

                if ((captureEventId & EventIdCaptureType.EventId_Id) != 0)
                    logEvent.Properties[eventIdPropertyNames.Item2] = eventId.Id == 0 ? ZeroEventId : GetEventId(eventId.Id);

                if ((captureEventId & EventIdCaptureType.EventId_Name) != 0 && eventId.Name != null)
                    logEvent.Properties[eventIdPropertyNames.Item3] = eventId.Name;

                if ((captureEventId & EventIdCaptureType.EventId) != 0)
                    logEvent.Properties[nameof(EventId)] = eventId;
            }
        }

        private bool IncludeEventIdProperties(EventId eventId)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return eventId != default || !_options.IgnoreEmptyEventId;
#else
            return eventId.Id != 0 || !string.IsNullOrEmpty(eventId.Name) || !_options.IgnoreEmptyEventId; 
#endif
        }

        private static Tuple<string, string, string> CreateEventIdPropertyNames(string eventIdSeparator)
        {
            var eventIdPropertyNames = new Tuple<string, string, string>(
                eventIdSeparator,
                String.Concat("EventId", eventIdSeparator, "Id"),
                String.Concat("EventId", eventIdSeparator, "Name"));
            return eventIdPropertyNames;
        }

        private static object GetEventId(int eventId)
        {
            if (eventId >= 0 && eventId < EventIdBoxing.Length)
                return EventIdBoxing[eventId];
            else
                return eventId;
        }

        private static void CaptureMessageProperties(LogEventInfo logEvent, IEnumerable<KeyValuePair<string, object>> messageProperties)
        {
            if (messageProperties != null)
            {
                foreach (var property in messageProperties)
                {
                    if (String.IsNullOrEmpty(property.Key))
                        continue;

                    logEvent.Properties[property.Key] = property.Value;
                }
            }
        }

        private static void CaptureMessagePropertiesList(LogEventInfo logEvent, IReadOnlyList<KeyValuePair<string, object>> messageProperties)
        {
            var messagePropertyCount = messageProperties.Count;
            for (int i = 0; i < messagePropertyCount; ++i)
            {
                var property = messageProperties[i];
                if (String.IsNullOrEmpty(property.Key))
                    continue;

                if (i == messagePropertyCount - 1 && OriginalFormatPropertyName.Equals(property.Key))
                    continue;

                logEvent.Properties[property.Key] = property.Value;
            }
        }

        /// <summary>
        /// Is logging enabled for this logger at this <paramref name="logLevel"/>?
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return IsLogLevelEnabled(logLevel);
        }

        private bool IsLogLevelEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return _logger.IsTraceEnabled;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return _logger.IsDebugEnabled;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return _logger.IsInfoEnabled;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return _logger.IsWarnEnabled;
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return _logger.IsErrorEnabled;
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return _logger.IsFatalEnabled;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return LoggerName;
        }

        /// <summary>
        /// Convert log level to NLog variant.
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

        /// <summary>
        /// Begin a scope. Use in config with ${scopeproperty} or ${scopenested}
        /// </summary>
        /// <param name="state">The state (message)</param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            if (!_options.IncludeScopes || state == null)
            {
                return NullScope.Instance;
            }

            try
            {
                return _beginScopeParser.ParseBeginScope(state) ?? NullScope.Instance;
            }
            catch (Exception ex)
            {
                Common.InternalLogger.Debug(ex, "Exception in BeginScope");
                return NullScope.Instance;
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();

            private NullScope()
            {
            }

            /// <inheritdoc />
            public void Dispose()
            {
                // Nothing to do
            }
        }
    }
}
