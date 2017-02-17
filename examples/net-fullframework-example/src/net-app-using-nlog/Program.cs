using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using net_library_using_ms_logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_app_using_nlog
{
    /// <summary>
    /// Represents an application which injects NLog for logging, and wishes to log messages coming from an external library which uses Microsoft.Extensions.Logging.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //Set up DI
            Autofac.IContainer _container = BuildContainer();
            ConfigureServices(_container);

            //Accomplish something using our example service which depends on an external library
            //Resolve the service with dependencies injected - e.g. loggers
            using (var scope = _container.BeginLifetimeScope())
            {
                IExampleService service = scope.Resolve<IExampleService>();
                service.DoSomethingAsync().GetAwaiter().GetResult();
            }

            //Wait to exit
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Constructs an application container 
        /// </summary>
        /// <returns>An application container populated with desired types and services</returns>
        static Autofac.IContainer BuildContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();

            //Register local types to be resolved
            builder.RegisterType<ExampleService>().AsImplementedInterfaces();
            builder.RegisterType<ExampleWorker>();

            //Register logging services for resolution
            ServiceCollection services = new ServiceCollection();
            services.AddLogging();
            builder.Populate(services);

            return builder.Build();
        }

        /// <summary>
        /// Perform any desired configuration on container services
        /// </summary>
        /// <param name="container">The application container to be configured</param>
        static void ConfigureServices(Autofac.IContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                //Add NLog as a log consumer
                ILoggerFactory loggerFactory = scope.Resolve<ILoggerFactory>();
                loggerFactory.AddNLog(); //notice: the project's only line of code referencing NLog (aside from .config)
            }
        }
    }
}
