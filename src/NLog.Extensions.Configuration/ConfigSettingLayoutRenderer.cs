using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NLog.Config;
using NLog.Layouts;
using NLog.LayoutRenderers;
using System.Text;

namespace NLog.Extensions.Configuration
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
    public class ConfigSettingLayoutRenderer : LayoutRenderer
    {
        internal IConfigurationRoot _configurationRoot;

        private static readonly Dictionary<string, WeakReference<IConfigurationRoot>> _cachedConfigFiles = new Dictionary<string, WeakReference<IConfigurationRoot>>();

        /// <summary>
        /// Global Configuration Container. Used if <see cref="FileName" /> has default value
        /// </summary>
        public static IConfiguration DefaultConfiguration { get; set; }

        ///<summary>
        /// Name of the setting
        ///</summary>
        [RequiredParameter]
        [DefaultParameter]
        public string Name { get => _name; set => _name = value?.Replace(".", ":"); }
        private string _name;

        ///<summary>
        /// The default value to render if the setting value is null.
        ///</summary>
        public string Default { get; set; }

        /// <summary>
        /// Configuration FileName (Multiple filenames can be split using '|' pipe-character)
        /// </summary>
        /// <remarks>Relative paths are automatically prefixed with ${basedir}</remarks>
        public Layout FileName { get; set; } = DefaultFileName;
        private const string DefaultFileName = "appsettings.json|appsettings.${environment:variable=ASPNETCORE_ENVIRONMENT}.json";

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            _configurationRoot = null;
            base.InitializeLayoutRenderer();
        }

        /// <inheritdoc/>
        protected override void CloseLayoutRenderer()
        {
            _configurationRoot = null;
            base.CloseLayoutRenderer();
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(_name))
                return;

            string value = null;
            var configurationRoot = TryGetConfigurationRoot();
            if (configurationRoot != null)
            {
                value = configurationRoot[_name];
            }

            builder.Append(value ?? Default);
        }

        private IConfiguration TryGetConfigurationRoot()
        {
            if (DefaultConfiguration != null)
            {
                var simpleLayout = FileName as SimpleLayout;
                if (simpleLayout == null || string.IsNullOrEmpty(simpleLayout.Text) || ReferenceEquals(simpleLayout.Text, DefaultFileName))
                {
                    if (_configurationRoot != null)
                        _configurationRoot = null;
                    return DefaultConfiguration;
                }
            }

            if (_configurationRoot != null)
                return _configurationRoot;

            var fileNames = FileName?.Render(LogEventInfo.CreateNullEvent());
            if (!string.IsNullOrEmpty(fileNames))
            {
                return _configurationRoot = LoadFileConfiguration(fileNames);
            }

            return null;
        }

        private IConfigurationRoot LoadFileConfiguration(string fileNames)
        {
            lock (_cachedConfigFiles)
            {
                if (_cachedConfigFiles.TryGetValue(fileNames, out var wearkConfigRoot) && wearkConfigRoot.TryGetTarget(out var configRoot))
                {
                    return configRoot;
                }
                else
                {
                    configRoot = BuildConfigurationRoot(fileNames);
                    _cachedConfigFiles[fileNames] = new WeakReference<IConfigurationRoot>(configRoot);
                    return configRoot;
                }
            }
        }

        private static IConfigurationRoot BuildConfigurationRoot(string fileNames)
        {
            var configBuilder = new ConfigurationBuilder();
            string baseDir = null;
            foreach (var fileName in fileNames.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var fullPath = fileName;
                if (!System.IO.Path.IsPathRooted(fullPath))
                {
                    if (baseDir == null)
                        baseDir = new BaseDirLayoutRenderer().Render(LogEventInfo.CreateNullEvent()) ?? string.Empty;
                    fullPath = System.IO.Path.Combine(baseDir, fileName);
                }

                AddFileConfiguration(configBuilder, fullPath);
            }

            return configBuilder.Build();
        }

        private static void AddFileConfiguration(ConfigurationBuilder configBuilder, string fullPath)
        {
            if (System.IO.File.Exists(fullPath))
            {
                // NOTE! Decided not to monitor for changes, as it would require access to dipose this monitoring again
                if (string.Equals(System.IO.Path.GetExtension(fullPath), ".json", StringComparison.OrdinalIgnoreCase))
                {
                    configBuilder.AddJsonFile(fullPath);
                }
                else if (string.Equals(System.IO.Path.GetExtension(fullPath), ".xml", StringComparison.OrdinalIgnoreCase))
                {
                    configBuilder.AddXmlFile(fullPath);
                }
                else if (string.Equals(System.IO.Path.GetExtension(fullPath), ".ini", StringComparison.OrdinalIgnoreCase))
                {
                    configBuilder.AddIniFile(fullPath);
                }
                else
                {
                    Common.InternalLogger.Info("configSetting - Skipping FileName with unknown file-extension: {0}", fullPath);
                }
            }
            else
            {
                Common.InternalLogger.Info("configSetting - Skipping FileName as file doesnt't exists: {0}", fullPath);
            }
        }
    }
}
