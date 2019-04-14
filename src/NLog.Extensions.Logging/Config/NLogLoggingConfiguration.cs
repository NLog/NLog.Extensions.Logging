using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NLog.Common;
using NLog.Config;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Configures NLog through Microsoft Extension Configuration section (Ex from appsettings.json)
    /// </summary>
    public class NLogLoggingConfiguration : LoggingConfigurationParser
    {
        private readonly IConfigurationSection _originalConfigSection;
        private bool _autoReload;
        private Action<object> _reloadConfiguration;

        /// <summary>
        /// Gets the collection of file names which should be watched for changes by NLog.
        /// </summary>
        public override IEnumerable<string> FileNamesToWatch
        {
            get
            {
                if (_autoReload && _reloadConfiguration == null)
                {
                    // Prepare for setting up reload notification handling
                    _reloadConfiguration = (state) => ReloadConfigurationSection((IConfigurationSection)state);
                    LogFactory.ConfigurationChanged += LogFactory_ConfigurationChanged;
                }
                return Enumerable.Empty<string>();
            }
        }

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
            _originalConfigSection = nlogConfig;
            _autoReload = LoadConfigurationSection(nlogConfig);
        }

        /// <inheritdoc />
        public override LoggingConfiguration Reload()
        {
            return new NLogLoggingConfiguration(_originalConfigSection, LogFactory);
        }

        private bool LoadConfigurationSection(IConfigurationSection nlogConfig)
        {
            var configElement = new LoggingConfigurationElement(nlogConfig, true);
            LoadConfig(configElement, null);
            return configElement.AutoReload;
        }

        private void LogFactory_ConfigurationChanged(object sender, LoggingConfigurationChangedEventArgs e)
        {
            if (ReferenceEquals(e.DeactivatedConfiguration, this))
            {
                if (_autoReload)
                {
                    _autoReload = false;    // Cannot unsubscribe to reload event, but we can stop reacting to it
                    LogFactory.ConfigurationChanged -= LogFactory_ConfigurationChanged;
                }
            }
            else if (ReferenceEquals(e.ActivatedConfiguration, this))
            {
                if (_autoReload && _reloadConfiguration != null)
                {
                    // Setup reload notification
                    LogFactory.ConfigurationChanged += LogFactory_ConfigurationChanged;
                    MonitorForReload(_originalConfigSection);
                }
            }
        }

        private void ReloadConfigurationSection(IConfigurationSection nlogConfig)
        {
            try
            {
                if (!_autoReload)
                    return; // Should no longer react to reload events

                Common.InternalLogger.Info("Reloading NLogLoggingConfiguration...");
                NLogLoggingConfiguration newConfig = new NLogLoggingConfiguration(nlogConfig, LogFactory);
                if (LogFactory.Configuration != null)
                {
                    LogFactory.Configuration = newConfig;
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "NLogLoggingConfiguration failed to reload");
                MonitorForReload(nlogConfig);   // Continue watching this file
            }
        }

        private void MonitorForReload(IConfigurationSection nlogConfig)
        {
            nlogConfig.GetReloadToken().RegisterChangeCallback(_reloadConfiguration, nlogConfig);
        }

        private class LoggingConfigurationElement : ILoggingConfigurationElement
        {
            private const string TargetKey = "target";
            private const string DefaultTargetParameters = "default-target-parameters";
            private const string VariableKey = "Variable";
            private readonly IConfigurationSection _configurationSection;
            private readonly string _nameOverride;
            private readonly bool _topElement;

            public string Name => _nameOverride ?? _configurationSection.Key;
            public IEnumerable<KeyValuePair<string, string>> Values => GetValues();
            public IEnumerable<ILoggingConfigurationElement> Children => GetChildren();
            public bool AutoReload { get; }

            public LoggingConfigurationElement(IConfigurationSection configurationSection, bool topElement, string nameOverride = null)
            {
                _configurationSection = configurationSection;
                _nameOverride = nameOverride;
                _topElement = topElement;
                if (topElement)
                {
                    if (bool.TryParse(configurationSection["autoreload"], out var autoreload))
                    {
                        AutoReload = autoreload;
                    }
                }
            }

            private IEnumerable<KeyValuePair<string, string>> GetValues()
            {
                var children = _configurationSection.GetChildren();
                foreach (var child in children)
                {
                    if (!child.GetChildren().Any())
                        yield return new KeyValuePair<string, string>(child.Key, child.Value);
                }
                if (_nameOverride != null)
                {
                    if (ReferenceEquals(_nameOverride, DefaultTargetParameters))
                        yield return new KeyValuePair<string, string>("type", _configurationSection.Key);
                    else
                        yield return new KeyValuePair<string, string>("name", _configurationSection.Key);

                    if (ReferenceEquals(_nameOverride, VariableKey))
                        yield return new KeyValuePair<string, string>("value", _configurationSection.Value);
                }
            }

            private IEnumerable<ILoggingConfigurationElement> GetChildren()
            {
                var variables = _topElement ? _configurationSection.GetSection("Variables") : null;
                if (variables != null)
                {
                    foreach (var variable in variables.GetChildren())
                        yield return new LoggingConfigurationElement(variable, false, VariableKey);
                }

                bool targetsSection = !_topElement && _nameOverride == null && _configurationSection.Key.EqualsOrdinalIgnoreCase("targets");
                var defaultWrapper = targetsSection ? _configurationSection.GetSection("default-wrapper") : null;
                if (defaultWrapper?.GetChildren().Any()==true)
                {
                    yield return new LoggingConfigurationElement(defaultWrapper, false);
                }

                var defaultTargetParameters = targetsSection ? _configurationSection.GetSection(DefaultTargetParameters) : null;
                if (defaultTargetParameters != null)
                {
                    foreach (var targetParameters in defaultTargetParameters.GetChildren())
                        yield return new LoggingConfigurationElement(targetParameters, false, DefaultTargetParameters);
                }

                var children = _configurationSection.GetChildren();
                foreach (var child in children)
                {
                    var firstChildValue = child?.GetChildren()?.FirstOrDefault();
                    if (firstChildValue == null)
                        continue;   // Simple value without children

                    if (_nameOverride == TargetKey && child.Key.EqualsOrdinalIgnoreCase(TargetKey) && child.GetChildren().Count() == 1)
                    {
                        // Target-config inside Wrapper-Target
                        yield return new LoggingConfigurationElement(firstChildValue, false, TargetKey);
                    }
                    else
                    {
                        if (variables != null && child.Key.EqualsOrdinalIgnoreCase(variables.Key))
                            continue;

                        string nameOverride = null;
                        if (targetsSection)
                        {
                            if (defaultWrapper != null && child.Key.EqualsOrdinalIgnoreCase(defaultWrapper.Key))
                                continue;
                            if (defaultTargetParameters != null && child.Key.EqualsOrdinalIgnoreCase(defaultTargetParameters.Key))
                                continue;

                            nameOverride = TargetKey;
                        }
                        yield return new LoggingConfigurationElement(child, false, nameOverride);
                    }
                }
            }
        }
    }
}
