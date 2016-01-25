using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog;

namespace NLog.Framework.Logging
{
    public class NLogLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
    {

        public NLogLoggerProvider()
        {
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string name)
        {
            return new NLogLogger(LogManager.GetLogger(name));
        }

        public void Dispose()
        {
        }
    }
}


