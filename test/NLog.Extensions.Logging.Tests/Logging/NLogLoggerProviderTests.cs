using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Logging.Tests.Logging
{
    public class NLogLoggerProviderTests : NLogTestBase
    {
        [Fact]
        public void CreateLogger_HappyPath_LoggerWithCorrectName()
        {
            // Arrange
            var unitUnderTest = new NLogLoggerProvider();
            string name = "namespace.class1";

            // Act
            var result = unitUnderTest.CreateLogger(name);

            // Assert
            Assert.NotNull(result);
            var logger = Assert.IsType<NLogLogger>(result);
            Assert.Equal(name, logger.LoggerName);
        }

        [Fact]
        public void Dispose_HappyPath_FlushLogFactory()
        {
            // Arrange
            var logFactory = new LogFactory();
            var logConfig = new Config.LoggingConfiguration(logFactory);
            logConfig.AddTarget(new Targets.MemoryTarget("output"));
            logConfig.AddRuleForAllLevels(new Targets.Wrappers.BufferingTargetWrapper("buffer", logConfig.FindTargetByName("output")));
            logFactory.Configuration = logConfig;
            var provider = new NLogLoggerProvider(null, logFactory);

            // Act
            provider.CreateLogger("test").LogInformation("Hello");
            provider.Dispose();

            // Assert
            Assert.Single(logFactory.Configuration.FindTargetByName<Targets.MemoryTarget>("output").Logs);
        }

    #if NETSTANDARD2_1_OR_GREATER || NET
        [Fact]
        public async Task DisposeAsync_ShutdownOnDispose_FlushesBufferedLog()
        {
            // Arrange
            var logFactory = new LogFactory();
            var logConfig = new Config.LoggingConfiguration(logFactory);
            var memoryTarget = new Targets.MemoryTarget("output");
            var target = new Targets.Wrappers.BufferingTargetWrapper("buffer", memoryTarget);
            logConfig.AddRuleForAllLevels(target);
            logFactory.Configuration = logConfig;
            var options = new NLogProviderOptions { ShutdownOnDispose = true };
            var provider = new NLogLoggerProvider(options, logFactory);

            // Act
            provider.CreateLogger("test").LogInformation("Hello");
            await provider.DisposeAsync();

            // Assert
            Assert.Single(memoryTarget.Logs);
        }
#endif
    }
}
