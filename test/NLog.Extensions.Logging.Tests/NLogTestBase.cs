using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Common;

namespace NLog.Extensions.Logging.Tests
{
    public class NLogTestBase
    {
        IServiceProvider _serviceProvider;

        protected IServiceProvider ConfigureServiceProvider<T>(Action<ServiceCollection> configureServices = null, NLogProviderOptions options = null) where T : class
        {
            if (_serviceProvider == null)
            {
                var services = new ServiceCollection();

                services.AddTransient<T>();
                services.AddSingleton<ILoggerFactory, LoggerFactory>();
                services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                configureServices?.Invoke(services);

                _serviceProvider = services.BuildServiceProvider();

                var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
                loggerFactory.AddNLog(options ?? new NLogProviderOptions() { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            }
            return _serviceProvider;
        }

        protected T GetRunner<T>(NLogProviderOptions options = null) where T : class
        {
            // Start program
            var runner = ConfigureServiceProvider<T>(null, options).GetRequiredService<T>();
            return runner;
        }

        protected static StringWriter CaptureInternalLog()
        {
            var stringWriter = new StringWriter();
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.LogWriter = stringWriter;
            return stringWriter;
        }

        ~NLogTestBase()
        {
            InternalLogger.LogLevel = LogLevel.Off;
            InternalLogger.LogWriter = null;
        }
    }
}
