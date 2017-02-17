using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_library_using_ms_logging
{
    /// <summary>
    /// Represents an external library method which can log messages via the generic Microsoft.Extensions.Logging
    /// </summary>
    public class ExampleWorker
    {
        private readonly ILogger _logger;

        public ExampleWorker(ILogger<ExampleWorker> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Simulates a long-running task which logs various messages during its lifetime.
        /// </summary>
        /// <returns></returns>
        public async Task DoWorkAsync()
        {
            _logger?.LogInformation("Starting work.");

            for (int i = 0; i <= 100; i += 20)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                _logger?.LogDebug($"Completed {i}% of work");
            }

            _logger?.LogInformation("Finished work.");
        }
    }
}
