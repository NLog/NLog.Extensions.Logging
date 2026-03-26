using System.Diagnostics;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Helpers for getting the right values from Activity no matter the format (w3c or hierarchical)
    /// </summary>
    internal static class ActivityExtensions
    {
        private static readonly string EmptySpanIdToHexString = default(System.Diagnostics.ActivitySpanId).ToHexString();
        private static readonly string EmptyTraceIdToHexString = default(System.Diagnostics.ActivityTraceId).ToHexString();

        public static string GetSpanId(this Activity? activity)
        {
            return activity?.IdFormat switch
            {
                ActivityIdFormat.Hierarchical => activity.Id,
                ActivityIdFormat.W3C => SpanIdToHexString(activity.SpanId),
                _ => null,
            } ?? string.Empty;
        }

        public static string GetTraceId(this Activity? activity)
        {
            return activity?.IdFormat switch
            {
                ActivityIdFormat.Hierarchical => activity.RootId,
                ActivityIdFormat.W3C => TraceIdToHexString(activity.TraceId),
                _ => null,
            } ?? string.Empty;
        }

        public static string GetParentId(this Activity? activity)
        {
            return activity?.IdFormat switch
            {
                ActivityIdFormat.Hierarchical => activity.ParentId,
                ActivityIdFormat.W3C => SpanIdToHexString(activity.ParentSpanId),
                _ => null,
            } ?? string.Empty;
        }

        private static string SpanIdToHexString(ActivitySpanId spanId)
        {
            var spanIdString = spanId.ToHexString();
            return EmptySpanIdToHexString.Equals(spanIdString, System.StringComparison.Ordinal) ? string.Empty : spanIdString;
        }

        private static string TraceIdToHexString(ActivityTraceId traceId)
        {
            var traceIdString = traceId.ToHexString();
            return EmptyTraceIdToHexString.Equals(traceIdString, System.StringComparison.Ordinal) ? string.Empty : traceIdString;
        }
    }
}
