#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NLog.LayoutRenderers;
using Xunit;

namespace NLog.Extensions.Hosting.Tests
{
    [CollectionDefinition("Host renderer environment", DisableParallelization = true)]
    public class HostRendererEnvironmentCollection
    {
    }

    internal static class HostLayoutRendererTestHelper
    {
        public static string Render<TLayoutRenderer>(string rendererName, string layout, IHostEnvironment? hostEnvironment = null)
            where TLayoutRenderer : LayoutRenderer, new()
        {
            using var logFactory = new LogFactory();
            var target = new Targets.MemoryTarget { Layout = layout };

            logFactory.Setup().SetupExtensions(extensions =>
            {
                extensions.RegisterLayoutRenderer<TLayoutRenderer>(rendererName);
                if (hostEnvironment is not null)
                    extensions.RegisterSingletonService(hostEnvironment);
            });
            logFactory.Setup().LoadConfiguration(configuration => configuration.ForLogger().WriteTo(target));

            return target.Layout.Render(LogEventInfo.CreateNullEvent());
        }
    }

    internal sealed class TestHostEnvironment : IHostEnvironment
    {
        private string _applicationName = string.Empty;
        private string _contentRootPath = string.Empty;
        private string _environmentName = string.Empty;

        public bool ThrowOnApplicationName { get; set; }
        public bool ThrowOnContentRootPath { get; set; }
        public bool ThrowOnEnvironmentName { get; set; }

        public string ApplicationName
        {
            get => ThrowOnApplicationName ? throw new InvalidOperationException() : _applicationName;
            set => _applicationName = value;
        }

        public string ContentRootPath
        {
            get => ThrowOnContentRootPath ? throw new InvalidOperationException() : _contentRootPath;
            set => _contentRootPath = value;
        }

        public string EnvironmentName
        {
            get => ThrowOnEnvironmentName ? throw new InvalidOperationException() : _environmentName;
            set => _environmentName = value;
        }

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    internal sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly Dictionary<string, string?> _originalValues = new Dictionary<string, string?>();

        public EnvironmentVariableScope(params string[] variableNames)
        {
            foreach (var variableName in variableNames)
            {
                _originalValues[variableName] = Environment.GetEnvironmentVariable(variableName);
                Environment.SetEnvironmentVariable(variableName, null);
            }
        }

        public void Set(string variableName, string? value)
        {
            Environment.SetEnvironmentVariable(variableName, value);
        }

        public void Dispose()
        {
            foreach (var variable in _originalValues)
                Environment.SetEnvironmentVariable(variable.Key, variable.Value);
        }
    }
}