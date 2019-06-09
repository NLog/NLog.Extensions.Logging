using NLog.Extensions.Logging;
using System;
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
            var provider = new NLogLoggerProvider();

            var internalLogWriter = CaptureInternalLog();

            // Act
            provider.Dispose();

            // Assert
            var internalLog = internalLogWriter.ToString();
            Assert.Contains("LogFactory.Flush", internalLog, StringComparison.OrdinalIgnoreCase);
        }
    }
}
