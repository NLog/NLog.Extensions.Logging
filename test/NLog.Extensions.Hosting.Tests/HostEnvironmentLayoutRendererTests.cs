using Xunit;

namespace NLog.Extensions.Hosting.Tests
{
    [Collection("Host renderer environment")]
    public class HostEnvironmentLayoutRendererTests
    {
        [Fact]
        public void RendersHostEnvironmentName()
        {
            // Arrange
            var hostEnvironment = new TestHostEnvironment { EnvironmentName = "TestEnvironment" };

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostEnvironmentLayoutRenderer>("host-environment", "${host-environment}", hostEnvironment);

            // Assert
            Assert.Equal("TestEnvironment", result);
        }

        [Fact]
        public void UsesAspNetCoreEnvironmentBeforeDotnetEnvironmentAndTrimsIt()
        {
            // Arrange
            using var variables = new EnvironmentVariableScope("ASPNETCORE_ENVIRONMENT", "DOTNET_ENVIRONMENT");
            variables.Set("ASPNETCORE_ENVIRONMENT", " Staging ");
            variables.Set("DOTNET_ENVIRONMENT", "Development");
            var hostEnvironment = new TestHostEnvironment();

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostEnvironmentLayoutRenderer>("host-environment", "${host-environment}", hostEnvironment);

            // Assert
            Assert.Equal("Staging", result);
        }

        [Fact]
        public void UsesDotnetEnvironmentWhenAspNetCoreEnvironmentIsBlank()
        {
            // Arrange
            using var variables = new EnvironmentVariableScope("ASPNETCORE_ENVIRONMENT", "DOTNET_ENVIRONMENT");
            variables.Set("ASPNETCORE_ENVIRONMENT", "  ");
            variables.Set("DOTNET_ENVIRONMENT", " Development ");
            var hostEnvironment = new TestHostEnvironment();

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostEnvironmentLayoutRenderer>("host-environment", "${host-environment}", hostEnvironment);

            // Assert
            Assert.Equal("Development", result);
        }

        [Fact]
        public void UsesEnvironmentFallbackWhenHostEnvironmentThrows()
        {
            // Arrange
            using var variables = new EnvironmentVariableScope("ASPNETCORE_ENVIRONMENT", "DOTNET_ENVIRONMENT");
            variables.Set("DOTNET_ENVIRONMENT", "TestEnvironment");
            var hostEnvironment = new TestHostEnvironment { ThrowOnEnvironmentName = true };

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostEnvironmentLayoutRenderer>("host-environment", "${host-environment}", hostEnvironment);

            // Assert
            Assert.Equal("TestEnvironment", result);
        }

        [Fact]
        public void DefaultsToProductionWhenEnvironmentValuesAreMissing()
        {
            // Arrange
            using var variables = new EnvironmentVariableScope("ASPNETCORE_ENVIRONMENT", "DOTNET_ENVIRONMENT");
            var hostEnvironment = new TestHostEnvironment();

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostEnvironmentLayoutRenderer>("host-environment", "${host-environment}", hostEnvironment);

            // Assert
            Assert.Equal("Production", result);
        }

        [Fact]
        public void PreservesWhitespaceHostEnvironmentName()
        {
            // Arrange
            using var variables = new EnvironmentVariableScope("ASPNETCORE_ENVIRONMENT", "DOTNET_ENVIRONMENT");
            var hostEnvironment = new TestHostEnvironment { EnvironmentName = " " };

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostEnvironmentLayoutRenderer>("host-environment", "${host-environment}", hostEnvironment);

            // Assert
            Assert.Equal(" ", result);
        }
    }
}