using System;
using System.Reflection;
#if !NETCORE1_0
using Microsoft.Extensions.Logging;
#endif
using NLog.Common;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Provider logger for NLog + Microsoft.Extensions.Logging
    /// </summary>
 #if !NETCORE1_0
    [ProviderAlias("NLog")]
#endif
    public class NLogLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
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
            :this(null)
        {
        }

        /// <summary>
        /// New provider with options
        /// </summary>
        /// <param name="options"></param>
        public NLogLoggerProvider(NLogProviderOptions options)
            :this(options, null)
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
                LogFactory.Flush();
            }
        }

        /// <summary>
        /// Ignore assemblies for ${callsite}
        /// </summary>
        private static void RegisterHiddenAssembliesForCallSite()
        {
            InternalLogger.Debug("Hide assemblies for callsite");
            LogManager.AddHiddenAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);
            NLog.Config.ConfigurationItemFactory.Default.RegisterItemsFromAssembly(typeof(NLogLoggerProvider).GetTypeInfo().Assembly);

#if !NETCORE1_0
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in allAssemblies)
            {
                if (assembly.FullName.StartsWith("NLog.Extensions.Logging,", StringComparison.OrdinalIgnoreCase)
                    || assembly.FullName.StartsWith("NLog.Web,", StringComparison.OrdinalIgnoreCase)
                    || assembly.FullName.StartsWith("NLog.Web.AspNetCore,", StringComparison.OrdinalIgnoreCase)
                    || assembly.FullName.StartsWith("Microsoft.Extensions.Logging,", StringComparison.OrdinalIgnoreCase)
                    || assembly.FullName.StartsWith("Microsoft.Extensions.Logging.Abstractions,", StringComparison.OrdinalIgnoreCase)
                    || assembly.FullName.StartsWith("Microsoft.Extensions.Logging.Filter,", StringComparison.OrdinalIgnoreCase)
                    || assembly.FullName.StartsWith("Microsoft.Logging,", StringComparison.OrdinalIgnoreCase))
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

#if NETCORE1_0
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
                    InternalLogger.Debug(ex, "Hiding assembly {0} failed. This could influence the ${callsite}", assemblyName);
                }
            }
        }
#endif
    }
}


