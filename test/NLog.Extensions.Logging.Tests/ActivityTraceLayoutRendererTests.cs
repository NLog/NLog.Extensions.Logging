#if NETCOREAPP3_0

using System;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class ActivityTraceLayoutRendererTests
    {
        [Fact]
        public void TestAllPropertiesWhenActivityNull()
        {
            bool orgThrowExceptions = LogManager.ThrowExceptions;

            try
            {
                LogManager.ThrowExceptions = true;
                System.Diagnostics.Activity.Current = null;

                var logEvent = LogEventInfo.CreateNullEvent();

                foreach (ActivityTraceProperty property in Enum.GetValues(typeof(ActivityTraceProperty)))
                {
                    ActivityTraceLayoutRenderer layoutRenderer = new ActivityTraceLayoutRenderer();
                    layoutRenderer.Property = property;
                    var result = layoutRenderer.Render(logEvent);
                    Assert.True(string.IsNullOrEmpty(result));
                }
            }
            finally
            {
                LogManager.ThrowExceptions = orgThrowExceptions;
            }
        }

        [Theory]
        [InlineData(ActivityTraceProperty.Id, false)]           // Id will never be empty
        [InlineData(ActivityTraceProperty.TraceId, true)]
        [InlineData(ActivityTraceProperty.OperationName, true)]
        [InlineData(ActivityTraceProperty.StartTimeUtc, true)]
        [InlineData(ActivityTraceProperty.Duration, true)]
        [InlineData(ActivityTraceProperty.Baggage, true)]
        [InlineData(ActivityTraceProperty.Tags, true)]
        [InlineData(ActivityTraceProperty.ParentId, true)]
        [InlineData(ActivityTraceProperty.ParentSpanId, true)]
        [InlineData(ActivityTraceProperty.RootId, false)]       // Will fallback to Id
        [InlineData(ActivityTraceProperty.TraceState, true)]
        [InlineData(ActivityTraceProperty.ActivityTraceFlags, true)]
        public void TestAllPropertiesWhenActivityEmpty(ActivityTraceProperty property, bool empty)
        {
            bool orgThrowExceptions = LogManager.ThrowExceptions;

            try
            {
                LogManager.ThrowExceptions = true;
                System.Diagnostics.Activity.Current = new System.Diagnostics.Activity(null).Start();
                System.Diagnostics.Activity.Current.SetStartTime(new DateTime(0, DateTimeKind.Utc));

                var logEvent = LogEventInfo.CreateNullEvent();

                ActivityTraceLayoutRenderer layoutRenderer = new ActivityTraceLayoutRenderer();
                layoutRenderer.Property = property;
                var result = layoutRenderer.Render(logEvent);
                Assert.True(string.IsNullOrEmpty(result) == empty);
            }
            finally
            {
                LogManager.ThrowExceptions = orgThrowExceptions;
            }
        }

        [Theory]
        [InlineData(ActivityTraceProperty.Id, null, null)]        // Id will never be empty
        [InlineData(ActivityTraceProperty.TraceId, null, "")]
        [InlineData(ActivityTraceProperty.OperationName, null, "MyOperation")]
        [InlineData(ActivityTraceProperty.StartTimeUtc, "u", "0001-01-01 00:00:00Z")]
        [InlineData(ActivityTraceProperty.Duration, null, "00:00:00")]
        [InlineData(ActivityTraceProperty.Baggage, null, "")]
        [InlineData(ActivityTraceProperty.Tags, null, "")]
        [InlineData(ActivityTraceProperty.ParentId, null, "")]
        [InlineData(ActivityTraceProperty.ParentSpanId, null, "")]
        [InlineData(ActivityTraceProperty.RootId, null, null)]       // Will fallback to Id
        [InlineData(ActivityTraceProperty.TraceState, null, "")]
        [InlineData(ActivityTraceProperty.ActivityTraceFlags, null, "")]
        public void TestAllPropertiesWhenActivityRunning(ActivityTraceProperty property, string format, string output)
        {
            bool orgThrowExceptions = LogManager.ThrowExceptions;

            try
            {
                DateTime startedTime = new DateTime(1, DateTimeKind.Utc);

                LogManager.ThrowExceptions = true;
                System.Diagnostics.Activity.Current = new System.Diagnostics.Activity("MyOperation").Start();
                System.Diagnostics.Activity.Current.SetStartTime(startedTime);

                var logEvent = LogEventInfo.CreateNullEvent();

                ActivityTraceLayoutRenderer layoutRenderer = new ActivityTraceLayoutRenderer();
                layoutRenderer.Property = property;
                layoutRenderer.Format = format;
                var result = layoutRenderer.Render(logEvent);
                if (output != null)
                    Assert.Equal(output, result);
            }
            finally
            {
                LogManager.ThrowExceptions = orgThrowExceptions;
            }
        }

        [Fact]
        public void TestBaggageSingleItem()
        {
            // Arrange
            var logEvent = LogEventInfo.CreateNullEvent();
            ActivityTraceLayoutRenderer layoutRenderer = new ActivityTraceLayoutRenderer();
            layoutRenderer.Property = ActivityTraceProperty.Baggage;

            // Act
            System.Diagnostics.Activity.Current = new System.Diagnostics.Activity("MyOperation").Start().AddBaggage("myitem1", "myvalue1");
            var result = layoutRenderer.Render(logEvent);

            // Assert
            Assert.Equal("myitem1=myvalue1", result);
        }

        [Fact]
        public void TestBaggageSingleItemJson()
        {
            // Arrange
            var logEvent = LogEventInfo.CreateNullEvent();
            ActivityTraceLayoutRenderer layoutRenderer = new ActivityTraceLayoutRenderer();
            layoutRenderer.Property = ActivityTraceProperty.Baggage;
            layoutRenderer.Format = "@";

            // Act
            System.Diagnostics.Activity.Current = new System.Diagnostics.Activity("MyOperation").Start().AddBaggage("myitem1", "myvalue1");
            var result = layoutRenderer.Render(logEvent);

            // Assert
            Assert.Equal("{ \"myitem1\": \"myvalue1\" }", result);
        }

        [Fact]
        public void TestBaggageDoubleItem()
        {
            // Arrange
            var logEvent = LogEventInfo.CreateNullEvent();
            ActivityTraceLayoutRenderer layoutRenderer = new ActivityTraceLayoutRenderer();
            layoutRenderer.Property = ActivityTraceProperty.Baggage;

            // Act
            System.Diagnostics.Activity.Current = new System.Diagnostics.Activity("MyOperation").Start().AddBaggage("myitem1", "myvalue1").AddBaggage("myitem2", "myvalue2");
            var result = layoutRenderer.Render(logEvent);

            // Assert
            Assert.Contains("myitem1=myvalue1", result);
            Assert.Contains("myitem2=myvalue2", result);
        }

        [Fact]
        public void TestBaggageDoubleItemJson()
        {
            // Arrange
            var logEvent = LogEventInfo.CreateNullEvent();
            ActivityTraceLayoutRenderer layoutRenderer = new ActivityTraceLayoutRenderer();
            layoutRenderer.Property = ActivityTraceProperty.Baggage;
            layoutRenderer.Format = "@";

            // Act
            System.Diagnostics.Activity.Current = new System.Diagnostics.Activity("MyOperation").Start().AddBaggage("myitem1", "myvalue1").AddBaggage("myitem2", "myvalue2");
            var result = layoutRenderer.Render(logEvent);

            var jsonElement = (System.Text.Json.JsonElement)System.Text.Json.JsonSerializer.Deserialize(result, typeof(object));
            Assert.Equal("myvalue1", jsonElement.GetProperty("myitem1").GetString());
            Assert.Equal("myvalue2", jsonElement.GetProperty("myitem2").GetString());
        }

        [Fact]
        public void TestTagsSingleItem()
        {
            // Arrange
            var logEvent = LogEventInfo.CreateNullEvent();
            ActivityTraceLayoutRenderer layoutRenderer = new ActivityTraceLayoutRenderer();
            layoutRenderer.Property = ActivityTraceProperty.Tags;

            // Act
            System.Diagnostics.Activity.Current = new System.Diagnostics.Activity("MyOperation").Start().AddTag("myitem1", "myvalue1");
            var result = layoutRenderer.Render(logEvent);

            // Assert
            Assert.Equal("myitem1=myvalue1", result);
        }

        [Fact]
        public void TestTagsDoubleItem()
        {
            // Arrange
            var logEvent = LogEventInfo.CreateNullEvent();
            ActivityTraceLayoutRenderer layoutRenderer = new ActivityTraceLayoutRenderer();
            layoutRenderer.Property = ActivityTraceProperty.Tags;

            // Act
            System.Diagnostics.Activity.Current = new System.Diagnostics.Activity("MyOperation").Start().AddTag("myitem1", "myvalue1").AddTag("myitem2", "myvalue2");
            var result = layoutRenderer.Render(logEvent);

            // Assert
            Assert.Contains("myitem1=myvalue1", result);
            Assert.Contains("myitem2=myvalue2", result);
        }

        [Fact]
        public void TestTagsDoubleItemJson()
        {
            // Arrange
            var logEvent = LogEventInfo.CreateNullEvent();
            ActivityTraceLayoutRenderer layoutRenderer = new ActivityTraceLayoutRenderer();
            layoutRenderer.Property = ActivityTraceProperty.Tags;
            layoutRenderer.Format = "@";

            // Act
            System.Diagnostics.Activity.Current = new System.Diagnostics.Activity("MyOperation").Start().AddTag("myitem1", "myvalue1").AddTag("myitem2", "myvalue2");
            var result = layoutRenderer.Render(logEvent);

            var jsonElement = (System.Text.Json.JsonElement)System.Text.Json.JsonSerializer.Deserialize(result, typeof(object));
            Assert.Equal("myvalue1", jsonElement.GetProperty("myitem1").GetString());
            Assert.Equal("myvalue2", jsonElement.GetProperty("myitem2").GetString());
        }
    }
}

#endif