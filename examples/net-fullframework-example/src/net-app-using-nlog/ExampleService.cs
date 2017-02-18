using Microsoft.Extensions.Logging;
using net_library_using_ms_logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_app_using_nlog
{
    class ExampleService : IExampleService
    {
        private readonly ExampleWorker _exampleWorker;
        private readonly ILogger _logger;

        public ExampleService(ExampleWorker exampleWorker, ILogger<ExampleService> logger = null) //keeping exampleWorker concrete here for example simplicity...
        {
            _exampleWorker = exampleWorker;
            _logger = logger;
        }

        public async Task DoSomethingAsync()
        {
            _logger?.LogInformation("Doing something!");

            await _exampleWorker.DoWorkAsync();

            _logger?.LogInformation("Did something!");
        }
    }
}
