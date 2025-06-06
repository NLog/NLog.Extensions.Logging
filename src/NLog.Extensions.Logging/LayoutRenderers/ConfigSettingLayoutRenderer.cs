﻿using System;
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
    /// <seealso href="https://github.com/NLog/NLog/wiki/ConfigSetting-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("configsetting")]
    [ThreadAgnostic]
    public class ConfigSettingLayoutRenderer : LayoutRenderer
    {
        private IConfiguration? _serviceConfiguration;

        /// <summary>
        /// Global Configuration Container
        /// </summary>
        public static IConfiguration? DefaultConfiguration { get; set; }

        ///<summary>
        /// Item in the setting container
        ///</summary>
        [DefaultParameter]
        public string Item
        {
            get => _item;
            set
            {
                _item = value;
                _itemLookup = value?.Replace("\\.", "::").Replace(".", ":").Replace("::", ".") ?? string.Empty;
            }
        }
        private string _item = string.Empty;
        private string _itemLookup = string.Empty;

        /// <summary>
        /// Name of the Item
        /// </summary>
        [Obsolete("Replaced by Item-property")]
        public string Name { get => Item; set => Item = value; }

        ///<summary>
        /// The default value to render if the setting value is null.
        ///</summary>
        public string Default { get; set; } = string.Empty;

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            try
            {
                // Avoid NLogDependencyResolveException when possible
                if (!ReferenceEquals(ResolveService<IServiceProvider>(), LoggingConfiguration?.LogFactory?.ServiceRepository ?? NLog.LogManager.LogFactory.ServiceRepository))
                {
                    _serviceConfiguration = ResolveService<IConfiguration>();
                }
            }
            catch (NLogDependencyResolveException ex)
            {
                _serviceConfiguration = null;
                InternalLogger.Debug("ConfigSetting - Fallback to DefaultConfiguration: {0}", ex.Message);
            }

            base.InitializeLayoutRenderer();
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(_itemLookup))
                return;

            string? value = null;
            var configurationRoot = _serviceConfiguration ?? DefaultConfiguration;
            if (configurationRoot != null)
            {
                value = configurationRoot[_itemLookup];
            }
            else
            {
                InternalLogger.Debug("Missing DefaultConfiguration. Remember to register IConfiguration by calling UseNLog");
            }

            builder.Append(value ?? Default);
        }
    }
}
