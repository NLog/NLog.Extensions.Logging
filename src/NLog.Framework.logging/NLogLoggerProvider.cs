using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog;

namespace NLog.Framework.Logging
{
    /// <summary>
    /// Provider logger for NLog.
    /// </summary>
    public class NLogLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
    {
        /// <summary>
        /// <see cref="NLogLoggerProvider"/> with default options.
        /// </summary>
        public NLogLoggerProvider()
        {
        }

        /// <summary>
        /// Create a logger with the name <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the logger to be created.</param>
        /// <returns>New Logger</returns>
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string name)
        {
            return new NLogLogger(LogManager.GetLogger(name));
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
        }
    }
}


