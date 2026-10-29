using System.IO;
using Xunit;

namespace NLog.Extensions.Hosting.Tests
{
    [Collection("Host renderer environment")]
    public class HostRootDirLayoutRendererTests
    {
        [Fact]
        public void RendersHostContentRootWithoutTrailingSeparators()
        {
            // Arrange
            var contentRootPath = Path.Combine(Path.GetTempPath(), "HostRoot") + Path.DirectorySeparatorChar + Path.AltDirectorySeparatorChar;
            var hostEnvironment = new TestHostEnvironment { ContentRootPath = contentRootPath };

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostRootDirLayoutRenderer>("host-rootdir", "${host-rootdir}", hostEnvironment);

            // Assert
            Assert.Equal(contentRootPath.TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar), result);
        }

        [Fact]
        public void UsesAspNetCoreContentRootBeforeDotnetContentRoot()
        {
            // Arrange
            using var variables = new EnvironmentVariableScope("ASPNETCORE_CONTENTROOT", "DOTNET_CONTENTROOT");
            var aspNetCoreRoot = Path.Combine(Path.GetTempPath(), "AspNetCoreRoot") + Path.DirectorySeparatorChar;
            variables.Set("ASPNETCORE_CONTENTROOT", $" {aspNetCoreRoot} ");
            variables.Set("DOTNET_CONTENTROOT", Path.Combine(Path.GetTempPath(), "DotnetRoot"));
            var hostEnvironment = new TestHostEnvironment();

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostRootDirLayoutRenderer>("host-rootdir", "${host-rootdir}", hostEnvironment);

            // Assert
            Assert.Equal(aspNetCoreRoot.TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar), result);
        }

        [Fact]
        public void UsesDotnetContentRootWhenAspNetCoreContentRootIsBlank()
        {
            // Arrange
            using var variables = new EnvironmentVariableScope("ASPNETCORE_CONTENTROOT", "DOTNET_CONTENTROOT");
            var dotnetRoot = Path.Combine(Path.GetTempPath(), "DotnetRoot") + Path.AltDirectorySeparatorChar;
            variables.Set("ASPNETCORE_CONTENTROOT", "  ");
            variables.Set("DOTNET_CONTENTROOT", dotnetRoot);
            var hostEnvironment = new TestHostEnvironment();

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostRootDirLayoutRenderer>("host-rootdir", "${host-rootdir}", hostEnvironment);

            // Assert
            Assert.Equal(dotnetRoot.TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar), result);
        }

        [Fact]
        public void UsesEnvironmentFallbackWhenHostContentRootThrows()
        {
            // Arrange
            using var variables = new EnvironmentVariableScope("ASPNETCORE_CONTENTROOT", "DOTNET_CONTENTROOT");
            var environmentRoot = Path.Combine(Path.GetTempPath(), "EnvironmentRoot");
            variables.Set("DOTNET_CONTENTROOT", environmentRoot);
            var hostEnvironment = new TestHostEnvironment { ThrowOnContentRootPath = true };

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostRootDirLayoutRenderer>("host-rootdir", "${host-rootdir}", hostEnvironment);

            // Assert
            Assert.Equal(environmentRoot, result);
        }

        [Fact]
        public void FallsBackToCurrentApplicationDirectoryWhenContentRootsAreMissing()
        {
            // Arrange
            using var variables = new EnvironmentVariableScope("ASPNETCORE_CONTENTROOT", "DOTNET_CONTENTROOT");

            // Act
            var result = HostLayoutRendererTestHelper.Render<HostRootDirLayoutRenderer>("host-rootdir", "${host-rootdir}");

            // Assert
            Assert.True(Path.IsPathRooted(result));
            Assert.NotEmpty(result);
        }
    }
}