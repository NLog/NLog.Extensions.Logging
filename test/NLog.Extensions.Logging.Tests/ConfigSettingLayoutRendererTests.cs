﻿using System.Collections.Generic;
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
            var layoutRenderer = new ConfigSettingLayoutRenderer() { Name = "Options.TableName", Default = "MyTableName" };
            var result = layoutRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("MyTableName", result);
        }

#if NETSTANDARD2_0 || NET461
        [Fact]
        public void ConfigSettingGlobalConfigLookup()
        {
            var memoryConfig = new Dictionary<string, string>();
            memoryConfig["Mode"] = "Test";
            ConfigSettingLayoutRenderer.DefaultConfiguration = new ConfigurationBuilder().AddInMemoryCollection(memoryConfig).Build();
            var layoutRenderer = new ConfigSettingLayoutRenderer() { Name = "Mode" };
            var result = layoutRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("Test", result);
        }
#endif
    }
}
