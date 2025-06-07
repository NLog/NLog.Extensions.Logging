using System;
using System.Text;
using Microsoft.Extensions.Hosting;
using NLog.Config;
using NLog.LayoutRenderers;

#if NETSTANDARD2_0
using IHostEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;
#endif

namespace NLog.Extensions.Hosting
{
    /// <summary>
    /// Rendering development environment. <see cref="IHostingEnvironment.EnvironmentName" />
    /// </summary>
    /// <remarks>
    /// <code>${host-environment}</code>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Host-Environment-layout-renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("host-environment")]
    [ThreadAgnostic]
    public class HostEnvironmentLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Provides access to the current IHostEnvironment
        /// </summary>
        /// <returns>IHostEnvironment or <c>null</c></returns>
        internal IHostEnvironment? HostEnvironment => _hostEnvironment ?? (_hostEnvironment = ResolveHostEnvironment());
        private IHostEnvironment? _hostEnvironment;
        private string? _environmentName;

        /// <inheritdoc />
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var environmentName = _environmentName ?? (_environmentName = ResolveEnvironmentName());
            builder.Append(environmentName ?? "Production");
        }

        private IHostEnvironment? ResolveHostEnvironment()
        {
            return ResolveService<IHostEnvironment>();
        }

        private string? ResolveEnvironmentName()
        {
            string? environmentName = null;
            try
            {
                environmentName = HostEnvironment?.EnvironmentName;
            }
            catch
            {
                environmentName = null;
            }
            if (string.IsNullOrEmpty(environmentName))
            {
                environmentName = GetDotnetHostEnvironment("ASPNETCORE_ENVIRONMENT") ?? GetDotnetHostEnvironment("DOTNET_ENVIRONMENT");
            }
            return string.IsNullOrEmpty(environmentName) ? null : environmentName;
        }

        /// <inheritdoc/>
        protected override void CloseLayoutRenderer()
        {
            _hostEnvironment = null;
            _environmentName = null;
            base.CloseLayoutRenderer();
        }

        private static string? GetDotnetHostEnvironment(string variableName)
        {
            try
            {
                var environment = Environment.GetEnvironmentVariable(variableName);
                if (string.IsNullOrWhiteSpace(environment))
                    return null;

                return environment.Trim();
            }
            catch (Exception ex)
            {
                NLog.Common.InternalLogger.Error(ex, "Failed to lookup environment variable {0}", variableName);
                return null;
            }
        }
    }
}