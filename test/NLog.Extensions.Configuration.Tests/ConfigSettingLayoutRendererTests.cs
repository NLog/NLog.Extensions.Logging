using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace NLog.Extensions.Configuration.Tests
{
    public class ConfigSettingLayoutRendererTests
    {
        [Fact]
        public void ConfigSettingSimpleLookup()
        {
            ConfigSettingLayoutRenderer.DefaultConfiguration = null;
            var layoutRenderer = new ConfigSettingLayoutRenderer() { Name = "Mode" };
            var result = layoutRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("Prod", result);
        }

        [Fact]
        public void ConfigSettingOptimizedLookup()
        {
            ConfigSettingLayoutRenderer.DefaultConfiguration = null;
            var layoutRenderer1 = new ConfigSettingLayoutRenderer() { Name = "Mode" };
            var result1 = layoutRenderer1.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("Prod", result1);

            var layoutRenderer2 = new ConfigSettingLayoutRenderer() { Name = "Options.SqlConnectionString" };
            var result2 = layoutRenderer2.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("YourProdStorageConnectionString", result2);

            Assert.Same(layoutRenderer1._configurationRoot, layoutRenderer2._configurationRoot);
        }

        [Fact]
        public void ConfigSettingFallbackDefaultLookup()
        {
            ConfigSettingLayoutRenderer.DefaultConfiguration = null;
            var layoutRenderer = new ConfigSettingLayoutRenderer() { Name = "Options.TableName", Default = "MyTableName" };
            var result = layoutRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("MyTableName", result);
        }

        [Fact]
        public void ConfigSettingCustomeFileNameLookup()
        {
            ConfigSettingLayoutRenderer.DefaultConfiguration = null;
            var layoutRenderer = new ConfigSettingLayoutRenderer() { Name = "Mode", FileName = "appsettings.json" };
            var result = layoutRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("Prod", result);
        }

        [Fact]
        public void ConfigSettingGlobalConfigLookup()
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig["Mode"] = "Test";
            ConfigSettingLayoutRenderer.DefaultConfiguration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var layoutRenderer = new ConfigSettingLayoutRenderer() { Name = "Mode", FileName = "" };
            var result = layoutRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("Test", result);
        }
    }
}
