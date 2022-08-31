using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class MicrosoftConsoleJsonLayoutTests
    {
        [Fact]
        public void MicrosoftConsoleJsonLayout_NullEvent()
        {
            var layout = new MicrosoftConsoleJsonLayout() { TimestampFormat = null };
            var result = layout.Render(LogEventInfo.CreateNullEvent());
            Assert.Contains("{ \"EventId\": 0, \"LogLevel\": \"Critical\" }", result);
        }

        [Fact]
        public void MicrosoftConsoleJsonLayout_ExceptionEvent()
        {
            var layout = new MicrosoftConsoleJsonLayout();
            var exception = new ArgumentException("Test");
            var eventId = 42;
            var logEvent1 = new LogEventInfo(LogLevel.Error, "MyLogger", null, "Alert {EventId}", new object[] { eventId }, exception);
            var result1 = layout.Render(logEvent1);
            Assert.Equal($"{{ \"Timestamp\": \"{logEvent1.TimeStamp.ToUniversalTime().ToString("O")}\", \"EventId\": {eventId}, \"LogLevel\": \"Error\", \"Category\": \"MyLogger\", \"Message\": \"Alert {eventId}\", \"Exception\": \"{exception.ToString()}\", \"State\": {{ \"{{OriginalFormat}}\": \"Alert {{EventId}}\" }} }}", result1);

            var eventId2 = 420;
            var logEvent2 = new LogEventInfo(LogLevel.Error, "MyLogger", null, "Alert {EventId_Id}", new object[] { eventId2 }, exception);
            var result2 = layout.Render(logEvent2);
            Assert.Equal($"{{ \"Timestamp\": \"{logEvent2.TimeStamp.ToUniversalTime().ToString("O")}\", \"EventId\": {eventId2}, \"LogLevel\": \"Error\", \"Category\": \"MyLogger\", \"Message\": \"Alert {eventId2}\", \"Exception\": \"{exception.ToString()}\", \"State\": {{ \"{{OriginalFormat}}\": \"Alert {{EventId_Id}}\" }} }}", result2);
        }

        [Fact]
        public void MicrosoftConsoleJsonLayout_IncludeScopesEvent()
        {
            var logFactory = new LogFactory().Setup().LoadConfiguration(builder =>
            {
                var layout = new MicrosoftConsoleJsonLayout() { IncludeScopes = true };
                builder.ForLogger().WriteTo(new NLog.Targets.MemoryTarget("test") { Layout = layout });
            }).LogFactory;
            var logger = logFactory.GetCurrentClassLogger();

            var exception = new ArgumentException("Test");
            var eventId = 42;
            using var requestScope = logger.PushScopeNested("Request Started");
            using var activityScope = logger.PushScopeNested("Activity Started");
            var logEvent = new LogEventInfo(LogLevel.Error, null, null, "Alert {EventId}", new object[] { eventId }, exception);
            logger.Log(logEvent);
            var result = logFactory.Configuration.FindTargetByName<NLog.Targets.MemoryTarget>("test")?.Logs?.FirstOrDefault();
            Assert.Equal($"{{ \"Timestamp\": \"{logEvent.TimeStamp.ToUniversalTime().ToString("O")}\", \"EventId\": {eventId}, \"LogLevel\": \"Error\", \"Category\": \"{typeof(MicrosoftConsoleJsonLayoutTests).FullName}\", \"Message\": \"Alert {eventId}\", \"Exception\": \"{exception.ToString()}\", \"State\": {{ \"{{OriginalFormat}}\": \"Alert {{EventId}}\" }}, \"Scopes\": [ \"Request Started\", \"Activity Started\" ] }}", result);
        }
    }
}
