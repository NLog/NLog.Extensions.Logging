using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using NLog.Common;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Layout renderer that can lookup values from Microsoft Extension Configuration Container (json, xml, ini)
    /// </summary>
    /// <remarks>Not to be confused with NLog.AppConfig that includes ${appsetting}</remarks>
    /// <example>
    /// Example: appsettings.json
    /// {
    ///     "Mode":"Prod",
    ///     "Options":{
    ///         "StorageConnectionString":"UseDevelopmentStorage=true",
    ///     }
    /// }
    /// 
    /// Config Setting Lookup:
    ///     ${configsetting:name=Mode} = "Prod"
    ///     ${configsetting:name=Options.StorageConnectionString} = "UseDevelopmentStorage=true"
    ///     ${configsetting:name=Options.TableName:default=MyTable} = "MyTable"
    ///     
    /// Config Setting Lookup Cached:
    ///      ${configsetting:cached=True:name=Mode}
    /// </example>
    [LayoutRenderer("configsetting")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class ConfigSettingLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Global Configuration Container
        /// </summary>
        public static IConfiguration DefaultConfiguration { get; set; }

        ///<summary>
        /// Item in the setting container
        ///</summary>
        [RequiredParameter]
        [DefaultParameter]
        public string Item
        {
            get => _item;
            set
            {
                _item = value;
                _itemLookup = value?.Replace(".", ":");
            }
        }
        private string _item;
        private string _itemLookup;

        /// <summary>
        /// Name of the Item
        /// </summary>
        [Obsolete("Replaced by Item-property")]
        public string Name { get => Item; set => Item = value; }

        ///<summary>
        /// The default value to render if the setting value is null.
        ///</summary>
        public string Default { get; set; }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(_itemLookup))
                return;

            string value = null;
            var configurationRoot = DefaultConfiguration;
            if (configurationRoot != null)
            {
                value = configurationRoot[_itemLookup];
            }
            else
            {
#if NETCORE1_0
                InternalLogger.Debug("Missing DefaultConfiguration. Remember to provide IConfiguration when calling AddNLog");
#else
                InternalLogger.Debug("Missing DefaultConfiguration. Remember to register IConfiguration by calling UseNLog");
#endif
            }

            builder.Append(value ?? Default);
        }
    }
}
