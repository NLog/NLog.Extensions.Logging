﻿using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Config;
using NLog.Extensions.Logging;

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
            builder.ConfigureServices((builderContext, services) => AddNLogLoggerProvider(services, builderContext.Configuration, options, CreateNLogLoggerProvider));
            return builder;
        }

        private static void AddNLogLoggerProvider(IServiceCollection services, IConfiguration configuration, NLogProviderOptions options, Func<IServiceProvider, IConfiguration, NLogProviderOptions, NLogLoggerProvider> factory)
        {
            ConfigurationItemFactory.Default.RegisterItemsFromAssembly(typeof(ConfigureExtensions).GetTypeInfo().Assembly);
            LogManager.AddHiddenAssembly(typeof(ConfigureExtensions).GetTypeInfo().Assembly);
            services.TryAddNLogLoggingProvider((svc, addlogging) => svc.AddLogging(addlogging), configuration, options, factory);
        }

        private static NLogLoggerProvider CreateNLogLoggerProvider(IServiceProvider serviceProvider, IConfiguration configuration, NLogProviderOptions options)
        {
            NLogLoggerProvider provider = new NLogLoggerProvider(options);
            configuration = SetupConfiguration(serviceProvider, configuration);
            if (configuration != null)
            {
                provider.Configure(configuration.GetSection("Logging:NLog"));
                provider.TryLoadConfigurationFromSection(configuration);
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
    }
}