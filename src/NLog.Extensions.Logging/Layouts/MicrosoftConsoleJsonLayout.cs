using System;
using System.Collections.Generic;
using System.Linq;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Renders output that simulates Microsoft Json Console Formatter from AddJsonConsole
    /// </summary>
    /// <seealso href="https://github.com/NLog/NLog/wiki/MicrosoftConsoleJsonLayout">Documentation on NLog Wiki</seealso>
    [Layout("MicrosoftConsoleJsonLayout")]
    [ThreadAgnostic]
    public class MicrosoftConsoleJsonLayout : JsonLayout
    {
        private static readonly string[] EventIdMapper = Enumerable.Range(0, 512).Select(id => id.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray();

        private readonly SimpleLayout _timestampLayout = new SimpleLayout("\"${date:format=o:universalTime=true}\"");

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftConsoleJsonLayout" /> class.
        /// </summary>
        public MicrosoftConsoleJsonLayout()
        {
            SuppressSpaces = false;
            Attributes.Add(new JsonAttribute("Timestamp", _timestampLayout) { Encode = false });
            Attributes.Add(new JsonAttribute("EventId", Layout.FromMethod(evt => LookupEventId(evt), LayoutRenderOptions.ThreadAgnostic)) { Encode = false });
            Attributes.Add(new JsonAttribute("LogLevel", Layout.FromMethod(evt => ConvertLogLevel(evt.Level), LayoutRenderOptions.ThreadAgnostic)) { Encode = false });
            Attributes.Add(new JsonAttribute("Category", "${logger}"));
            Attributes.Add(new JsonAttribute("Message", "${message}"));
            Attributes.Add(new JsonAttribute("Exception", "${exception:format=tostring,data}"));
            var stateJsonLayout = new JsonLayout() { IncludeEventProperties = true };
            stateJsonLayout.ExcludeProperties.Add(nameof(EventIdCaptureType.EventId));
            stateJsonLayout.ExcludeProperties.Add(nameof(EventIdCaptureType.EventId_Id));
            stateJsonLayout.Attributes.Add(new JsonAttribute("{OriginalFormat}", "${message:raw=true}"));
            Attributes.Add(new JsonAttribute("State", stateJsonLayout) { Encode = false });
        }

        /// <summary>
        /// Gets the array of attributes for the "state"-section
        /// </summary>
        [ArrayParameter(typeof(JsonAttribute), "state")]
        public IList<JsonAttribute>? StateAttributes
        {
            get
            {
                var index = LookupNamedAttributeIndex("State");
                return index >= 0 ? (Attributes[index]?.Layout as JsonLayout)?.Attributes : new List<JsonAttribute>();
            }
        }

        /// <summary>
        /// Gets or sets whether to include "scopes"-section
        /// </summary>
        public bool IncludeScopes
        {
            get => LookupNamedAttributeIndex("Scopes") >= 0;
            set
            {
                var index = LookupNamedAttributeIndex("Scopes");
                if (index >= 0)
                {
                    if (!value)
                        Attributes.RemoveAt(index);
                }
                else if (value)
                {
                    Attributes.Add(new JsonAttribute("Scopes", "${scopenested:format=@}") { Encode = false });
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to include "TraceId" + "SpanId" + "ParentId" from <see cref="System.Diagnostics.Activity.Current"/>
        /// </summary>
        /// <remarks>
        /// Similar to ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId | ActivityTrackingOptions.ParentId .
        ///
        /// For additional Activity properties, use <see cref="StateAttributes"/> together with NLog.DiagnosticSource-nuget-package.
        /// </remarks>
        public bool IncludeActivityIds
        {
            get => LookupNamedAttributeIndex("TraceId") >= 0;
            set
            {
                var index_traceId = LookupNamedAttributeIndex("TraceId");
                if (index_traceId >= 0)
                {
                    if (!value)
                    {
                        Attributes.RemoveAt(index_traceId);
                        var index_spanId = LookupNamedAttributeIndex("SpanId");
                        if (index_spanId >= 0)
                            Attributes.RemoveAt(index_spanId);
                        var index_parentId = LookupNamedAttributeIndex("ParentId");
                        if (index_parentId >= 0)
                            Attributes.RemoveAt(index_parentId);
                    }
                }
                else if (value)
                {
                    Attributes.Add(new JsonAttribute("TraceId", Layout.FromMethod(l => ActivityExtensions.GetTraceId(System.Diagnostics.Activity.Current))));
                    Attributes.Add(new JsonAttribute("SpanId", Layout.FromMethod(l => ActivityExtensions.GetSpanId(System.Diagnostics.Activity.Current))));
                    Attributes.Add(new JsonAttribute("ParentId", Layout.FromMethod(l => ActivityExtensions.GetParentId(System.Diagnostics.Activity.Current))));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to include "Timestamp"-section
        /// </summary>
        public string? TimestampFormat
        {
            get
            {
                var index = LookupNamedAttributeIndex("Timestamp");
                return index >= 0 ? ((Attributes[index].Layout as SimpleLayout)?.LayoutRenderers?.OfType<DateLayoutRenderer>().FirstOrDefault())?.Format : null;
            }
            set
            {
                var index = LookupNamedAttributeIndex("Timestamp");
                if (index >= 0)
                {
                    Attributes.RemoveAt(index);
                }

                if (value != null && !string.IsNullOrEmpty(value))
                {
                    var dateLayoutRenderer = _timestampLayout.LayoutRenderers.OfType<DateLayoutRenderer>().First();
                    dateLayoutRenderer.Format = value;
                    Attributes.Insert(0, new JsonAttribute("Timestamp", _timestampLayout) { Encode = false });
                }
            }
        }

        /// <inheritdoc />
        protected override void InitializeLayout()
        {
            IncludeEventProperties = false;
            IncludeScopeProperties = false;

            var stateIndex = LookupNamedAttributeIndex("State");
            var stateJsonLayout = stateIndex >= 0 ? Attributes[stateIndex]?.Layout as JsonLayout : null;
            if (stateJsonLayout != null)
            {
                stateJsonLayout.MaxRecursionLimit = MaxRecursionLimit;
                stateJsonLayout.DottedRecursion = DottedRecursion;
                stateJsonLayout.ExcludeEmptyProperties = ExcludeEmptyProperties;
                stateJsonLayout.SuppressSpaces = SuppressSpaces && !IndentJson;
                if (ExcludeProperties?.Count > 0)
                {
                    foreach (var excludeProperty in ExcludeProperties)
                    {
                        stateJsonLayout.ExcludeProperties.Add(excludeProperty);
                    }
                }
            }

            base.InitializeLayout();
        }

        private int LookupNamedAttributeIndex(string attributeName)
        {
            for (int i = 0; i < Attributes.Count; ++i)
            {
                if (attributeName.Equals(Attributes[i].Name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        private static string LookupEventId(LogEventInfo logEvent)
        {
            if (logEvent.HasProperties)
            {
                if (logEvent.Properties.TryGetValue(nameof(EventIdCaptureType.EventId), out var eventObject))
                {
                    if (eventObject is int eventId)
                        return ConvertEventId(eventId);
                    else if (eventObject is Microsoft.Extensions.Logging.EventId eventIdStruct)
                        return ConvertEventId(eventIdStruct.Id);
                }

                if (logEvent.Properties.TryGetValue(nameof(EventIdCaptureType.EventId_Id), out var eventid) && eventid is int)
                {
                    return ConvertEventId((int)eventid);
                }
            }

            return "0";
        }

        private static string ConvertEventId(int eventId)
        {
            if (eventId == 0)
                return "0";
            else if (eventId > 0 && eventId < EventIdMapper.Length)
                return EventIdMapper[eventId];
            else
                return eventId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string ConvertLogLevel(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Trace)
                return "\"" + nameof(Microsoft.Extensions.Logging.LogLevel.Trace) + "\"";
            else if (logLevel == LogLevel.Debug)
                return "\"" + nameof(Microsoft.Extensions.Logging.LogLevel.Debug) + "\"";
            else if (logLevel == LogLevel.Info)
                return "\"" + nameof(Microsoft.Extensions.Logging.LogLevel.Information) + "\"";
            else if (logLevel == LogLevel.Warn)
                return "\"" + nameof(Microsoft.Extensions.Logging.LogLevel.Warning) + "\"";
            else if (logLevel == LogLevel.Error)
                return "\"" + nameof(Microsoft.Extensions.Logging.LogLevel.Error) + "\"";
            else
                return "\"" + nameof(Microsoft.Extensions.Logging.LogLevel.Critical) + "\"";
        }
    }
}
