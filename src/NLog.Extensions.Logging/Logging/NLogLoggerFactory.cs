using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NLog.Common;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Creating DI loggers for Microsoft.Extensions.Logging and NLog
    /// </summary>
    public class NLogLoggerFactory : ILoggerFactory
#if NETSTANDARD2_1_OR_GREATER || NET
        , IAsyncDisposable
#endif
    {
        private readonly ConcurrentDictionary<string, Microsoft.Extensions.Logging.ILogger> _loggers = new ConcurrentDictionary<string, Microsoft.Extensions.Logging.ILogger>(StringComparer.Ordinal);

        private readonly NLogLoggerProvider _provider;

        /// <summary>
        /// New factory with default options
        /// </summary>
        public NLogLoggerFactory()
            : this(NLogProviderOptions.Default)
        {
        }

        /// <summary>
        /// New factory with options. 
        /// </summary>
        /// <param name="options"></param>
        public NLogLoggerFactory(NLogProviderOptions options)
            : this(options, LogManager.LogFactory)
        {
        }

        /// <summary>
        /// New factory with options and isolated LogFactory
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logFactory"></param>
        public NLogLoggerFactory(NLogProviderOptions options, LogFactory logFactory)
            : this(new NLogLoggerProvider(options, logFactory))
        {
            RegisterNLogLoggingProvider.SetupNLogConfigSettings(null, null, _provider.LogFactory);
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
        /// Creates a new <see cref="Microsoft.Extensions.Logging.ILogger" /> instance.
        /// </summary>
        /// <param name="categoryName">The logger name for messages produced by the logger.</param>
        /// <returns>The <see cref="Microsoft.Extensions.Logging.ILogger" />.</returns>
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            if (!_loggers.TryGetValue(categoryName, out var logger))
            {
                lock (_loggers)
                {
                    if (!_loggers.TryGetValue(categoryName, out logger))
                    {
                        logger = _provider.CreateLogger(categoryName);
                        _loggers[categoryName] = logger;
                    }
                }
            }
            return logger;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="provider">The <see cref="Microsoft.Extensions.Logging.ILoggerProvider" />.</param>
        public void AddProvider(ILoggerProvider provider)
        {
            InternalLogger.Debug("NLogLoggerFactory: AddProvider has been ignored {0}", provider?.GetType());
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

#if NETSTANDARD2_1_OR_GREATER || NET
        /// <summary>
        /// Cleanup
        /// </summary>
        async System.Threading.Tasks.ValueTask IAsyncDisposable.DisposeAsync()
        {
            await _provider.DisposeAsync();
            GC.SuppressFinalize(this);
        }
#endif
    }
}
