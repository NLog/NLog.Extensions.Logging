using System;
using Microsoft.Extensions.Logging;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Provider logger for NLog + Microsoft.Extensions.Logging
    /// </summary>
    [ProviderAlias("NLog")]
    public class NLogLoggerProvider : ILoggerProvider
#if NETSTANDARD2_1_OR_GREATER || NET
        , IAsyncDisposable
#endif
    {
        private readonly NLogBeginScopeParser _beginScopeParser;

        /// <summary>
        /// NLog options
        /// </summary>
        public NLogProviderOptions Options { get; set; }

        /// <summary>
        /// NLog Factory
        /// </summary>
        public LogFactory LogFactory { get; }

        /// <summary>
        /// New provider with default options, see <see cref="Options"/>
        /// </summary>
        public NLogLoggerProvider()
            : this(NLogProviderOptions.Default)
        {
        }

        /// <summary>
        /// New provider with options
        /// </summary>
        /// <param name="options"></param>
        public NLogLoggerProvider(NLogProviderOptions options)
            : this(options, LogManager.LogFactory)
        {
        }

        /// <summary>
        /// New provider with options
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logFactory">Optional isolated NLog LogFactory</param>
        public NLogLoggerProvider(NLogProviderOptions options, LogFactory logFactory)
        {
            LogFactory = logFactory ?? LogManager.LogFactory;
            Options = options ?? NLogProviderOptions.Default;
            _beginScopeParser = new NLogBeginScopeParser(Options);
        }

        /// <summary>
        /// Create a logger with the name <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the logger to be created.</param>
        /// <returns>New Logger</returns>
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string name)
        {
            return new NLogLogger(LogFactory.GetLogger(name), Options, _beginScopeParser);
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
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Options.ShutdownOnDispose)
                {
                    LogFactory.Shutdown();
                }
                else
                {
                    LogFactory.Flush();
                }
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NET
        /// <summary>
        /// Cleanup
        /// </summary>
        public async System.Threading.Tasks.ValueTask DisposeAsync()
        {
            if (Options.ShutdownOnDispose)
            {
                await LogFactory.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                await LogFactory.FlushAsync(System.Threading.CancellationToken.None).ConfigureAwait(false);
            }
            GC.SuppressFinalize(this);
        }
#endif
    }
}


