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

        /// <inheritdoc />
        public override LoggingConfiguration Reload()
        {
            return ReloadLoggingConfiguration(_originalConfigSection);
        }

        /// <inheritdoc />
        protected override void OnConfigurationAssigned(LogFactory logFactory)
        {
            _registerChangeCallback?.Dispose();

            if (_autoReload)
            {
                if (logFactory is null)
                {
                    _autoReload = false;
                }
                else
                {
                    MonitorForReload(_originalConfigSection);
                }
            }

            base.OnConfigurationAssigned(logFactory);
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
            _registerChangeCallback = new AutoReloadConfigChangeMonitor(nlogConfig, this);
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
                            continue;   // Complex object without any properties has no children and null-value (Ex. empty targets-section / variables-section)

                        yield return new KeyValuePair<string, string>(configKey, configValue);
                    }
                }
            }
            
            private static bool IgnoreTopElementChildNullValue(string configKey, object configValue)
            {
                if (configValue is null)
                {
                    // Only accept known section-names as being empty (when no children and null value)
                    if (string.Equals(TargetDefaultParameters, configKey, StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (string.Equals(DefaultTargetParameters, configKey, StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (string.Equals(RulesKey, configKey, StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (string.Equals(ExtensionsKey, configKey, StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (string.Equals(VariablesKey, configKey, StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (string.Equals(TargetsKey, configKey, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }

            private IEnumerable<ILoggingConfigurationElement> GetChildren()
            {
                var variables = GetVariablesSection();
                if (variables != null)
                {
                    foreach (var variable in GetVariablesChildren(variables))
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

            private static IEnumerable<IConfigurationSection> GetVariablesChildren(IConfigurationSection variables)
            {
                List<KeyValuePair<string, IConfigurationSection>> sortVariables = null;
                foreach (var variable in variables.GetChildren())
                {
                    var configKey = GetConfigKey(variable);
                    var configValue = variable.Value;
                    if (string.IsNullOrEmpty(configKey) || configValue?.Contains('$') == false)
                        yield return variable;

                    sortVariables ??= new List<KeyValuePair<string, IConfigurationSection>>();
                    sortVariables.Insert(0, new KeyValuePair<string, IConfigurationSection>(configKey, variable));
                }

                bool foundIndependentVariable = true;
                while (sortVariables?.Count > 0 && foundIndependentVariable)
                {
                    foundIndependentVariable = false;

                    // Enumerate all variables that doesn't reference other variables
                    for (int i = sortVariables.Count - 1; i >= 0; i--)
                    {
                        var configValue = sortVariables[i].Value.Value;
                        if (configValue is null)
                            continue;

                        bool usingOtherVariables = IsNLogConfigVariableValueUsingOthers(configValue, sortVariables);
                        if (!usingOtherVariables)
                        {
                            foundIndependentVariable = true;
                            yield return sortVariables[i].Value;
                            sortVariables.RemoveAt(i);
                        }
                    }
                }

                if (sortVariables?.Count > 0)
                {
                    // Give up and just return the variables in their sorted order
                    for (int i = sortVariables.Count - 1; i >= 0; i--)
                    {
                        yield return sortVariables[i].Value;
                    }
                }
            }

            private static bool IsNLogConfigVariableValueUsingOthers(string variableValue, List<KeyValuePair<string, IConfigurationSection>> allVariables)
            {
                foreach (var otherConfigVariable in allVariables)
                {
                    var otherConfigKey = otherConfigVariable.Key;
                    var referenceVariable = $"${{{otherConfigKey}}}";
                    if (variableValue.IndexOf(referenceVariable, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }

                return false;
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

        private sealed class AutoReloadConfigChangeMonitor : IDisposable
        {
            private NLogLoggingConfiguration _nlogConfig;
            private IDisposable _registerChangeCallback;

            public AutoReloadConfigChangeMonitor(IConfigurationSection configSection, NLogLoggingConfiguration nlogConfig)
            {
                _nlogConfig = nlogConfig;
                _registerChangeCallback = configSection.GetReloadToken().RegisterChangeCallback((s) => ReloadConfigurationSection((IConfigurationSection)s), configSection);
            }

            private void ReloadConfigurationSection(IConfigurationSection configSection)
            {
                _nlogConfig?.ReloadConfigurationSection(configSection);
            }

            public void Dispose()
            {
                _nlogConfig = null; // Disconnect to allow garbage collection
                var registerChangeCallback = _registerChangeCallback;
                _registerChangeCallback = null;
                registerChangeCallback?.Dispose();
            }
        }
    }
}