using System;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Framework.Logging
{
    /// <summary>
    /// Wrap NLog's Logger in a Microsoft.Extensions.Logging's interface <see cref="Microsoft.Extensions.Logging.ILogger"/>.
    /// </summary>
    internal class NLogLogger : Microsoft.Extensions.Logging.ILogger
    {
        private readonly Logger _logger;

        public NLogLogger(Logger logger)
        {
            _logger = logger;
        }

        //todo  callsite showing the framework logging classes/methods

        public void Log(Microsoft.Extensions.Logging.LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            var nLogLogLevel = ConvertLogLevel(logLevel);
            if (IsEnabled(nLogLogLevel))
            {
                string message;
                if (formatter != null)
                {
                    message = formatter(state, exception);
                }
                else
                {
                    message = Microsoft.Extensions.Logging.LogFormatter.Formatter(state, exception);
                }
                if (!string.IsNullOrEmpty(message))
                {

                    //message arguments are not needed as it is already checked that the loglevel is enabled.
                    var eventInfo = LogEventInfo.Create(nLogLogLevel, _logger.Name, message);
                    eventInfo.Exception = exception;
                    eventInfo.Properties["EventId"] = eventId;
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
            //note in RC2 verbose = trace
            //https://github.com/aspnet/Logging/pull/314
            switch (logLevel)
            {
                
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    //note in RC1 trace is verbose is lower then Debug
                    return LogLevel.Trace;
                case Microsoft.Extensions.Logging.LogLevel.Verbose:
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

        
        public IDisposable BeginScopeImpl(object state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            //TODO not working with async
            return NestedDiagnosticsContext.Push(state.ToString());
        }
    }
}