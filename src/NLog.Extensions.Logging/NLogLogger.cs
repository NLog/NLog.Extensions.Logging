using System;
using Microsoft.Extensions.Logging;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Wrap NLog's Logger in a Microsoft.Extensions.Logging's interface <see cref="Microsoft.Extensions.Logging.ILogger"/>.
    /// </summary>
    internal class NLogLogger : Microsoft.Extensions.Logging.ILogger
    {
        private readonly Logger _logger;
        private readonly NLogProviderOptions _options;

        private static readonly object _emptyEventId = default(EventId);    // Cache boxing of empty EventId-struct
        private static readonly object _zeroEventId = default(EventId).Id;  // Cache boxing of zero EventId-Value
        private Tuple<string, string, string> _eventIdPropertyNames;

        public NLogLogger(Logger logger, NLogProviderOptions options)
        {
            _logger = logger;
            _options = options ?? NLogProviderOptions.Default;
        }

        //todo  callsite showing the framework logging classes/methods
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var nLogLogLevel = ConvertLogLevel(logLevel);
            if (IsEnabled(nLogLogLevel))
            {
                if (formatter == null)
                {
                    throw new ArgumentNullException(nameof(formatter));
                }
                var message = formatter(state, exception);

                //message arguments are not needed as it is already checked that the loglevel is enabled.
                var eventInfo = LogEventInfo.Create(nLogLogLevel, _logger.Name, message);
                eventInfo.Exception = exception;
                if (!_options.IgnoreEmptyEventId || eventId.Id != 0 || !string.IsNullOrEmpty(eventId.Name))
                {
                    /// Attempt to reuse the same string-allocations based on the current <see cref="NLogProviderOptions.EventIdSeparator"/>
                    var eventIdPropertyNames = _eventIdPropertyNames ?? new Tuple<string, string, string>(null, null, null);
                    var eventIdSeparator = _options.EventIdSeparator ?? string.Empty;
                    if (!ReferenceEquals(eventIdPropertyNames.Item1, eventIdSeparator))
                    {
                        // Perform atomic cache update of the string-allocations matching the current separator
                        eventIdPropertyNames = new Tuple<string, string, string>(
                            eventIdSeparator,
                            string.Concat("EventId", eventIdSeparator, "Id"),
                            string.Concat("EventId", eventIdSeparator, "Name"));
                        _eventIdPropertyNames = eventIdPropertyNames;
                    }
                    
                    eventInfo.Properties[eventIdPropertyNames.Item2] = eventId.Id == 0 ? _zeroEventId : eventId.Id;
                    eventInfo.Properties[eventIdPropertyNames.Item3] = eventId.Name;
                    eventInfo.Properties["EventId"] = (eventId.Id == 0 && eventId.Name == null) ? _emptyEventId : eventId;
                }
                _logger.Log(eventInfo);
                
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

        /// <summary>
        /// Begin a scope. Log in config with ${ndc} 
        /// TODO not working with async
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            
            return NestedDiagnosticsLogicalContext.Push(state);
        }
    }
}
