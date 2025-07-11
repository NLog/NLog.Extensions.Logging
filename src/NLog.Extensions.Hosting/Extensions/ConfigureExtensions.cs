﻿using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Config;
using NLog.Extensions.Logging;

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
        public static IHostBuilder UseNLog(this IHostBuilder builder, NLogProviderOptions? options)
        {
            Guard.ThrowIfNull(builder);
            builder.ConfigureServices((builderContext, services) => AddNLogLoggerProvider(services, builderContext.Configuration, builderContext.HostingEnvironment, options, CreateNLogLoggerProvider));
            return builder;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">NLogProviderOptions object to configure NLog behavior</param>
        /// <param name="factoryBuilder">Initialize NLog LogFactory with NLog LoggingConfiguration.</param>
        /// <returns>IHostBuilder for chaining</returns>
        public static IHostBuilder UseNLog(this IHostBuilder builder, NLogProviderOptions options, Func<IServiceProvider, LogFactory> factoryBuilder)
        {
            Guard.ThrowIfNull(builder);
            builder.ConfigureServices((builderContext, services) => AddNLogLoggerProvider(services, builderContext.Configuration, builderContext.HostingEnvironment, options, (serviceProvider, config, hostEnv, opt) =>
            {
                RegisterHostNLogExtensions(LogManager.LogFactory, serviceProvider, hostEnv);
                config = serviceProvider.SetupNLogConfigSettings(config, LogManager.LogFactory);

                // Delay initialization of targets until we have loaded config-settings
                var logFactory = factoryBuilder(serviceProvider);
                var provider = CreateNLogLoggerProvider(serviceProvider, config, hostEnv, opt, logFactory);
                return provider;
            }));
            return builder;
        }

#if !NET6_0
        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>IHostApplicationBuilder for chaining</returns>
        public static IHostApplicationBuilder UseNLog(this IHostApplicationBuilder builder)
        {
            Guard.ThrowIfNull(builder);
            return builder.UseNLog(null);
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">NLogProviderOptions object to configure NLog behavior</param>
        /// <returns>IHostApplicationBuilder for chaining</returns>
        public static IHostApplicationBuilder UseNLog(this IHostApplicationBuilder builder, NLogProviderOptions? options)
        {
            Guard.ThrowIfNull(builder);
            builder.Services.TryAddNLogLoggingProvider((svc, addlogging) => svc.AddLogging(addlogging), builder.Configuration, options, (serviceProvider, config, opt) => CreateNLogLoggerProvider(serviceProvider, config, builder.Environment, opt));
            return builder;
        }

        /// <summary>
        /// Enable NLog as logging provider for Microsoft Extension Logging
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">NLogProviderOptions object to configure NLog behavior</param>
        /// <param name="factoryBuilder">Initialize NLog LogFactory with NLog LoggingConfiguration.</param>
        /// <returns>IHostApplicationBuilder for chaining</returns>
        public static IHostApplicationBuilder UseNLog(this IHostApplicationBuilder builder, NLogProviderOptions options, Func<IServiceProvider, LogFactory> factoryBuilder)
        {
            Guard.ThrowIfNull(builder);
            builder.Services.TryAddNLogLoggingProvider((svc, addlogging) => svc.AddLogging(addlogging), builder.Configuration, options, (serviceProvider, config, opt) =>
            {
                RegisterHostNLogExtensions(LogManager.LogFactory, serviceProvider, builder.Environment);
                config = serviceProvider.SetupNLogConfigSettings(config, LogManager.LogFactory);

                // Delay initialization of targets until we have loaded config-settings
                var logFactory = factoryBuilder(serviceProvider);
                var provider = CreateNLogLoggerProvider(serviceProvider, config, builder.Environment, opt, logFactory);
                return provider;
            });
            return builder;
        }
#endif

        private static void AddNLogLoggerProvider(IServiceCollection services, IConfiguration? hostConfiguration, IHostEnvironment? hostEnvironment, NLogProviderOptions? options, Func<IServiceProvider, IConfiguration?, IHostEnvironment?, NLogProviderOptions, NLogLoggerProvider> factory)
        {
            services.TryAddNLogLoggingProvider((svc, addlogging) => svc.AddLogging(addlogging), hostConfiguration, options, (provider, cfg, opt) => factory(provider, cfg, hostEnvironment, opt));
        }

        private static NLogLoggerProvider CreateNLogLoggerProvider(IServiceProvider serviceProvider, IConfiguration? hostConfiguration, IHostEnvironment? hostEnvironment, NLogProviderOptions options)
        {
            return CreateNLogLoggerProvider(serviceProvider, hostConfiguration, hostEnvironment, options, LogManager.LogFactory);
        }

        private static NLogLoggerProvider CreateNLogLoggerProvider(IServiceProvider serviceProvider, IConfiguration? hostConfiguration, IHostEnvironment? hostEnvironment, NLogProviderOptions options, LogFactory logFactory)
        {
            RegisterHostNLogExtensions(logFactory, serviceProvider, hostEnvironment);

            NLogLoggerProvider provider = serviceProvider.CreateNLogLoggerProvider(hostConfiguration, options, logFactory);

            string nlogConfigFile = string.Empty;
            var contentRootPath = hostEnvironment?.ContentRootPath;
            var environmentName = hostEnvironment?.EnvironmentName;
            if (!string.IsNullOrWhiteSpace(contentRootPath) || !string.IsNullOrWhiteSpace(environmentName))
            {
                provider.LogFactory.Setup().LoadConfiguration(cfg =>
                {
                    if (!IsLoggingConfigurationLoaded(cfg.Configuration))
                    {
                        nlogConfigFile = ResolveEnvironmentNLogConfigFile(contentRootPath, environmentName);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        cfg.Configuration = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    }
                });
            }

            if (!string.IsNullOrEmpty(nlogConfigFile))
            {
                provider.LogFactory.Setup().LoadConfigurationFromFile(nlogConfigFile, optional: true);
            }

            provider.LogFactory.Setup().SetupLogFactory(ext => ext.AddCallSiteHiddenAssembly(typeof(ConfigureExtensions).Assembly));
            return provider;
        }

        private static void RegisterHostNLogExtensions(LogFactory logFactory, IServiceProvider serviceProvider, IHostEnvironment? hostingEnvironment)
        {
            (logFactory ?? LogManager.LogFactory).Setup().SetupExtensions(ext =>
            {
                if (serviceProvider != null)
                    ext.RegisterServiceProvider(serviceProvider);
                if (hostingEnvironment != null)
                    ext.RegisterSingletonService(hostingEnvironment);
                ext.RegisterLayoutRenderer<HostEnvironmentLayoutRenderer>("host-environment");
                ext.RegisterLayoutRenderer<HostRootDirLayoutRenderer>("host-rootdir");
                ext.RegisterLayoutRenderer<HostAppNameLayoutRenderer>("host-appname");
            });
        }

        private static string ResolveEnvironmentNLogConfigFile(string? basePath, string? environmentName)
        {
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                if (!string.IsNullOrWhiteSpace(environmentName))
                {
                    var nlogConfigEnvFilePath = Path.Combine(basePath, $"nlog.{environmentName}.config");
                    if (File.Exists(nlogConfigEnvFilePath))
                        return Path.GetFullPath(nlogConfigEnvFilePath);
                    nlogConfigEnvFilePath = Path.Combine(basePath, $"NLog.{environmentName}.config");
                    if (File.Exists(nlogConfigEnvFilePath))
                        return Path.GetFullPath(nlogConfigEnvFilePath);
                }

                var nlogConfigFilePath = Path.Combine(basePath, "nlog.config");
                if (File.Exists(nlogConfigFilePath))
                    return Path.GetFullPath(nlogConfigFilePath);
                nlogConfigFilePath = Path.Combine(basePath, "NLog.config");
                if (File.Exists(nlogConfigFilePath))
                    return Path.GetFullPath(nlogConfigFilePath);
            }

            if (!string.IsNullOrWhiteSpace(environmentName))
                return $"nlog.{environmentName}.config";

            return string.Empty;
        }

        private static bool IsLoggingConfigurationLoaded(LoggingConfiguration cfg)
        {
            return cfg?.LoggingRules?.Count > 0 && cfg?.AllTargets?.Count > 0;
        }
    }
}