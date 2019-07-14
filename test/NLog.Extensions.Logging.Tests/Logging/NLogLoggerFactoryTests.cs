using System;
using System.IO;
using Microsoft.Extensions.Logging;
using NLog.Common;
using Xunit;

namespace NLog.Extensions.Logging.Tests.Logging
{
    [Collection(TestsWithInternalLog)]
    public class NLogLoggerFactoryTests : NLogTestBase
    {
        [Fact]
        public void Dispose_HappyPath_FlushLogFactory()
        {
            // Arrange
            var loggerFactory = new NLogLoggerFactory();

            var internalLogWriter = CaptureInternalLog();

            // Act
            loggerFactory.Dispose();

            // Assert
            var internalLog = internalLogWriter.ToString();
            Assert.Contains("LogFactory.Flush", internalLog, StringComparison.OrdinalIgnoreCase);
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
            var unitUnderTest = new NLogLoggerFactory();
            ILoggerProvider provider = new NLogLoggerProvider();

            var internalLogWriter = CaptureInternalLog();

            // Act
            unitUnderTest.AddProvider(provider);

            // Assert
            var internalLog = internalLogWriter.ToString();
            Assert.Contains("AddProvider will be ignored", internalLog, StringComparison.OrdinalIgnoreCase);
        }
    }
}
