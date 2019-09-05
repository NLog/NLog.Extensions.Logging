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
    }
}
