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
        private static readonly object EmptyEventId = default(EventId);    // Cache boxing of empty EventId-struct
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
            var nLogLogLevel = ConvertLogLevel(logLevel);
            if (!_logger.IsEnabled(nLogLogLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            LogEventInfo eventInfo = CreateLogEventInfo(nLogLogLevel, state, exception, formatter);

            CaptureEventId(eventInfo, eventId);

            if (exception != null)
            {
                eventInfo.Exception = exception;
            }

            _logger.Log(typeof(Microsoft.Extensions.Logging.ILogger), eventInfo);
        }

        private LogEventInfo CreateLogEventInfo<TState>(LogLevel nLogLogLevel, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var messageProperties = (_options.CaptureMessageTemplates || _options.CaptureMessageProperties)
                ? state as IReadOnlyList<KeyValuePair<string, object>>
                : null;

            LogEventInfo eventInfo =
                TryParseMessageTemplate(nLogLogLevel, messageProperties, out var messageParameters) ??
                CreateLogEventInfo(nLogLogLevel, formatter(state, exception), messageProperties, messageParameters);

            if (messageParameters == null && messageProperties == null && _options.CaptureMessageProperties)
            {
                CaptureMessageProperties(eventInfo, state as IEnumerable<KeyValuePair<string, object>>);
            }

            return eventInfo;
        }

        private LogEventInfo CreateLogEventInfo(LogLevel nLogLogLevel, string formattedMessage, IReadOnlyList<KeyValuePair<string, object>> messageProperties, NLogMessageParameterList messageParameters)
        {
            return TryCaptureMessageTemplate(nLogLogLevel, formattedMessage, messageProperties, messageParameters) ??
                CreateSimpleLogEventInfo(nLogLogLevel, formattedMessage, messageProperties, messageParameters);
        }

        /// <summary>
        /// Checks if the already parsed input message-parameters must be sent through
        /// the NLog MessageTemplate Parser for proper handling of message-template syntax.
        /// </summary>
        /// <remarks>
        /// Using the NLog MessageTemplate Parser will hurt performance: 1 x Microsoft Parser - 2 x NLog Parser - 1 x NLog Formatter
        /// </remarks>
        private LogEventInfo TryParseMessageTemplate(LogLevel nLogLogLevel, IReadOnlyList<KeyValuePair<string, object>> messageProperties, out NLogMessageParameterList messageParameters)
        {
            messageParameters = TryParseMessageParameterList(messageProperties);

            if (messageParameters?.HasMessageTemplateSyntax(_options.ParseMessageTemplates)==true)
            {
                var originalMessage = messageParameters.GetOriginalMessage(messageProperties);
                var eventInfo = new LogEventInfo(nLogLogLevel, _logger.Name, null, originalMessage, SingleItemArray);
                var messageTemplateParameters = eventInfo.MessageTemplateParameters;   // Forces parsing of OriginalMessage
                if (messageTemplateParameters.Count > 0)
                {
                    // We have parsed the message and found parameters, now we need to do the parameter mapping
                    eventInfo.Parameters = CreateLogEventInfoParameters(messageParameters, messageTemplateParameters, out var extraProperties);
                    AddExtraPropertiesToLogEvent(eventInfo, extraProperties);
                    return eventInfo;
                }

                return null;    // Parsing not possible
            }

            return null;    // Parsing not needed
        }

        /// <summary>
        /// Convert IReadOnlyList to <see cref="NLogMessageParameterList"/>
        /// </summary>
        /// <param name="messageProperties"></param>
        /// <returns></returns>
        private NLogMessageParameterList TryParseMessageParameterList(IReadOnlyList<KeyValuePair<string, object>> messageProperties)
        {
            return (messageProperties != null && _options.CaptureMessageTemplates)
                ? NLogMessageParameterList.TryParse(messageProperties)
                : null;
        }

        /// <summary>
        /// Append extra property on <paramref name="eventInfo"/>
        /// </summary>
        private static void AddExtraPropertiesToLogEvent(LogEventInfo eventInfo, List<MessageTemplateParameter> extraProperties)
        {
            if (extraProperties?.Count > 0)
            {
                // Need to harvest additional parameters
                foreach (var property in extraProperties)
                    eventInfo.Properties[property.Name] = property.Value;
            }
        }

        private LogEventInfo TryCaptureMessageTemplate(LogLevel nLogLogLevel, string message, IReadOnlyList<KeyValuePair<string, object>> messageProperties, NLogMessageParameterList messageParameters)
        {
            if (messageParameters?.HasComplexParameters == false)
            {
                // Parsing not needed, we take the fast route 
                var originalMessage = messageParameters.GetOriginalMessage(messageProperties);
                var eventInfo = new LogEventInfo(nLogLogLevel, _logger.Name, originalMessage ?? message, messageParameters.IsPositional ? EmptyParameterArray : messageParameters);
                if (originalMessage != null)
                {
                    SetLogEventMessageFormatter(eventInfo, messageParameters, message);
                }
                return eventInfo;
            }
            return null;
        }

        private LogEventInfo CreateSimpleLogEventInfo(LogLevel nLogLogLevel, string message, IReadOnlyList<KeyValuePair<string, object>> messageProperties, NLogMessageParameterList messageParameters)
        {
            // Parsing failed or no messageParameters
            var eventInfo = LogEventInfo.Create(nLogLogLevel, _logger.Name, message);
            if (messageParameters != null)
            {
                for (int i = 0; i < messageParameters.Count; ++i)
                {
                    var property = messageParameters[i];
                    eventInfo.Properties[property.Name] = property.Value;
                }
            }
            else if (messageProperties != null && _options.CaptureMessageProperties)
            {
                CaptureMessagePropertiesList(eventInfo, messageProperties);
            }
            return eventInfo;
        }

        /// <summary>
        /// Allocates object[]-array for <see cref="LogEventInfo.Parameters"/> after checking
        /// for mismatch between Microsoft Extension Logging and NLog Message Template Parser
        /// </summary>
        /// <remarks>
        /// Cannot trust the parameters received from Microsoft Extension Logging, as extra parameters can be injected
        /// </remarks>
        private static object[] CreateLogEventInfoParameters(NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters, out List<MessageTemplateParameter> extraProperties)
        {
            if (AllParameterCorrectlyPositionalMapped(messageParameters, messageTemplateParameters))
            {
                // Everything is mapped correctly, inject messageParameters directly as params-array
                extraProperties = null;
                var paramsArray = new object[messageTemplateParameters.Count];
                for (int i = 0; i < paramsArray.Length; ++i)
                    paramsArray[i] = messageParameters[i].Value;
                return paramsArray;
            }
            else
            {
                // Resolves mismatch between the input from Microsoft Extension Logging TState and NLog Message Template Parser
                if (messageTemplateParameters.IsPositional)
                {
                    return CreatePositionalLogEventInfoParameters(messageParameters, messageTemplateParameters, out extraProperties);
                }
                else
                {
                    return CreateStructuredLogEventInfoParameters(messageParameters, messageTemplateParameters, out extraProperties);
                }
            }
        }

        private static readonly object[] SingleItemArray = { null };
        private static readonly IList<MessageTemplateParameter> EmptyParameterArray = new MessageTemplateParameter[] { };

        /// <summary>
        /// Are all parameters positional and correctly mapped?
        /// </summary>
        /// <param name="messageParameters"></param>
        /// <param name="messageTemplateParameters"></param>
        /// <returns>true if correct</returns>
        private static bool AllParameterCorrectlyPositionalMapped(NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters)
        {
            if (messageTemplateParameters.Count != messageParameters.Count || messageTemplateParameters.IsPositional)
            {
                return false;
            }

            for (int i = 0; i < messageTemplateParameters.Count; ++i)
            {
                if (messageTemplateParameters[i].Name != messageParameters[i].Name)
                {
                    return false;
                }
            }

            return true;
        }

        private static object[] CreateStructuredLogEventInfoParameters(NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters, out List<MessageTemplateParameter> extraProperties)
        {
            extraProperties = null;

            var paramsArray = new object[messageTemplateParameters.Count];
            int startPos = 0;
            for (int i = 0; i < messageParameters.Count; ++i)
            {
                bool extraProperty = true;
                for (int j = startPos; j < messageTemplateParameters.Count; ++j)
                {
                    if (messageParameters[i].Name == messageTemplateParameters[j].Name)
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

        private static object[] CreatePositionalLogEventInfoParameters(NLogMessageParameterList messageParameters, MessageTemplateParameters messageTemplateParameters, out List<MessageTemplateParameter> extraProperties)
        {
            extraProperties = null;

            var maxIndex = FindMaxIndex(messageTemplateParameters);
            object[] paramsArray = null;
            for (int i = 0; i < messageParameters.Count; ++i)
            {
                // First positional name is the startPos
                if (char.IsDigit(messageParameters[i].Name[0]) && paramsArray == null)
                {
                    paramsArray = new object[maxIndex + 1];
                    for (int j = 0; j <= maxIndex; ++j)
                    {
                        if (i + j < messageParameters.Count)
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
        private static List<MessageTemplateParameter> AddExtraProperty(List<MessageTemplateParameter> extraProperties, MessageTemplateParameter item)
        {
            extraProperties = extraProperties ?? new List<MessageTemplateParameter>();
            extraProperties.Add(item);
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
                    maxIndex = Math.Max(maxIndex, int.Parse(messageTemplateParameters[i].Name));
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

        private void CaptureEventId(LogEventInfo eventInfo, EventId eventId)
        {
            if (_options.CaptureMessageProperties && (!_options.IgnoreEmptyEventId || eventId.Id != 0 || !String.IsNullOrEmpty(eventId.Name)))
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
                var eventIdObj = idIsZero ? ZeroEventId : GetEventId(eventId.Id);
                eventInfo.Properties[eventIdPropertyNames.Item2] = eventIdObj;
                if (_options.CaptureEntireEventId)
                {
                    eventInfo.Properties[eventIdPropertyNames.Item3] = eventId.Name;
                    eventInfo.Properties["EventId"] = idIsZero && eventId.Name == null ? EmptyEventId : eventId;
                }
                else if (!string.IsNullOrEmpty(eventId.Name))
                {
                    eventInfo.Properties[eventIdPropertyNames.Item3] = eventId.Name;
                }
            }
        }

        private static object GetEventId(int eventId)
        {
            if (eventId >= 0 && eventId < EventIdBoxing.Length)
                return EventIdBoxing[eventId];
            else
                return eventId;
        }

        private static Tuple<string, string, string> CreateEventIdPropertyNames(string eventIdSeparator)
        {
            var eventIdPropertyNames = new Tuple<string, string, string>(
                eventIdSeparator,
                String.Concat("EventId", eventIdSeparator, "Id"),
                String.Concat("EventId", eventIdSeparator, "Name"));
            return eventIdPropertyNames;
        }

        private void CaptureMessageProperties(LogEventInfo eventInfo, IEnumerable<KeyValuePair<string, object>> messageProperties)
        {
            if (messageProperties != null)
            {
                foreach (var property in messageProperties)
                {
                    if (String.IsNullOrEmpty(property.Key))
                        continue;

                    eventInfo.Properties[property.Key] = property.Value;
                }
            }
        }

        private void CaptureMessagePropertiesList(LogEventInfo eventInfo, IReadOnlyList<KeyValuePair<string, object>> messageProperties)
        {
            for (int i = 0; i < messageProperties.Count; ++i)
            {
                var property = messageProperties[i];
                if (String.IsNullOrEmpty(property.Key))
                    continue;

                if (i == messageProperties.Count - 1 && OriginalFormatPropertyName.Equals(property.Key))
                    continue;

                eventInfo.Properties[property.Key] = property.Value;
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
        /// Begin a scope. Use in config with ${ndlc} 
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
