using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_library_using_ms_logging
{
    public static class ExampleLibraryLoggerFactory
    {
        private static Lazy<ILoggerFactory> _instance = new Lazy<ILoggerFactory>(() => new LoggerFactory());
        public static ILoggerFactory Instance { get { return _instance.Value; } }
    }
}
