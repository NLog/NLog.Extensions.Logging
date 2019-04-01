using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NLog.Config;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Configures NLog through Microsoft Extension Configuration section (Ex from appsettings.json)
    /// </summary>
    public class NLogLoggingConfiguration : LoggingConfigurationParser
    {
        private readonly Action<object> _reloadConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLoggingConfiguration"/> class. 
        /// </summary>
        /// <param name="nlogConfig">Configuration section to be read</param>
        public NLogLoggingConfiguration(IConfigurationSection nlogConfig)
            : this(nlogConfig, LogManager.LogFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLoggingConfiguration"/> class. 
        /// </summary>
        /// <param name="nlogConfig">Configuration section to be read</param>
        /// <param name="logFactory">The <see cref="LogFactory"/> to which to apply any applicable configuration values.</param>
        public NLogLoggingConfiguration(IConfigurationSection nlogConfig, LogFactory logFactory)
            : base(logFactory)
        {
            _reloadConfiguration = (state) => LoadConfigurationSection((IConfigurationSection)state, true);
            LoadConfigurationSection(nlogConfig, null);
        }

        private void LoadConfigurationSection(IConfigurationSection nlogConfig, bool? autoReload)
        {
            var configElement = new LoggingConfigurationElement(nlogConfig, true);
            LoadConfig(configElement, null);
            if (autoReload ?? configElement.AutoReload)
            {
                nlogConfig.GetReloadToken().RegisterChangeCallback(_reloadConfiguration, nlogConfig);
            }
        }

        private class LoggingConfigurationElement : ILoggingConfigurationElement
        {
            readonly IConfigurationSection _configurationSection;
            readonly string _nameOverride;

            public bool AutoReload { get; }

            public LoggingConfigurationElement(IConfigurationSection configurationSection, bool topElement, string nameOverride = null)
            {
                _configurationSection = configurationSection;
                _nameOverride = nameOverride;
                if (topElement && bool.TryParse(configurationSection["autoreload"], out var autoreload))
                {
                    AutoReload = autoreload;
                }
            }

            public string Name => _nameOverride ?? _configurationSection.Key;

            public IEnumerable<KeyValuePair<string, string>> Values
            {
                get
                {
                    var children = _configurationSection.GetChildren();
                    foreach (var child in children)
                    {
                        if (!child.GetChildren().Any())
                            yield return new KeyValuePair<string, string>(child.Key, child.Value);
                    }
                    if (_nameOverride != null)
                        yield return new KeyValuePair<string, string>("name", _configurationSection.Key);
                }
            }

            public IEnumerable<ILoggingConfigurationElement> Children
            {
                get
                {
                    var children = _configurationSection.GetChildren();
                    foreach (var child in children)
                    {
                        var firstChildValue = child?.GetChildren()?.FirstOrDefault();
                        if (firstChildValue == null)
                        {
                            continue;
                        }

                        if (_nameOverride == "target" && child.Key.EqualsOrdinalIgnoreCase("target") && child.GetChildren().Count() == 1)
                        {
                            yield return new LoggingConfigurationElement(firstChildValue, false, "target");
                        }
                        else
                        {
                            string nameOverride = null;
                            if (_configurationSection.Key.EqualsOrdinalIgnoreCase("targets"))
                            {
                                nameOverride = "target";
                            }
                            yield return new LoggingConfigurationElement(child, false, nameOverride);
                        }
                    }
                }
            }
        }
    }
}
