using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Logging.Tests.Logging
{
    public class NLogLoggerFactoryTests
    {
        [Fact]
        public void Dispose_HappyPath_FlushLogFactory()
        {
            // Arrange
            var logFactory = new LogFactory();
            var logConfig = new Config.LoggingConfiguration(logFactory);
            var target = new Targets.Wrappers.BufferingTargetWrapper("buffer", new Targets.MemoryTarget("output"));
            logConfig.AddRuleForAllLevels(target);
            logFactory.Configuration = logConfig;
            var provider = new NLogLoggerProvider(new NLogProviderOptions(), logFactory);
            var loggerFactory = new NLogLoggerFactory(provider);

            // Act
            loggerFactory.CreateLogger("test").LogInformation("Hello");
            loggerFactory.Dispose();

            // Assert
            Assert.Single(logFactory.Configuration.AllTargets.OfType<Targets.MemoryTarget>().FirstOrDefault().Logs);
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
            var logConfig = new Config.LoggingConfiguration(logFactory);
            logConfig.AddRuleForAllLevels(new Targets.MemoryTarget("output"));
            logFactory.Configuration = logConfig;
            var provider = new NLogLoggerProvider(null, logFactory);
            var loggerFactory = new NLogLoggerFactory(provider);

            // Act
            ILoggerProvider newProvider = new NLogLoggerProvider();
            loggerFactory.AddProvider(newProvider);
            loggerFactory.CreateLogger("test").LogInformation("Hello");

            // Assert
            Assert.Single(logFactory.Configuration.FindTargetByName<Targets.MemoryTarget>("output").Logs);
        }

        [Fact]
        public void CreateLogger_SameName_ReturnsSameInstanceTest()
        {
            // Arrange
            var loggerFactory = new NLogLoggerFactory();
            string loggerName = "namespace.class1";

            // Act
            var result1 = loggerFactory.CreateLogger(loggerName);
            var result2 = loggerFactory.CreateLogger(loggerName);

            // Assert
            Assert.NotNull(result1);
            Assert.Equal(loggerName, result1.ToString());
            Assert.Same(result1, result2);
        }
    }
}
