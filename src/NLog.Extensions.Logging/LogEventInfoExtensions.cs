using Microsoft.Extensions.Logging;

namespace NLog.Extensions.Logging
{

    public static class LogEventInfoExtensions {


        /// <summary>
        /// Gets the <see cref="Microsoft.Extensions.Logging.EventId"/>. If it was not included in the log 
        /// event, then uses the default value.
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static EventId getEventId(this LogEventInfo logEvent, EventId defaultValue = default(EventId))
        {

            if (!logEvent.HasProperties)
                return defaultValue;

            // setup key names the same way "NLogLogger.cs" does it
            var options = new NLogProviderOptions();
            var idKey = $"EventId{options.EventIdSeparator}Id";
            var nameKey = $"EventId{options.EventIdSeparator}Name";

            // try get EventId properties or use defaults
            var id = (logEvent.Properties.ContainsKey(idKey)) ? (int)logEvent.Properties[idKey] : defaultValue.Id;
            var name = (logEvent.Properties.ContainsKey(nameKey)) ? logEvent.Properties[nameKey].ToString() : defaultValue.Name;

            return new EventId(id, name);
        }


    }
}
