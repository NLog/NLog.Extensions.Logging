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
        private IDisposable _registerChangeCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="nlogConfig">Configuration section to be read</param>
        public NLogLoggingConfiguration(IConfigurationSection nlogConfig)
            : this(nlogConfig, LogManager.LogFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLoggingConfiguration" /> class.
        /// </summary>
        /// <param name="nlogConfig">Configuration section to be read</param>
        /// <param name="logFactory">The <see cref="LogFactory" /> to which to apply any applicable configuration values.</param>
        public NLogLoggingConfiguration(IConfigurationSection nlogConfig, LogFactory logFactory)
            : base(logFactory)
        {
            _originalConfigSection = nlogConfig;
            _autoReload = LoadConfigurationSection(nlogConfig);
        }

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
                    _reloadConfiguration = state => ReloadConfigurationSection((IConfigurationSection)state);
                    LogFactory.ConfigurationChanged += LogFactory_ConfigurationChanged;
                }

                return Enumerable.Empty<string>();
            }
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
                    _autoReload = false; // Cannot unsubscribe to reload event, but we can stop reacting to it
                    LogFactory.ConfigurationChanged -= LogFactory_ConfigurationChanged;
                    _registerChangeCallback?.Dispose();
                    _registerChangeCallback = null;
                }
            }
            else if (ReferenceEquals(e.ActivatedConfiguration, this) && _autoReload && _reloadConfiguration != null)
            {
                // Setup reload notification
                LogFactory.ConfigurationChanged += LogFactory_ConfigurationChanged;
                MonitorForReload(_originalConfigSection);
            }
        }

        private void ReloadConfigurationSection(IConfigurationSection nlogConfig)
        {
            try
            {
                if (!_autoReload)
                {
                    return; // Should no longer react to reload events
                }

                InternalLogger.Info("Reloading NLogLoggingConfiguration...");
                var newConfig = new NLogLoggingConfiguration(nlogConfig, LogFactory);
                var oldConfig = LogFactory.Configuration;
                if (oldConfig != null)
                {
                    if (LogFactory.KeepVariablesOnReload)
                    {
                        foreach (var variable in oldConfig.Variables)
                            newConfig.Variables[variable.Key] = variable.Value;
                    }
                    LogFactory.Configuration = newConfig;
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "NLogLoggingConfiguration failed to reload");
                MonitorForReload(nlogConfig); // Continue watching this file
            }
        }

        private void MonitorForReload(IConfigurationSection nlogConfig)
        {
            _registerChangeCallback?.Dispose();
            _registerChangeCallback = null;
            _registerChangeCallback = nlogConfig.GetReloadToken().RegisterChangeCallback(_reloadConfiguration, nlogConfig);
        }

        private class LoggingConfigurationElement : ILoggingConfigurationElement
        {
            private const string TargetKey = "target";
            private const string TargetsKey = "targets";
            private const string DefaultTargetParameters = "Default-target-parameters";
            private const string VariableKey = "Variable";
            private const string DefaultWrapper = "Default-wrapper";
            private readonly IConfigurationSection _configurationSection;
            private IConfigurationSection DefaultTargetParametersSection { get; set; }
            private IConfigurationSection DefaultTargetWrapperSection { get; set; }
            private readonly string _nameOverride;
            private readonly bool _topElement;

            public LoggingConfigurationElement(IConfigurationSection configurationSection, bool topElement, string nameOverride = null)
            {
                _configurationSection = configurationSection;
                _nameOverride = nameOverride;
                _topElement = topElement;
                if (topElement && bool.TryParse(configurationSection["autoreload"], out var autoReload))
                {
                    AutoReload = autoReload;
                }
            }

            public bool AutoReload { get; }

            public string Name => _nameOverride ?? _configurationSection.Key;
            public IEnumerable<KeyValuePair<string, string>> Values => GetValues();
            public IEnumerable<ILoggingConfigurationElement> Children => GetChildren();

            private IEnumerable<KeyValuePair<string, string>> GetValues()
            {
                var children = _configurationSection.GetChildren();
                foreach (var child in children)
                {
                    if (!child.GetChildren().Any())
                    {
                        yield return new KeyValuePair<string, string>(child.Key, child.Value);
                    }
                }

                if (_nameOverride != null)
                {
                    if (ReferenceEquals(_nameOverride, DefaultTargetParameters))
                    {
                        yield return new KeyValuePair<string, string>("type", _configurationSection.Key);
                    }
                    else
                    {
                        yield return new KeyValuePair<string, string>("name", _configurationSection.Key);
                    }

                    if (ReferenceEquals(_nameOverride, VariableKey))
                    {
                        yield return new KeyValuePair<string, string>("value", _configurationSection.Value);
                    }
                }
            }

            private IEnumerable<ILoggingConfigurationElement> GetChildren()
            {
                var variables = GetVariablesSection();
                if (variables != null)
                {
                    foreach (var variable in variables.GetChildren())
                    {
                        yield return new LoggingConfigurationElement(variable, false, VariableKey);
                    }
                }

                var isTargetsSection = IsTargetsSection();
                if (isTargetsSection)
                {
                    foreach (var targetDefaultConfig in GetTargetsDefaultConfigElements())
                    {
                        yield return targetDefaultConfig;
                    }
                }

                var children = _configurationSection.GetChildren();
                foreach (var loggingConfigurationElement in GetChildren(children, variables, isTargetsSection))
                {
                    yield return loggingConfigurationElement;
                }
            }

            private IEnumerable<ILoggingConfigurationElement> GetChildren(IEnumerable<IConfigurationSection> children, IConfigurationSection variables, bool isTargetsSection)
            {
                var defaultTargetWrapper = GetDefaultWrapperSection();
                var defaultTargetParameters = GetDefaultTargetParametersSection();
                foreach (var child in children)
                {
                    if (AlreadyReadChild(child, variables, defaultTargetWrapper, defaultTargetParameters))
                    {
                        continue;
                    }

                    var firstChildValue = child?.GetChildren()?.FirstOrDefault();
                    if (firstChildValue == null)
                    {
                        continue; // Simple value without children
                    }

                    if (IsTargetWithinWrapper(child))
                    {
                        yield return new LoggingConfigurationElement(firstChildValue, false, TargetKey);
                    }
                    else
                    {
                        var isTargetKey = child.Key.EqualsOrdinalIgnoreCase(TargetsKey);
                        yield return new LoggingConfigurationElement(child, false, isTargetsSection ? TargetKey : null)
                        {
                            DefaultTargetParametersSection = (defaultTargetParameters != null && isTargetKey) ? defaultTargetParameters : null,
                            DefaultTargetWrapperSection = (defaultTargetWrapper != null && isTargetKey) ? defaultTargetWrapper : null,
                        };
                    }
                }
            }

            private bool IsTargetsSection()
            {
                return !_topElement && _nameOverride == null && _configurationSection.Key.EqualsOrdinalIgnoreCase(TargetsKey);
            }

            /// <summary>
            /// Target-config inside Wrapper-Target
            /// </summary>
            /// <param name="child"></param>
            /// <returns></returns>
            private bool IsTargetWithinWrapper(IConfigurationSection child)
            {
                return _nameOverride == TargetKey && child.Key.EqualsOrdinalIgnoreCase(TargetKey) && child.GetChildren().Count() == 1;
            }

            private IEnumerable<LoggingConfigurationElement> GetTargetsDefaultConfigElements()
            {
                if (DefaultTargetWrapperSection != null)
                    yield return new LoggingConfigurationElement(DefaultTargetWrapperSection, true, DefaultWrapper);

                if (DefaultTargetParametersSection != null)
                {
                    foreach (var targetParameters in DefaultTargetParametersSection.GetChildren())
                    {
                        yield return new LoggingConfigurationElement(targetParameters, true, DefaultTargetParameters);
                    }
                }
            }

            private bool AlreadyReadChild(IConfigurationSection child, IConfigurationSection variables, IConfigurationSection defaultWrapper, IConfigurationSection defaultTargetParameters)
            {
                if (_topElement)
                {
                    if (variables != null && child.Key.EqualsOrdinalIgnoreCase(variables.Key))
                    {
                        return true;
                    }

                    if (defaultWrapper != null && child.Key.EqualsOrdinalIgnoreCase(defaultWrapper.Key))
                    {
                        return true;
                    }

                    if (defaultTargetParameters != null && child.Key.EqualsOrdinalIgnoreCase(defaultTargetParameters.Key))
                    {
                        return true;
                    }
                }

                return false;
            }

            private IConfigurationSection GetVariablesSection()
            {
                var variables = _topElement ? _configurationSection.GetSection("Variables") : null;
                return variables;
            }

            private IConfigurationSection GetDefaultTargetParametersSection()
            {
                var defaultTargetParameters = _topElement ? _configurationSection.GetSection(DefaultTargetParameters) : null;
                if (defaultTargetParameters != null && defaultTargetParameters.GetChildren().Any())
                {
                    return defaultTargetParameters;
                }

                return null;
            }

            private IConfigurationSection GetDefaultWrapperSection()
            {
                var defaultWrapper = _topElement ? _configurationSection.GetSection(DefaultWrapper) : null;
                if (defaultWrapper != null && defaultWrapper.GetChildren().Any())
                {
                    return defaultWrapper;
                }

                return null;
            }
        }
    }
}