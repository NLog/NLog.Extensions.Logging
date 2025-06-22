using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Hosting;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NLog.Extensions.Hosting
{
    /// <summary>
    /// Rendering Application Host <see cref="IHostEnvironment.ContentRootPath" />
    /// </summary>
    /// <remarks>
    /// <code>${host-rootdir}</code>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Host-RootDir-layout-renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("host-rootdir")]
    [ThreadAgnostic]
    public class HostRootDirLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Provides access to the current IHostEnvironment
        /// </summary>
        /// <returns>IHostEnvironment or <c>null</c></returns>
        internal IHostEnvironment? HostEnvironment => _hostEnvironment ?? (_hostEnvironment = ResolveHostEnvironment());
        private IHostEnvironment? _hostEnvironment;
        private string? _contentRootPath;
        private static string? _currentAppPath;

        /// <inheritdoc />
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var contentRootPath = _contentRootPath ?? (_contentRootPath = ResolveContentRootPath());
            builder.Append(contentRootPath ?? ResolveCurrentAppDirectory());
        }

        private IHostEnvironment? ResolveHostEnvironment()
        {
            return ResolveService<IHostEnvironment>();
        }

        private string? ResolveContentRootPath()
        {
            string? contentRootPath = null;
            try
            {
                contentRootPath = HostEnvironment?.ContentRootPath;
            }
            catch
            {
                contentRootPath = null;
            }
            if (string.IsNullOrEmpty(contentRootPath))
            {
                contentRootPath = GetDotnetHostEnvironment("ASPNETCORE_CONTENTROOT") ?? GetDotnetHostEnvironment("DOTNET_CONTENTROOT");
            }
            return TrimEndDirectorySeparator(contentRootPath);
        }

        private static string? TrimEndDirectorySeparator(string? directoryPath)
        {
            return (directoryPath is null || string.IsNullOrEmpty(directoryPath)) ? null : directoryPath.TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar);
        }

        private static string? ResolveCurrentAppDirectory()
        {
            if (!string.IsNullOrEmpty(_currentAppPath))
                return _currentAppPath;

            var currentAppPath = AppContext.BaseDirectory;

            try
            {
                var currentBasePath = Environment.CurrentDirectory;
                var normalizeCurDir = Path.GetFullPath(currentBasePath).TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                var normalizeAppDir = Path.GetFullPath(currentAppPath).TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                if (string.IsNullOrEmpty(normalizeCurDir) || !normalizeCurDir.StartsWith(normalizeAppDir, StringComparison.OrdinalIgnoreCase))
                {
                    currentBasePath = currentAppPath; // Avoid using Windows-System32 as current directory
                }
                return _currentAppPath = TrimEndDirectorySeparator(currentBasePath);
            }
            catch
            {
                // Not supported or access denied
                return _currentAppPath = TrimEndDirectorySeparator(currentAppPath);
            }
        }

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            ResolveCurrentAppDirectory();   // Capture current directory at startup, before it changes
            base.InitializeLayoutRenderer();
        }

        /// <inheritdoc/>
        protected override void CloseLayoutRenderer()
        {
            _hostEnvironment = null;
            _contentRootPath = null;
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