using System.Linq;
using NLog.Layouts;
using NLog.Extensions.Hosting;
using NLog.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Hosting.Tests
{
    public class SetupExtensionsBuilderExtensionsTests
    {
        [Fact]
        public void WriteToJsonTargets_ConfiguresJsonLayoutsAndScopes()
        {
            // Arrange
            var logFactory = new LogFactory();

            // Act
            logFactory.Setup().LoadConfiguration(configuration =>
            {
                configuration.ForLogger().WriteToJsonFile(new SimpleLayout("test.json"), includeScopes: true);
                configuration.ForLogger().WriteToJsonConsole();
            });

            // Assert
            var fileTarget = Assert.IsType<Targets.FileTarget>(logFactory.Configuration.AllTargets.Single(target => target is Targets.FileTarget));
            var consoleTarget = Assert.IsType<Targets.ConsoleTarget>(logFactory.Configuration.AllTargets.Single(target => target is Targets.ConsoleTarget));
            Assert.True(Assert.IsType<MicrosoftConsoleJsonLayout>(fileTarget.Layout).IncludeScopes);
            Assert.False(Assert.IsType<MicrosoftConsoleJsonLayout>(consoleTarget.Layout).IncludeScopes);
        }
    }
}