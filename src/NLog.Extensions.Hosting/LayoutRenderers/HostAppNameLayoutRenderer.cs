using System.Text;
using Microsoft.Extensions.Hosting;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NLog.Extensions.Hosting
{
    /// <summary>
    /// Rendering development environment. <see cref="IHostEnvironment.ApplicationName" />
    /// </summary>
    /// <remarks>
    /// <code>${host-environment}</code>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Host-AppName-layout-renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("host-appname")]
    [ThreadAgnostic]
    public class HostAppNameLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Provides access to the current IHostEnvironment
        /// </summary>
        /// <returns>IHostEnvironment or <c>null</c></returns>
        internal IHostEnvironment? HostEnvironment => _hostEnvironment ?? (_hostEnvironment = ResolveHostEnvironment());
        private IHostEnvironment? _hostEnvironment;
        private string? _hostAppName;
        private static string? _currentProcessName;

        /// <inheritdoc />
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var environmentName = _hostAppName ?? (_hostAppName = ResolveHostAppName());
            builder.Append(environmentName ?? ResolveProcessName());
        }

        private IHostEnvironment? ResolveHostEnvironment()
        {
            return ResolveService<IHostEnvironment>();
        }

        private string? ResolveHostAppName()
        {
            try
            {
                var appName = HostEnvironment?.ApplicationName;
                return string.IsNullOrWhiteSpace(appName) ? null : appName;
            }
            catch
            {
                return null;
            }
        }

        private static string ResolveProcessName()
        {
            if (_currentProcessName is null)
                _currentProcessName = System.Diagnostics.Process.GetCurrentProcess()?.ProcessName ?? "UnknownHostName";
            return _currentProcessName;
        }

        /// <inheritdoc/>
        protected override void CloseLayoutRenderer()
        {
            _hostEnvironment = null;
            _hostAppName = null;
            base.CloseLayoutRenderer();
        }
    }
}
