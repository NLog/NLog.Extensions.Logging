#if NETCOREAPP3_0

using System;
using System.Collections.Generic;
using System.Text;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Layout renderer that can render properties from <see cref="System.Diagnostics.Activity.Current"/>
    /// </summary>
    [LayoutRenderer("activity")]
    [ThreadSafe]
    public sealed class ActivityTraceLayoutRenderer : LayoutRenderer
    {
        private static readonly string EmptySpanId = default(System.Diagnostics.ActivitySpanId).ToString();
        private static readonly string EmptyTraceId = default(System.Diagnostics.ActivityTraceId).ToString();

        /// <summary>
        /// Gets or sets the property to retrieve.
        /// </summary>
        public ActivityTraceProperty Property { get; set; } = ActivityTraceProperty.Id;

        /// <summary>
        /// Single item to extract from <see cref="System.Diagnostics.Activity.Baggage"/> or <see cref="System.Diagnostics.Activity.Tags"/>
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// Control output formating of selected property (if supported)
        /// </summary>
        public string Format { get; set; }

        /// <inheritdoc />
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var activity = System.Diagnostics.Activity.Current;
            if (activity == null)
                return;

            if ((Property == ActivityTraceProperty.Baggage || Property == ActivityTraceProperty.Tags) && string.IsNullOrEmpty(Item))
            {
                var collection = Property == ActivityTraceProperty.Baggage ? activity.Baggage : activity.Tags;
                if (Format == "@")
                {
                    RenderStringDictionaryJson(collection, builder);
                }
                else
                {
                    RenderStringDictionaryFlat(collection, builder);
                }
            }
            else
            {
                var value = GetValue(activity);
                builder.Append(value);
            }
        }

        private string GetValue(System.Diagnostics.Activity activity)
        {
            switch (Property)
            {
                case ActivityTraceProperty.Id: return activity.Id;
                case ActivityTraceProperty.TraceId: return CoalesceTraceId(activity.TraceId.ToString());
                case ActivityTraceProperty.SpanId: return CoalesceSpanId(activity.SpanId.ToString());
                case ActivityTraceProperty.OperationName: return activity.OperationName;
                case ActivityTraceProperty.StartTimeUtc:return activity.StartTimeUtc > DateTime.MinValue ? activity.StartTimeUtc.ToString(Format) : string.Empty;
                case ActivityTraceProperty.Duration: return activity.StartTimeUtc > DateTime.MinValue ? activity.Duration.ToString(Format) : string.Empty;
                case ActivityTraceProperty.ParentId: return activity.ParentId;
                case ActivityTraceProperty.ParentSpanId: return CoalesceSpanId(activity.ParentSpanId.ToString());
                case ActivityTraceProperty.RootId: return activity.RootId;
                case ActivityTraceProperty.TraceState: return activity.TraceStateString;
                case ActivityTraceProperty.ActivityTraceFlags: return activity.ActivityTraceFlags == System.Diagnostics.ActivityTraceFlags.None && string.IsNullOrEmpty(Format) ? string.Empty : activity.ActivityTraceFlags.ToString(Format);
                case ActivityTraceProperty.Baggage: return GetCollectionItem(Item, activity.Baggage);
                case ActivityTraceProperty.Tags: return GetCollectionItem(Item, activity.Tags);
                default: return string.Empty;
            }
        }

        private static void RenderStringDictionaryFlat(IEnumerable<KeyValuePair<string, string>> collection, StringBuilder builder)
        {
            var firstItem = true;
            foreach (var keyValue in collection)
            {
                if (!firstItem)
                    builder.Append(",");
                firstItem = false;
                builder.Append(keyValue.Key);
                builder.Append("=");
                builder.Append(keyValue.Value);
            }
        }

        private static void RenderStringDictionaryJson(IEnumerable<KeyValuePair<string, string>> collection, StringBuilder builder)
        {
            var firstItem = true;
            foreach (var keyValue in collection)
            {
                if (firstItem)
                    builder.Append("{ ");
                else
                    builder.Append(", ");
                firstItem = false;
                builder.Append("\"");
                builder.Append(keyValue.Key);
                builder.Append("\": \"");
                builder.Append(keyValue.Value);
                builder.Append("\"");
            }
            if (!firstItem)
                builder.Append(" }");
        }

        private static string GetCollectionItem(string item, IEnumerable<KeyValuePair<string,string>> collection)
        {
            foreach (var keyValue in collection)
                if (item.Equals(keyValue.Key, StringComparison.OrdinalIgnoreCase))
                    return keyValue.Value;
            return string.Empty;
        }

        private string CoalesceTraceId(string traceId)
        {
            if (EmptyTraceId == traceId)
                return string.Empty;
            else
                return traceId;
        }

        private string CoalesceSpanId(string spanId)
        {
            if (EmptySpanId == spanId)
                return string.Empty;
            else
                return spanId;
        }
    }
}

#endif