using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Config;
using NLog.Extensions.Logging;
#if NETSTANDARD2_0
using IHostEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;
#endif

namespace NLog.Extensions.Hosting
{
    /// <summary>
    ///     Helpers for IHostbuilder
    /// </summary>
    public static class ConfigureExtensions
    {
        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>IHostBuilder for chaining</returns>
        public static IHostBuilder UseNLog(this IHostBuilder builder)
        {
            Guard.ThrowIfNull(builder);
            return builder.UseNLog(null);
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">NLogProviderOptions object to configure NLog behavior</param>
        /// <returns>IHostBuilder for chaining</returns>
        public static IHostBuilder UseNLog(this IHostBuilder builder, NLogProviderOptions options)
        {
            Guard.ThrowIfNull(builder);
#if NETSTANDARD2_0
            builder.ConfigureServices((builderContext, services) => AddNLogLoggerProvider(services, builderContext.Configuration, null, options, CreateNLogLoggerProvider));
#else
            builder.ConfigureServices((builderContext, services) => AddNLogLoggerProvider(services, builderContext.Configuration, builderContext.HostingEnvironment, options, CreateNLogLoggerProvider));
#endif
            return builder;
        }

        private static void AddNLogLoggerProvider(IServiceCollection services, IConfiguration hostConfiguration, IHostEnvironment hostEnvironment, NLogProviderOptions options, Func<IServiceProvider, IConfiguration, IHostEnvironment, NLogProviderOptions, NLogLoggerProvider> factory)
        {
            services.TryAddNLogLoggingProvider((svc, addlogging) => svc.AddLogging(addlogging), hostConfiguration, options, (provider, cfg, opt) => factory(provider, cfg, hostEnvironment, opt));
        }

        private static NLogLoggerProvider CreateNLogLoggerProvider(IServiceProvider serviceProvider, IConfiguration hostConfiguration, IHostEnvironment hostEnvironment, NLogProviderOptions options)
        {
            NLogLoggerProvider provider = serviceProvider.CreateNLogLoggerProvider(hostConfiguration, options, null);

            var contentRootPath = hostEnvironment?.ContentRootPath;
            if (!string.IsNullOrWhiteSpace(contentRootPath))
            {
                TryLoadConfigurationFromContentRootPath(provider.LogFactory, contentRootPath, hostEnvironment?.EnvironmentName);
            }

            provider.LogFactory.Setup().SetupLogFactory(ext => ext.AddCallSiteHiddenAssembly(typeof(ConfigureExtensions).Assembly));
            return provider;
        }

        private static void TryLoadConfigurationFromContentRootPath(LogFactory logFactory, string contentRootPath, string environmentName)
        {
            logFactory.Setup().LoadConfiguration(config =>
            {
                if (IsLoggingConfigurationLoaded(config.Configuration))
                    return;

                if (!string.IsNullOrEmpty(environmentName))
                {
                    var nlogConfig = LoadXmlLoggingConfigurationFromPath(contentRootPath, $"NLog.{environmentName}.config", config.LogFactory) ??
                        LoadXmlLoggingConfigurationFromPath(contentRootPath, $"nlog.{environmentName}.config", config.LogFactory) ??
                        LoadXmlLoggingConfigurationFromPath(contentRootPath, "NLog.config", config.LogFactory) ??
                        LoadXmlLoggingConfigurationFromPath(contentRootPath, "nlog.config", config.LogFactory);
                    if (nlogConfig != null)
                        config.Configuration = nlogConfig;
                }
                else
                {
                    var nlogConfig = LoadXmlLoggingConfigurationFromPath(contentRootPath, "NLog.config", config.LogFactory) ??
                        LoadXmlLoggingConfigurationFromPath(contentRootPath, "nlog.config", config.LogFactory);
                    if (nlogConfig != null)
                        config.Configuration = nlogConfig;
                }
            });
        }

        private static LoggingConfiguration LoadXmlLoggingConfigurationFromPath(string contentRootPath, string nlogConfigFileName, LogFactory logFactory)
        {
            var standardPath = System.IO.Path.Combine(contentRootPath, nlogConfigFileName);
            return System.IO.File.Exists(standardPath) ?
                new XmlLoggingConfiguration(standardPath, logFactory) : null;
        }

        private static bool IsLoggingConfigurationLoaded(LoggingConfiguration cfg)
        {
            return cfg?.LoggingRules?.Count > 0 && cfg?.AllTargets?.Count > 0;
        }
    }
}