using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Common;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Creating DI loggers for Microsoft.Extensions.Logging and NLog
    /// </summary>
    public class NLogLoggerFactory : ILoggerFactory
    {
        private readonly IConfiguration _configuration; //todo use
        private readonly NLogLoggerProvider _provider;

        /// <summary>
        /// New factory with default options
        /// </summary>
        public NLogLoggerFactory(IConfiguration configuration)
        {
            _configuration = configuration;
            _provider = new NLogLoggerProvider();
        }

        /// <summary>
        /// New factory with options. 
        /// </summary>
        public NLogLoggerFactory(NLogProviderOptions options, IConfiguration configuration)
        {
            _provider = new NLogLoggerProvider(options);
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
                LogManager.Flush();
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
            return _provider.CreateLogger(categoryName);
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