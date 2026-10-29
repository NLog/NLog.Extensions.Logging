using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Hosting.Tests
{
    public class HostLayoutRendererTests
    {
        [Fact]
        public void UseNLog_RendersHostEnvironmentValues()
        {
            // Arrange
            var contentRootPath = Path.GetTempPath();
            var hostConfiguration = new Dictionary<string, string>
            {
                [HostDefaults.ApplicationKey] = "TestApplication",
                [HostDefaults.EnvironmentKey] = "TestEnvironment",
                [HostDefaults.ContentRootKey] = contentRootPath
            };
            var target = new Targets.MemoryTarget
            {
                Layout = "${host-appname}|${host-rootdir}|${host-environment}"
            };
            var host = new HostBuilder()
                .ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(hostConfiguration))
                .UseNLog(null, serviceProvider =>
                {
                    var logFactory = new LogFactory();
                    logFactory.Setup().SetupExtensions(extensions =>
                    {
                        extensions.RegisterServiceProvider(serviceProvider);
                        extensions.RegisterHostSettings(serviceProvider.GetRequiredService<IConfiguration>());
                    });
                    logFactory.Setup().LoadConfiguration(configuration => configuration.ForLogger().WriteTo(target));
                    return logFactory;
                })
                .Build();

            try
            {
                // Act
                var loggerProvider = host.Services.GetService<ILoggerProvider>() as NLogLoggerProvider;
                loggerProvider.LogFactory.GetLogger("test").Info("test");

                // Assert
                Assert.Equal($"TestApplication|{contentRootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)}|TestEnvironment", Assert.Single(target.Logs));
            }
            finally
            {
                host.Dispose();
            }
        }
    }
}
