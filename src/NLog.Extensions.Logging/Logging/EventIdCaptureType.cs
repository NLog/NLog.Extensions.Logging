using System;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Defines EventId capture options
    /// </summary>
    [Flags]
    public enum EventIdCaptureType
    {
        /// <summary>
        /// Skip capture
        /// </summary>
        None = 0,
        /// <summary>
        /// Capture entire <see cref="Microsoft.Extensions.Logging.EventId"/> as "EventId"-property (with boxing)
        /// </summary>
        EventId = 1,
        /// <summary>
        /// Capture <see cref="Microsoft.Extensions.Logging.EventId.Id"/> as "EventId_Id"-property.
        /// </summary>
        EventId_Id = 2,
        /// <summary>
        /// Capture <see cref="Microsoft.Extensions.Logging.EventId.Name"/> as "EventId_Name"-property.
        /// </summary>
        EventId_Name = 4,
        /// <summary>
        /// Capture all properties (Legacy)
        /// </summary>
        All = EventId | EventId_Id | EventId_Name,
    }
}
