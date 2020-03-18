namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Interface for fluent setup of LogFactory integration with Microsoft Extension Logging
    /// </summary>
    public interface ISetupExtensionLoggingBuilder
    {
        /// <summary>
        /// LogFactory under configuration
        /// </summary>
        LogFactory LogFactory { get; }
    }
}
