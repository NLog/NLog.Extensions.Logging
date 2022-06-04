using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class MicrosoftConsoleLayoutRendererTest
    {
        [Fact]
        public void MicrosoftConsoleLayoutRenderer_NullEvent()
        {
            var layoutRenderer = new MicrosoftConsoleLayoutRenderer();
            var result = layoutRenderer.Render(LogEventInfo.CreateNullEvent());
            Assert.Contains("crit: [0]", result);
        }

        [Fact]
        public void MicrosoftConsoleLayoutRenderer_ExceptionEvent()
        {
            var layoutRenderer = new MicrosoftConsoleLayoutRenderer();
            var exception = new ArgumentException("Test");
            var eventId = 42;
            var result = layoutRenderer.Render(new LogEventInfo(LogLevel.Error, "MyLogger", null, "Alert {EventId_Id}", new object[] { eventId }, exception));
            Assert.Equal($"fail: MyLogger[{eventId}]{Environment.NewLine}      Alert 42{Environment.NewLine}{exception}", result);
        }

        [Fact]
        public void MicrosoftConsoleLayoutRenderer_OutOfMapperBoundsEventId()
        {
            var layoutRenderer = new MicrosoftConsoleLayoutRenderer();
            var exception = new ArgumentException("Test");
            var eventId = 500;
            var result = layoutRenderer.Render(new LogEventInfo(LogLevel.Error, "MyLogger", null, "Alert {EventId_Id}", new object[] { eventId }, exception));
            Assert.Equal($"fail: MyLogger[{eventId}]{Environment.NewLine}      Alert 500{Environment.NewLine}{exception}", result);
        }
    }
}
