namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Provider logger for NLog.
    /// </summary>
    public class NLogLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
    {
        /// <summary>
        /// NLog options
        /// </summary>
        public NLogProviderOptions Options { get; set; }

        /// <summary>
        /// <see cref="NLogLoggerProvider"/> with default options.
        /// </summary>
        public NLogLoggerProvider() 
        {
        }

        /// <summary>
        /// <see cref="NLogLoggerProvider"/> with default options.
        /// </summary>
        /// <param name="options"></param>
        public NLogLoggerProvider(NLogProviderOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Create a logger with the name <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the logger to be created.</param>
        /// <returns>New Logger</returns>
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string name)
        {
            return new NLogLogger(LogManager.GetLogger(name), Options);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
        }
    }
}


