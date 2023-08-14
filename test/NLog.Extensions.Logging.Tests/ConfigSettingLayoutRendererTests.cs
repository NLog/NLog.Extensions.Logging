using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class ConfigSettingLayoutRendererTests
    {
        [Fact]
        public void ConfigSettingFallbackDefaultLookup()
        {
            ConfigSettingLayoutRenderer.DefaultConfiguration = null;
            var layoutRenderer = new ConfigSettingLayoutRenderer { Item = "Options.TableName", Default = "MyTableName" };
            var result = layoutRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("MyTableName", result);
        }

        [Fact]
        public void ConfigSettingSimpleLookup()
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig["Mode"] = "Test";
            ConfigSettingLayoutRenderer.DefaultConfiguration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var layoutRenderer = new ConfigSettingLayoutRenderer { Item = "Mode" };
            var result = layoutRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("Test", result);
        }

        [Fact]
        public void ConfigSettingNestedLookup()
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig["Options:TableName"] = "Test";
            ConfigSettingLayoutRenderer.DefaultConfiguration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var layoutRenderer = new ConfigSettingLayoutRenderer { Item = "Options.TableName" };
            var result = layoutRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("Test", result);
        }

        [Fact]
        public void ConfigSettingConfigEscapeLookup()
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig["Logging:Microsoft.Logging"] = "Test";
            ConfigSettingLayoutRenderer.DefaultConfiguration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var layoutRenderer = new ConfigSettingLayoutRenderer { Item = @"Logging.Microsoft\.Logging" };
            var result = layoutRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("Test", result);

            var layoutRenderer2 = new ConfigSettingLayoutRenderer { Item = @"Logging.Microsoft..Logging" };
            var result2 = layoutRenderer2.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("Test", result2);
        }
    }
}
