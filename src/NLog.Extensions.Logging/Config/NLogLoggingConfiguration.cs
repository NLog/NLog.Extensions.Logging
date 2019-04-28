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
                    _reloadConfiguration = state => ReloadConfigurationSection((IConfigurationSection) state);
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
            var configElement = new LoggingConfigurationElement(nlogConfig, new LoggingConfigurationElementContext(), true);
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
            nlogConfig.GetReloadToken().RegisterChangeCallback(_reloadConfiguration, nlogConfig);
        }

        private class LoggingConfigurationElementContext
        {
            public IConfigurationSection DefaultTargetParametersSection;
            public IConfigurationSection DefaultWrapperSection;
        }

        private class LoggingConfigurationElement : ILoggingConfigurationElement
        {
            private const string TargetKey = "target";
            private const string DefaultTargetParameters = "Default-target-parameters";
            private const string VariableKey = "Variable";
            private const string DefaultWrapper = "Default-wrapper";
            private readonly IConfigurationSection _configurationSection;
            private readonly LoggingConfigurationElementContext _context;
            private readonly string _nameOverride;
            private readonly bool _topElement;

            public LoggingConfigurationElement(IConfigurationSection configurationSection, LoggingConfigurationElementContext context, bool topElement, string nameOverride = null)
            {
                _configurationSection = configurationSection;
                _context = context;
                _nameOverride = nameOverride;
                _topElement = topElement;
                if (topElement && bool.TryParse(configurationSection["autoreload"], out var autoreload))
                {
                    AutoReload = autoreload;
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
                        yield return new LoggingConfigurationElement(variable, _context, false, VariableKey);
                    }
                }

                var targetsSection = !_topElement && _nameOverride == null && _configurationSection.Key.EqualsOrdinalIgnoreCase("targets");
                var defaultWrapper = GetDefaultWrapperSection();
                if (defaultWrapper != null)
                {
                    _context.DefaultWrapperSection = defaultWrapper;
                }

                var defaultTargetParameters = GetDefaultTargetParametersSection();
                if (defaultTargetParameters != null)
                {
                    _context.DefaultTargetParametersSection = defaultTargetParameters;
                }
                if (targetsSection)
                {
                    foreach (var loggingConfigurationElement in YieldCapturedContextSections())
                    {
                        yield return loggingConfigurationElement;
                    }
                }

                var children = _configurationSection.GetChildren();
                foreach (var child in children)
                {
                    var firstChildValue = child?.GetChildren()?.FirstOrDefault();
                    if (firstChildValue == null)
                    {
                        continue; // Simple value without children
                    }

                    if (IsTargetWithinWrapper(child))
                    {
                        yield return new LoggingConfigurationElement(firstChildValue, _context, false, TargetKey);
                    }
                    else
                    {
                        string nameOverride = null;
                        if (AlreadReadChild(child, variables, defaultWrapper, defaultTargetParameters))
                        {
                            continue;
                        }

                        if (targetsSection)
                        {
                            nameOverride = TargetKey;
                        }

                        yield return new LoggingConfigurationElement(child, _context, false, nameOverride);
                    }
                }
            }

            private IEnumerable<ILoggingConfigurationElement> YieldCapturedContextSections()
            {
                if (_context.DefaultWrapperSection != null)
                {
                    yield return new LoggingConfigurationElement(_context.DefaultWrapperSection, _context, true, DefaultWrapper);
                    _context.DefaultWrapperSection = null;
                }

                if (_context.DefaultTargetParametersSection != null)
                {
                    foreach (var targetParameters in _context.DefaultTargetParametersSection.GetChildren())
                    {
                        yield return new LoggingConfigurationElement(targetParameters, _context, true, DefaultTargetParameters);
                    }

                    _context.DefaultTargetParametersSection = null;
                }
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

            private bool AlreadReadChild(IConfigurationSection child, IConfigurationSection variables, IConfigurationSection defaultWrapper, IConfigurationSection defaultTargetParameters)
            {
                if (variables != null && child.Key.EqualsOrdinalIgnoreCase(variables.Key))
                {
                    return true;
                }

                if (_topElement)
                {
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
                return defaultTargetParameters;
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