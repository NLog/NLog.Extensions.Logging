#if NETCOREAPP3_0

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Properties from <see cref="System.Diagnostics.Activity"/> available for <see cref="ActivityTraceLayoutRenderer"/> 
    /// </summary>
    public enum ActivityTraceProperty
    {
        /// <summary>
        /// Identifier that is specific to a particular request.
        /// </summary>
        Id,
        /// <summary>
        /// TraceId part of the <see cref="Id"/>
        /// </summary>
        TraceId,
        /// <summary>
        /// SPAN part of the <see cref="Id"/>
        /// </summary>
        SpanId,
        /// <summary>
        /// Operation name.
        /// </summary>
        OperationName,
        /// <summary>
        /// Time when the operation started.
        /// </summary>
        StartTimeUtc,
        /// <summary>
        /// Duration of the operation.
        /// </summary>
        Duration,
        /// <summary>
        /// Collection of key/value pairs that are passed to children of this Activity.
        /// </summary>
        Baggage,
        /// <summary>
        /// Collection of key/value pairs that are NOT passed to children of this Activity
        /// </summary>
        Tags,
        /// <summary>
        /// Activity's Parent ID.
        /// </summary>
        ParentId,
        /// <summary>
        /// Activity's Parent SpanID.
        /// </summary>
        ParentSpanId,
        /// <summary>
        /// Root ID of this Activity.
        /// </summary>
        RootId,
        /// <summary>
        /// W3C tracestate header.
        /// </summary>
        TraceState,
        /// <summary>
        /// <see cref="System.Diagnostics.ActivityTraceFlags"/> for activity (defined by the W3C ID specification) 
        /// </summary>
        ActivityTraceFlags,
    }
}

#endif