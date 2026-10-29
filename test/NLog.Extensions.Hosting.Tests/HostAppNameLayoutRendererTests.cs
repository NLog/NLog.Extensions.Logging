using System.Diagnostics;
using Xunit;

namespace NLog.Extensions.Hosting.Tests
{
    public class HostAppNameLayoutRendererTests
    {
        [Fact]
        public void RendersHostApplicationName()
        {
            // Arrange
            var hostEnvironment = new TestHostEnvironment { ApplicationName = "TestApplication" };

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostAppNameLayoutRenderer>("host-appname", "${host-appname}", hostEnvironment);

            // Assert
            Assert.Equal("TestApplication", result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void FallsBackToProcessNameWhenApplicationNameIsBlank(string applicationName)
        {
            // Arrange
            var hostEnvironment = new TestHostEnvironment { ApplicationName = applicationName };
            var processName = Process.GetCurrentProcess().ProcessName;

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostAppNameLayoutRenderer>("host-appname", "${host-appname}", hostEnvironment);

            // Assert
            Assert.Equal(processName, result);
        }

        [Fact]
        public void FallsBackToProcessNameWhenHostEnvironmentThrows()
        {
            // Arrange
            var hostEnvironment = new TestHostEnvironment { ThrowOnApplicationName = true };
            var processName = Process.GetCurrentProcess().ProcessName;

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostAppNameLayoutRenderer>("host-appname", "${host-appname}", hostEnvironment);

            // Assert
            Assert.Equal(processName, result);
        }
    }
}