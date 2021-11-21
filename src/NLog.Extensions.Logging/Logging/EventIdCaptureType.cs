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
        /// Capture integer <see cref="Microsoft.Extensions.Logging.EventId.Id"/> as "EventId"-property
        /// </summary>
        EventId = 1,
        /// <summary>
        /// Capture string <see cref="Microsoft.Extensions.Logging.EventId.Name"/> as "EventName"-property
        /// </summary>
        EventName = 2,
        /// <summary>
        /// Capture struct <see cref="Microsoft.Extensions.Logging.EventId"/> as "EventId"-property (with boxing)
        /// </summary>
        EventIdStruct = 4,
        /// <summary>
        /// Capture integer <see cref="Microsoft.Extensions.Logging.EventId.Id"/> as "EventId_Id"-property (Legacy)
        /// </summary>
        EventId_Id = 8,
        /// <summary>
        /// Capture string <see cref="Microsoft.Extensions.Logging.EventId.Name"/> as "EventId_Name"-property (Legacy)
        /// </summary>
        EventId_Name = 16,
        /// <summary>
        /// Captures legacy properties (EventId-struct + EventId_Id + EventId_Name)
        /// </summary>
        Legacy = EventIdStruct | EventId_Id | EventId_Name,
    }
}
