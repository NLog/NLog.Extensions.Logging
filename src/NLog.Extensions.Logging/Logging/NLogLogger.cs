﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NLog.Common;
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

            var messageParameters = NLogMessageParameterList.TryParse(_options.CaptureMessageTemplates ? state as IReadOnlyList<KeyValuePair<string, object>> : null);

            LogEventInfo eventInfo =
                TryParseLogEventInfo(nLogLogLevel, messageParameters) ??
                CreateLogEventInfo(nLogLogLevel, formatter(state, exception), messageParameters);
                
            if (exception != null)
            {
                eventInfo.Exception = exception;
            }

            CaptureEventId(eventInfo, eventId);

            if (messageParameters == null)
            {
                CaptureMessageProperties(eventInfo, state);
            }

            _logger.Log(typeof(Microsoft.Extensions.Logging.ILogger), eventInfo);
        }


        private LogEventInfo CreateLogEventInfo(LogLevel nLogLogLevel, string message, NLogMessageParameterList messageParameters)
        {
            if (messageParameters?.HasComplexParameters == false)
            {
                // Parsing not needed, we take the fast route 
                var originalMessage = messageParameters.OriginalMessage as string;
                var eventInfo = new LogEventInfo(nLogLogLevel, _logger.Name, originalMessage ?? message, messageParameters.IsPositional ? _emptyParameterArray : messageParameters);
                if (originalMessage != null)
                {
                    SetLogEventMessageFormatter(eventInfo, messageParameters, message);
                }
                return eventInfo;
            }
            else
            {
                // Parsing failed or no messageParameters
                var eventInfo = LogEventInfo.Create(nLogLogLevel, _logger.Name, message);
                if (messageParameters?.Count > 0)
                {
                    for (int i = 0; i < messageParameters.Count; ++i)
                    {
                        var property = messageParameters[i];
                        eventInfo.Properties[property.Name] = property.Value;
                    }
                }
                return eventInfo;
            }
        }

        /// <summary>
        /// Checks if the already parsed input message-parameters must be sent through
        /// the NLog MessageTemplate Parser for proper handling of message-template syntax.
        /// </summary>
        /// <remarks>
        /// Using the NLog MesageTemplate Parser will hurt performance: 1 x Microsoft Parser - 2 x NLog Parser - 1 x NLog Formatter
        /// </remarks>
        private LogEventInfo TryParseLogEventInfo(LogLevel nLogLogLevel, NLogMessageParameterList messageParameters)
        {
            if (messageParameters?.OriginalMessage != null && (messageParameters.HasComplexParameters || (_options.ParseMessageTemplates && messageParameters.Count > 0)))
            {
                // NLog MessageTemplate Parser must be used
                var eventInfo = new LogEventInfo(nLogLogLevel, _logger.Name, null, messageParameters.OriginalMessage as string, _singleItemArray);
                var messagetTemplateParameters = eventInfo.MessageTemplateParameters;   // Forces parsing of OriginalMessage
                if (messagetTemplateParameters.Count > 0)
                {
                    // We have parsed the message and found parameters, now we need to do the parameter mapping
                    eventInfo.Parameters = CreateLogEventInfoParameters(messageParameters, messagetTemplateParameters, out var extraProperties);
                    if (extraProperties?.Count > 0)
                    {
                        // Need to harvest additional parameters
                        foreach (var property in extraProperties)
                            eventInfo.Properties[property.Name] = property.Value;
                    }
                    return eventInfo;
                }

                return null;    // Parsing not possible
            }

            return null;    // Parsing not needed
        }

        /// <summary>
        /// Allocates object[]-array for <see cref="LogEventInfo.Parameters"/> after checking
        /// for mismatch between Microsoft Extension Logging and NLog Message Template Parser
        /// </summary>
        /// <remarks>
        /// Cannot trust the parameters received from Microsoft Extension Logging, as extra parameters can be injected
        /// </remarks>
        private static object[] CreateLogEventInfoParameters(NLogMessageParameterList messageParameters, MessageTemplateParameters messagetTemplateParameters, out List<MessageTemplateParameter> extraProperties)
        {
            if (AllParameterCorrectlyPositionalMapped(messageParameters, messagetTemplateParameters))
            {
                // Everything is mapped correctly, inject messageParameters directly as params-array
                extraProperties = null;
                var paramsArray = new object[messagetTemplateParameters.Count];
                for (int i = 0; i < paramsArray.Length; ++i)
                    paramsArray[i] = messageParameters[i].Value;
                return paramsArray;
            }
            else
            {
                // Resolves mismatch between the input from Microsoft Extension Logging TState and NLog Message Template Parser
                if (messagetTemplateParameters.IsPositional)
                {
                    return CreatePositionalLogEventInfoParameters(messageParameters, messagetTemplateParameters, out extraProperties);
                }
                else
                {
                    return CreateStructuredLogEventInfoParameters(messageParameters, messagetTemplateParameters, out extraProperties);
                }
            }
        }

        private static readonly object[] _singleItemArray = { null };
        private static readonly IList<MessageTemplateParameter> _emptyParameterArray = new MessageTemplateParameter[] { };

        /// <summary>
        /// Are all parameters positional and correctly mapped?
        /// </summary>
        /// <param name="messageParameters"></param>
        /// <param name="messagetTemplateParameters"></param>
        /// <returns>true if correct</returns>
        private static bool AllParameterCorrectlyPositionalMapped(NLogMessageParameterList messageParameters, MessageTemplateParameters messagetTemplateParameters)
        {
            if (messagetTemplateParameters.Count != messageParameters.Count || messagetTemplateParameters.IsPositional)
            {
                return false;
            }

            for (int i = 0; i < messagetTemplateParameters.Count; ++i)
            {
                if (messagetTemplateParameters[i].Name != messageParameters[i].Name)
                {
                    return false;
                }
            }

            return true;
        }

        private static object[] CreateStructuredLogEventInfoParameters(NLogMessageParameterList messageParameters, MessageTemplateParameters messagetTemplateParameters, out List<MessageTemplateParameter> extraProperties)
        {
            extraProperties = null;

            var paramsArray = new object[messagetTemplateParameters.Count];
            int startPos = 0;
            for (int i = 0; i < messageParameters.Count; ++i)
            {
                bool extraProperty = true;
                for (int j = startPos; j < messagetTemplateParameters.Count; ++j)
                {
                    if (messageParameters[i].Name == messagetTemplateParameters[j].Name)
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
                    extraProperties = extraProperties ?? new List<MessageTemplateParameter>();
                    extraProperties.Add(messageParameters[i]);
                }
            }

            return paramsArray;
        }

        private static object[] CreatePositionalLogEventInfoParameters(NLogMessageParameterList messageParameters, MessageTemplateParameters messagetTemplateParameters, out List<MessageTemplateParameter> extraProperties)
        {
            extraProperties = null;

            var maxIndex = FindMaxIndex(messagetTemplateParameters);
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
                    extraProperties = extraProperties ?? new List<MessageTemplateParameter>();
                    extraProperties.Add(messageParameters[i]);
                }
            }

            return paramsArray ?? new object[maxIndex + 1];
        }

        /// <summary>
        /// Find max index of the parameters
        /// </summary>
        /// <param name="messagetTemplateParameters"></param>
        /// <returns>index, 0 or higher</returns>
        private static int FindMaxIndex(MessageTemplateParameters messagetTemplateParameters)
        {
            int maxIndex = 0;
            for (int i = 0; i < messagetTemplateParameters.Count; ++i)
            {
                if (messagetTemplateParameters[i].Name.Length == 1)
                    maxIndex = Math.Max(maxIndex, messagetTemplateParameters[i].Name[0] - '0');
                else
                    maxIndex = Math.Max(maxIndex, int.Parse(messagetTemplateParameters[i].Name));
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

            /// <summary>
            /// Properties, never null and lazy init
            /// </summary>
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
