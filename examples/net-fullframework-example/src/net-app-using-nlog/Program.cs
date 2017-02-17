using net_library_using_ms_logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_app_using_nlog
{
    /// <summary>
    /// Represents an application which uses NLog for logging, and wishes to log messages coming from an external library which uses Microsoft.Extensions.Logging.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Uses an external library to do some work
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task MainAsync(string[] args)
        {
            //Wire up our external library to NLog
            ExampleLibraryLoggerFactory.Instance.AddNLog();

            //Use that library
            ExampleLibrary library = new ExampleLibrary();
            await library.DoWorkAsync();

            //Wait to exit
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
