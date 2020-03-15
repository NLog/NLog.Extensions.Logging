using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NLog.Common;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Creating DI loggers for Microsoft.Extensions.Logging and NLog
    /// </summary>
    public class NLogLoggerFactory : ILoggerFactory
    {
        private readonly Dictionary<string, Microsoft.Extensions.Logging.ILogger> _loggers = new Dictionary<string, Microsoft.Extensions.Logging.ILogger>(StringComparer.Ordinal);

        private readonly NLogLoggerProvider _provider;

        /// <summary>
        /// New factory with default options
        /// </summary>
        public NLogLoggerFactory()
            :this(new NLogLoggerProvider())
        {
        }

        /// <summary>
        /// New factory with options. 
        /// </summary>
        /// <param name="options"></param>
        public NLogLoggerFactory(NLogProviderOptions options)
            :this(new NLogLoggerProvider(options))
        {
        }

        /// <summary>
        /// New factory with provider. 
        /// </summary>
        /// <param name="provider"></param>
        public NLogLoggerFactory(NLogLoggerProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _provider.Dispose();
            }
        }

        #region Implementation of ILoggerFactory

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Extensions.Logging.ILogger" /> instance.
        /// </summary>
        /// <param name="categoryName">The logger name for messages produced by the logger.</param>
        /// <returns>The <see cref="T:Microsoft.Extensions.Logging.ILogger" />.</returns>
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
           if (_loggers.TryGetValue(categoryName, out var logger))
            {
                return logger;
            }
            else
            {
                lock (_loggers)
                {
                    if (!_loggers.TryGetValue(categoryName, out logger))
                    {
                        logger = _provider.CreateLogger(categoryName);
                        _loggers[categoryName] = logger;
                    }
                    return logger;
                }
            }
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="provider">The <see cref="T:Microsoft.Extensions.Logging.ILoggerProvider" />.</param>
        public void AddProvider(ILoggerProvider provider)
        {
            InternalLogger.Debug("AddProvider will be ignored");
        }

        #endregion
    }
}
