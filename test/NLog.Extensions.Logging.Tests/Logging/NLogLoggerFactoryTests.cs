using Microsoft.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Logging.Tests.Logging
{
    public class NLogLoggerFactoryTests : NLogTestBase
    {
        [Fact]
        public void Dispose_HappyPath_FlushLogFactory()
        {
            // Arrange
            ConfigureLoggerProvider();
            ConfigureNLog(new NLog.Targets.Wrappers.BufferingTargetWrapper("buffer", new NLog.Targets.MemoryTarget("output")));
            var loggerFactory = new NLogLoggerFactory(_nlogProvider);

            // Act
            loggerFactory.CreateLogger("test").LogInformation("Hello");
            loggerFactory.Dispose();

            // Assert
            Assert.Single(_nlogProvider.LogFactory.Configuration.FindTargetByName<NLog.Targets.MemoryTarget>("output").Logs);
        }

        [Fact]
        public void CreateLogger_HappyPath_LoggerWithCorrectName()
        {
            // Arrange
            var loggerFactory = new NLogLoggerFactory();
            string loggerName = "namespace.class1";

            // Act
            var result = loggerFactory.CreateLogger(loggerName);

            // Assert
            Assert.NotNull(result);
            var logger = Assert.IsType<NLogLogger>(result);
            Assert.Equal(loggerName, logger.LoggerName);
        }

        [Fact]
        public void AddProvider_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var logFactory = new LogFactory();
            var logConfig = new NLog.Config.LoggingConfiguration(logFactory);
            logConfig.AddRuleForAllLevels(new NLog.Targets.MemoryTarget("output"));
            logFactory.Configuration = logConfig;
            var provider = new NLogLoggerProvider(null, logFactory);
            var loggerFactory = new NLogLoggerFactory(provider);

            // Act
            ILoggerProvider newProvider = new NLogLoggerProvider();
            loggerFactory.AddProvider(newProvider);
            loggerFactory.CreateLogger("test").LogInformation("Hello");

            // Assert
            Assert.Single(logFactory.Configuration.FindTargetByName<NLog.Targets.MemoryTarget>("output").Logs);
        }
    }
}
