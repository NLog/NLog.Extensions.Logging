﻿using System;
using System.Collections.Generic;
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

                if (!string.IsNullOrEmpty(message))
                {
                    //message arguments are not needed as it is already checked that the loglevel is enabled.
                    var eventInfo = LogEventInfo.Create(nLogLogLevel, _logger.Name, message);
                    eventInfo.Exception = exception;
                    eventInfo.Properties["EventId" + _options.EventIdSeparator + "Id"] = eventId.Id;
                    eventInfo.Properties["EventId" + _options.EventIdSeparator + "Name"] = eventId.Name;
                    eventInfo.Properties["EventId"] = eventId;


                    // Extract all arguments passed in state and add them as event properties
                    var metadata = state as IEnumerable<KeyValuePair<string, object>>;
                    foreach (var item in metadata)
                    {
                        eventInfo.Properties[item.Key] = item.Value;
                    }

                    _logger.Log(eventInfo);
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
            
            return NestedDiagnosticsContext.Push(state);
        }
    }
}
