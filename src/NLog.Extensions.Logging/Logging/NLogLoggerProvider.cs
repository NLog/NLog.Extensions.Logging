using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NLog.Common;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Provider logger for NLog + Microsoft.Extensions.Logging
    /// </summary>
#if !NETCORE1_0
    [ProviderAlias("NLog")]
#endif
    public class NLogLoggerProvider : ILoggerProvider
    {
        private readonly NLogBeginScopeParser _beginScopeParser;

        /// <summary>
        /// NLog options
        /// </summary>
        public NLogProviderOptions Options { get; set; }

        /// <summary>
        /// NLog Factory
        /// </summary>
        public LogFactory LogFactory { get; }

        /// <summary>
        /// New provider with default options, see <see cref="Options"/>
        /// </summary>
        public NLogLoggerProvider()
            : this(null)
        {
        }

        /// <summary>
        /// New provider with options
        /// </summary>
        /// <param name="options"></param>
        public NLogLoggerProvider(NLogProviderOptions options)
            : this(options, null)
        {
        }

        /// <summary>
        /// New provider with options
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logFactory">Optional isolated NLog LogFactory</param>
        public NLogLoggerProvider(NLogProviderOptions options, LogFactory logFactory)
        {
            LogFactory = logFactory ?? LogManager.LogFactory;
            Options = options ?? NLogProviderOptions.Default;
            _beginScopeParser = new NLogBeginScopeParser(options);
            RegisterHiddenAssembliesForCallSite();
        }

        /// <summary>
        /// Create a logger with the name <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the logger to be created.</param>
        /// <returns>New Logger</returns>
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string name)
        {
            return new NLogLogger(LogFactory.GetLogger(name), Options, _beginScopeParser);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Options.ShutdownOnDispose)
                {
                    LogManager.Shutdown();  // TODO Fix global static. Instead use LogFactory-property
                }
                else
                {
                    LogFactory.Flush();
                }
            }
        }

        /// <summary>
        /// Ignore assemblies for ${callsite}
        /// </summary>
        private static void RegisterHiddenAssembliesForCallSite()
        {
            InternalLogger.Debug("Hide assemblies for callsite");
            LogManager.AddHiddenAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
            Config.ConfigurationItemFactory.Default.RegisterItemsFromAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);

#if !NETCORE1_0
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in allAssemblies)
            {
                if (ShouldAddHiddenAssembly(assembly))
                {
                    LogManager.AddHiddenAssembly(assembly);
                }
            }
#else
            SafeAddHiddenAssembly("Microsoft.Logging");
            SafeAddHiddenAssembly("Microsoft.Extensions.Logging");
            SafeAddHiddenAssembly("Microsoft.Extensions.Logging.Abstractions");

            //try the Filter ext, this one is not mandatory so could fail
            SafeAddHiddenAssembly("Microsoft.Extensions.Logging.Filter", false);
#endif
        }

#if !NETCORE1_0
        private static bool ShouldAddHiddenAssembly(Assembly assembly)
        {
            var assemblyFullName = assembly?.FullName;
            if (string.IsNullOrEmpty(assemblyFullName))
                return false;

            foreach (var hiddenAssemblyPrefix in HiddenAssemblyPrefixes)
                if (assemblyFullName.StartsWith(hiddenAssemblyPrefix, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        private static readonly string[] HiddenAssemblyPrefixes = new[]
        {
            "NLog.Extensions.Logging,",
            "NLog.Web,",
            "NLog.Web.AspNetCore,",
            "Microsoft.Extensions.Logging,",
            "Microsoft.Extensions.Logging.Abstractions,",
            "Microsoft.Extensions.Logging.Filter,",
            "Microsoft.Logging,"
        };
#else
        private static void SafeAddHiddenAssembly(string assemblyName, bool logOnException = true)
        {
            try
            {
                InternalLogger.Trace("Hide {0}", assemblyName);
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
                LogManager.AddHiddenAssembly(assembly);
            }
            catch (Exception ex)
            {
                if (logOnException)
                {
                    InternalLogger.Debug(ex, "Hiding assembly {0} failed. This could influence the ${{callsite}}", assemblyName);
                }
            }
        }
#endif
        }
}


