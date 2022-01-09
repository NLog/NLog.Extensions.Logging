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
    ///     Helpers for IHostbuilder, netcore 2.1
    /// </summary>
    public static class ConfigureExtensions
    {
        /// <summary>
        ///     Enable and configure NLog as a logging provider for buildable generic host (.NET Core 2.1+).
        ///     Can be used in discrete containers as well.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>IHostBuilder for chaining</returns>
        public static IHostBuilder UseNLog(this IHostBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.UseNLog(null);
        }

        /// <summary>
        ///     Enable and configure NLog as a logging provider for buildable generic host (.NET Core 2.1+).
        ///     Can be used in discrete containers as well.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">NLogProviderOptions object to configure NLog behavior</param>
        /// <returns>IHostBuilder for chaining</returns>
        public static IHostBuilder UseNLog(this IHostBuilder builder, NLogProviderOptions options)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.ConfigureServices((builderContext, services) => AddNLogLoggerProvider(services, builderContext.Configuration, builderContext.HostingEnvironment, options, CreateNLogLoggerProvider));
            return builder;
        }

        private static void AddNLogLoggerProvider(IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment, NLogProviderOptions options, Func<IServiceProvider, IConfiguration, IHostEnvironment, NLogProviderOptions, NLogLoggerProvider> factory)
        {
            ConfigurationItemFactory.Default.RegisterItemsFromAssembly(typeof(ConfigureExtensions).GetTypeInfo().Assembly);
            LogManager.AddHiddenAssembly(typeof(ConfigureExtensions).GetTypeInfo().Assembly);
            services.TryAddNLogLoggingProvider((svc, addlogging) => svc.AddLogging(addlogging), configuration, options, (provider, cfg, opt) => factory(provider, cfg, hostEnvironment, opt));
        }

        private static NLogLoggerProvider CreateNLogLoggerProvider(IServiceProvider serviceProvider, IConfiguration configuration, IHostEnvironment hostEnvironment, NLogProviderOptions options)
        {
            NLogLoggerProvider provider = new NLogLoggerProvider(options);

            configuration = SetupConfiguration(serviceProvider, configuration);

            if (serviceProvider != null && provider.Options.RegisterServiceProvider)
            {
                provider.LogFactory.ServiceRepository.RegisterService(typeof(IServiceProvider), serviceProvider);
            }

            if (configuration != null)
            {
                provider.Configure(configuration.GetSection("Logging:NLog"));
                provider.TryLoadConfigurationFromSection(configuration);
            }

            var contentRootPath = hostEnvironment?.ContentRootPath;
            if (!string.IsNullOrEmpty(contentRootPath))
            {
                TryLoadConfigurationFromContentRootPath(provider.LogFactory, contentRootPath);
            }

            if (provider.Options.ShutdownOnDispose)
            {
                provider.LogFactory.AutoShutdown = false;
            }

            return provider;
        }

        private static IConfiguration SetupConfiguration(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            configuration = configuration ?? (serviceProvider?.GetService(typeof(IConfiguration)) as IConfiguration);
            if (configuration != null)
            {
                ConfigSettingLayoutRenderer.DefaultConfiguration = configuration;
            }
            return configuration;
        }

        private static void TryLoadConfigurationFromContentRootPath(LogFactory logFactory, string contentRootPath)
        {
            logFactory.Setup().LoadConfiguration(config =>
            {
                if (config.Configuration.LoggingRules.Count == 0 && config.Configuration.AllTargets.Count == 0)
                {
                    var standardPath = System.IO.Path.Combine(contentRootPath, "NLog.config");
                    if (System.IO.File.Exists(standardPath))
                    {
                        config.Configuration = new XmlLoggingConfiguration(standardPath, config.LogFactory);
                    }
                    else
                    {
                        var lowercasePath = System.IO.Path.Combine(contentRootPath, "nlog.config");
                        if (System.IO.File.Exists(lowercasePath))
                        {
                            config.Configuration = new XmlLoggingConfiguration(lowercasePath, config.LogFactory);
                        }
                        else
                        {
                            config.Configuration = null;    // Perform default loading
                        }
                    }
                }
            });
        }
    }
}