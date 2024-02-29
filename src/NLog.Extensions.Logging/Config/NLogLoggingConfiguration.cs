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
        private const string RootSectionKey = "NLog";

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
            LoadConfigurationSection(nlogConfig);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLoggingConfiguration" /> class.
        /// </summary>
        private NLogLoggingConfiguration(LogFactory logFactory, IConfigurationSection nlogConfig)
            : base(logFactory)
        {
            _originalConfigSection = nlogConfig;
        }

        /// <summary>
        /// Gets the collection of file names which should be watched for changes by NLog.
        /// </summary>
        public override IEnumerable<string> FileNamesToWatch
        {
            get
            {
                if (_autoReload && _reloadConfiguration is null)
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
            return ReloadLoggingConfiguration(_originalConfigSection);
        }

        private LoggingConfiguration ReloadLoggingConfiguration(IConfigurationSection nlogConfig)
        {
            var newConfig = new NLogLoggingConfiguration(LogFactory, nlogConfig);
            newConfig.PrepareForReload(this);   // Ensure KeepVariablesOnReload works as intended
            newConfig.LoadConfigurationSection(nlogConfig);
            return newConfig;
        }

        private void LoadConfigurationSection(IConfigurationSection nlogConfig)
        {
            var configElement = new LoggingConfigurationElement(nlogConfig, RootSectionKey);
            LoadConfig(configElement, null);
            _autoReload = configElement.AutoReload;
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
                var newConfig = ReloadLoggingConfiguration(nlogConfig);
                var oldConfig = LogFactory.Configuration;
                if (oldConfig != null)
                {
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

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + $" ConfigSection={_originalConfigSection?.Key}";
        }

        private sealed class LoggingConfigurationElement : ILoggingConfigurationElement
        {
            private const string TargetKey = "target";
            private const string TargetsKey = "targets";
            private const string DefaultTargetParameters = "Default-target-parameters";
            private const string TargetDefaultParameters = "TargetDefaultParameters";
            private const string VariableKey = "Variable";
            private const string VariablesKey = "Variables";
            private const string RulesKey = "Rules";
            private const string ExtensionsKey = "Extensions";
            private const string DefaultWrapper = "Default-wrapper";
            private const string TargetDefaultWrapper = "TargetDefaultWrapper";
            private readonly IConfigurationSection _configurationSection;
            private IConfigurationSection TargetDefaultParametersSection { get; set; }
            private IConfigurationSection TargetDefaultWrapperSection { get; set; }
            private readonly string _nameOverride;
            private readonly bool _topElement;

            public LoggingConfigurationElement(IConfigurationSection configurationSection, string nameOverride = null)
            {
                _configurationSection = configurationSection;
                _nameOverride = nameOverride;
                _topElement = ReferenceEquals(nameOverride, RootSectionKey);
                if (_topElement && bool.TryParse(configurationSection["autoreload"], out var autoReload))
                {
                    AutoReload = autoReload;
                }
            }

            public bool AutoReload { get; }

            public string Name => _nameOverride ?? GetConfigKey(_configurationSection);
            public IEnumerable<KeyValuePair<string, string>> Values => GetValues();
            public IEnumerable<ILoggingConfigurationElement> Children => GetChildren();

            private IEnumerable<KeyValuePair<string, string>> GetValues()
            {
                if (_nameOverride != null)
                {
                    if (ReferenceEquals(_nameOverride, DefaultTargetParameters))
                    {
                        yield return new KeyValuePair<string, string>("type", GetConfigKey(_configurationSection));
                    }
                    else if (ReferenceEquals(_nameOverride, VariableKey))
                    {
                        var configValue = _configurationSection.Value;
                        yield return new KeyValuePair<string, string>("name", GetConfigKey(_configurationSection));
                        if (configValue is null)
                            yield break;    // Signal to NLog Config Parser to check GetChildren() for variable layout
                        else
                            yield return new KeyValuePair<string, string>("value", configValue);
                    }
                    else if (!_topElement)
                    {
                        var configValue = _configurationSection.Value;
                        if (configValue is null)
                        {
                            yield return new KeyValuePair<string, string>("name", GetConfigKey(_configurationSection));
                        }
                        else
                        {
                            yield return new KeyValuePair<string, string>(GetConfigKey(_configurationSection), configValue);
                        }
                    }
                }

                var children = _configurationSection.GetChildren();
                foreach (var child in children)
                {
                    if (!child.GetChildren().Any())
                    {
                        var configKey = GetConfigKey(child);
                        var configValue = child.Value;
                        if (_topElement && IgnoreTopElementChildNullValue(configKey, configValue))
                            continue;   // Complex object without any properties has no children and null-value (Ex. empty targets-section)

                        yield return new KeyValuePair<string, string>(configKey, configValue);
                    }
                }
            }
            
            private static bool IgnoreTopElementChildNullValue(string configKey, object configValue)
            {
                if (configValue is null)
                {
                    if (string.Equals(TargetsKey, configKey, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(VariablesKey, configKey, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(TargetDefaultParameters, configKey, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(DefaultTargetParameters, configKey, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(RulesKey, configKey, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(ExtensionsKey, configKey, StringComparison.OrdinalIgnoreCase))
                        return true;    // Only accept known section-names as being empty (when no children and null value)
                }

                return false;
            }

            private IEnumerable<ILoggingConfigurationElement> GetChildren()
            {
                var variables = GetVariablesSection();
                if (variables != null)
                {
                    foreach (var variable in variables.GetChildren())
                    {
                        yield return new LoggingConfigurationElement(variable, VariableKey);
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

                if (ReferenceEquals(_nameOverride, VariableKey) && _configurationSection.Value is null)
                {
                    yield return new LoggingConfigurationElement(_configurationSection);
                }
                else
                {
                    var children = _configurationSection.GetChildren();
                    foreach (var loggingConfigurationElement in GetChildren(children, variables, isTargetsSection))
                    {
                        yield return loggingConfigurationElement;
                    }
                }
            }

            private IEnumerable<ILoggingConfigurationElement> GetChildren(IEnumerable<IConfigurationSection> children, IConfigurationSection variables, bool isTargetsSection)
            {
                var targetDefaultWrapper = GetTargetDefaultWrapperSection();
                var targetDefaultParameters = GetTargetDefaultParametersSection();
                foreach (var child in children)
                {
                    if (AlreadyReadChild(child, variables, targetDefaultWrapper, targetDefaultParameters))
                    {
                        continue;
                    }

                    var firstChildValue = child?.GetChildren()?.FirstOrDefault();
                    if (firstChildValue is null)
                    {
                        continue; // Simple value without children
                    }

                    if (IsTargetWithinWrapper(child))
                    {
                        yield return new LoggingConfigurationElement(firstChildValue, TargetKey);
                    }
                    else if (_topElement && GetConfigKey(child).EqualsOrdinalIgnoreCase(TargetsKey))
                    {
                        yield return new LoggingConfigurationElement(child)
                        {
                            TargetDefaultParametersSection = targetDefaultParameters,
                            TargetDefaultWrapperSection = targetDefaultWrapper,
                        };
                    }
                    else
                    {
                        yield return new LoggingConfigurationElement(child, isTargetsSection ? TargetKey : null);
                    }
                }
            }

            private static string GetConfigKey(IConfigurationSection child)
            {
                return child.Key?.Trim() ?? string.Empty;
            }

            private bool IsTargetsSection()
            {
                return !_topElement && _nameOverride is null && GetConfigKey(_configurationSection).EqualsOrdinalIgnoreCase(TargetsKey);
            }

            /// <summary>
            /// Target-config inside Wrapper-Target
            /// </summary>
            private bool IsTargetWithinWrapper(IConfigurationSection child)
            {
                return _nameOverride == TargetKey && GetConfigKey(child).EqualsOrdinalIgnoreCase(TargetKey) && child.GetChildren().Count() == 1;
            }

            private IEnumerable<LoggingConfigurationElement> GetTargetsDefaultConfigElements()
            {
                if (TargetDefaultWrapperSection != null)
                    yield return new LoggingConfigurationElement(TargetDefaultWrapperSection, DefaultWrapper);

                if (TargetDefaultParametersSection != null)
                {
                    foreach (var targetParameters in TargetDefaultParametersSection.GetChildren())
                    {
                        yield return new LoggingConfigurationElement(targetParameters, DefaultTargetParameters);
                    }
                }
            }

            private bool AlreadyReadChild(IConfigurationSection child, IConfigurationSection variables, IConfigurationSection targetDefaultWrapper, IConfigurationSection targetDefaultParameters)
            {
                if (_topElement)
                {
                    if (variables != null && child.Key.EqualsOrdinalIgnoreCase(variables.Key))
                    {
                        return true;
                    }

                    if (targetDefaultWrapper != null && child.Key.EqualsOrdinalIgnoreCase(targetDefaultWrapper.Key))
                    {
                        return true;
                    }

                    if (targetDefaultParameters != null && child.Key.EqualsOrdinalIgnoreCase(targetDefaultParameters.Key))
                    {
                        return true;
                    }
                }

                return false;
            }

            private IConfigurationSection GetVariablesSection()
            {
                var variables = _topElement ? _configurationSection.GetSection(VariablesKey) : null;
                return variables;
            }

            private IConfigurationSection GetTargetDefaultParametersSection()
            {
                if (_topElement)
                {
                    var targetDefaultParameters = _configurationSection.GetSection(TargetDefaultParameters);
                    if (targetDefaultParameters?.GetChildren().Any() == true)
                    {
                        return targetDefaultParameters;
                    }

                    targetDefaultParameters = _configurationSection.GetSection(DefaultTargetParameters);
                    if (targetDefaultParameters?.GetChildren().Any() == true)
                    {
                        return targetDefaultParameters;
                    }
                }

                return null;
            }

            private IConfigurationSection GetTargetDefaultWrapperSection()
            {
                if (_topElement)
                {
                    var targetDefaultWrapper = _configurationSection.GetSection(TargetDefaultWrapper);
                    if (targetDefaultWrapper?.GetChildren().Any() == true)
                    {
                        return targetDefaultWrapper;
                    }

                    targetDefaultWrapper = _configurationSection.GetSection(DefaultWrapper);
                    if (targetDefaultWrapper?.GetChildren().Any() == true)
                    {
                        return targetDefaultWrapper;
                    }
                }

                return null;
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}