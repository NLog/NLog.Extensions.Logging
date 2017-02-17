using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_library_using_ms_logging
{
    /// <summary>
    /// Represents an external library which logs messages via the generic Microsoft.Extensions.Logging
    /// </summary>
    public class ExampleLibrary
    {
        private Lazy<ILogger> _logger = new Lazy<ILogger>(() => ExampleLibraryLoggerFactory.Instance.CreateLogger<ExampleLibrary>());
        private ILogger Logger { get { return _logger.Value; } }

        /// <summary>
        /// Simulates a long-running task which logs various messages during its lifetime.
        /// </summary>
        /// <returns></returns>
        public async Task DoWorkAsync()
        {
            Logger.LogInformation("Starting work.");

            for (int i = 0; i <= 100; i += 20)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                Logger.LogDebug($"Completed {i}% of work");
            }

            Logger.LogInformation("Finished work.");
        }
    }
}
