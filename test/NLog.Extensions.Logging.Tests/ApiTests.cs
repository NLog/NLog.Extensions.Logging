namespace NLog.Extensions.Logging.Tests
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using System.Reflection;
    using NLog.Config;
    using NLog.Extensions.Logging;
    using NLog.LayoutRenderers;
    using Xunit;

    /// <summary>
    /// Test the characteristics of the API. Config of the API is tested in <see cref="NLog.UnitTests.Config.ConfigApiTests"/>
    /// </summary>
    public class ApiTests
    {
        private readonly Type[] allTypes;
        private readonly Assembly nlogExtensionAssembly = typeof(NLogLoggerFactory).Assembly;
        private readonly Dictionary<Type, int> typeUsageCount = new Dictionary<Type, int>();

        public ApiTests()
        {
            allTypes = nlogExtensionAssembly.GetTypes();
        }

        [Fact]
        public void PublicEnumsTest()
        {
            foreach (Type type in allTypes)
            {
                if (!type.IsPublic)
                {
                    continue;
                }

                if (type.IsEnum || type.IsInterface)
                {
                    typeUsageCount[type] = 0;
                }
            }

            typeUsageCount[typeof(IInstallable)] = 1;

            foreach (Type type in allTypes)
            {
                if (type.IsGenericTypeDefinition)
                {
                    continue;
                }

                if (type.BaseType != null)
                {
                    IncrementUsageCount(type.BaseType);
                }

                foreach (var iface in type.GetInterfaces())
                {
                    IncrementUsageCount(iface);
                }

                foreach (var method in type.GetMethods())
                {
                    if (method.IsGenericMethodDefinition)
                    {
                        continue;
                    }

                    // Console.WriteLine("  {0}", method.Name);
                    try
                    {
                        IncrementUsageCount(method.ReturnType);

                        foreach (var p in method.GetParameters())
                        {
                            IncrementUsageCount(p.ParameterType);
                        }
                    }
                    catch (Exception ex)
                    {
                        // this sometimes throws on .NET Compact Framework, but is not fatal
                        Console.WriteLine("EXCEPTION {0}", ex);
                    }
                }
            }

            var unusedTypes = new List<Type>();
            StringBuilder sb = new StringBuilder();

            foreach (var kvp in typeUsageCount)
            {
                if (kvp.Value == 0)
                {
                    Console.WriteLine("Type '{0}' is not used.", kvp.Key);
                    unusedTypes.Add(kvp.Key);
                    sb.Append(kvp.Key.FullName).Append("\n");
                }
            }

            Assert.Empty(unusedTypes);
        }

        private void IncrementUsageCount(Type type)
        {
            if (type.IsArray)
            {
                type = type.GetElementType();
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                IncrementUsageCount(type.GetGenericTypeDefinition());
                foreach (var parm in type.GetGenericArguments())
                {
                    IncrementUsageCount(parm);
                }
                return;
            }

            if (type.Assembly != nlogExtensionAssembly)
            {
                return;
            }

            if (typeUsageCount.ContainsKey(type))
            {
                typeUsageCount[type]++;
            }
        }

        [Fact]
        public void TypesInInternalNamespaceShouldBeInternalTest()
        {
            var notInternalTypes = allTypes
                .Where(t => t.Namespace != null && t.Namespace.Contains(".Internal"))
                .Where(t => !t.IsNested && (t.IsVisible || t.IsPublic))
                .Select(t => t.FullName)
                .ToList();

            Assert.Empty(notInternalTypes);
        }

        [Fact]
        public void AppDomainFixedOutput_Attribute_EnsureThreadAgnostic()
        {
            foreach (Type type in allTypes)
            {
                var appDomainFixedOutputAttribute = type.GetCustomAttribute<AppDomainFixedOutputAttribute>();
                if (appDomainFixedOutputAttribute != null)
                {
                    var threadAgnosticAttribute = type.GetCustomAttribute<ThreadAgnosticAttribute>();
                    Assert.True(!(threadAgnosticAttribute is null), $"{type.ToString()} is missing [ThreadAgnostic] attribute");
                }
            }
        }

        [Fact]
        public void WrapperLayoutRenderer_EnsureThreadAgnostic()
        {
            foreach (Type type in allTypes)
            {
                if (typeof(NLog.LayoutRenderers.Wrappers.WrapperLayoutRendererBase).IsAssignableFrom(type))
                {
                    if (type.IsAbstract || !type.IsPublic)
                        continue;   // skip non-concrete types, enumerations, and private nested types

                    Assert.True(type.IsDefined(typeof(ThreadAgnosticAttribute), true), $"{type.ToString()} is missing [ThreadAgnostic] attribute.");
                }
            }
        }

        [Fact]
        public void SingleDefaultConfigOption()
        {
            string prevDefaultPropertyName = null;

            foreach (Type type in allTypes)
            {
                prevDefaultPropertyName = null;

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var prop in properties)
                {
                    var defaultParameter = prop.GetCustomAttribute<DefaultParameterAttribute>();
                    if (defaultParameter != null)
                    {
                        Assert.True(prevDefaultPropertyName == null, prevDefaultPropertyName?.ToString());
                        prevDefaultPropertyName = prop.Name;
                        Assert.True(type.IsSubclassOf(typeof(NLog.LayoutRenderers.LayoutRenderer)), type.ToString());
                    }
                }
            }
        }

        [Fact]
        public void ValidateLayoutRendererTypeAlias()
        {
            // These class-names should be repaired with next major version bump
            // Do NOT add more incorrect class-names to this exlusion-list
            HashSet<string> oldFaultyClassNames = new HashSet<string>()
            {
                "MicrosoftConsoleLayoutRenderer",
            };
            foreach (Type type in allTypes)
            {
                if (type.IsSubclassOf(typeof(LayoutRenderer)))
                {
                    var layoutRendererAttributes = type.GetCustomAttributes<LayoutRendererAttribute>()?.ToArray() ?? new LayoutRendererAttribute[0];
                    if (layoutRendererAttributes.Length == 0)
                    {
                        Assert.True(type.IsAbstract, $"{type} without LayoutRendererAttribute must be abstract");
                    }
                    else
                    {
                        Assert.False(type.IsAbstract, $"{type} with LayoutRendererAttribute cannot be abstract");

                        if (!oldFaultyClassNames.Contains(type.Name))
                        {
                            var typeAlias = layoutRendererAttributes.First().Name.Replace("-", "");
                            Assert.Equal(typeAlias + "LayoutRenderer", type.Name, StringComparer.OrdinalIgnoreCase);
                        }
                    }

                    Assert.Equal("NLog.Extensions.Logging", type.Namespace);
                }
            }
        }

        [Fact]
        public void ValidateConfigurationItemFactory()
        {
            ConfigurationItemFactory.Default = null;    // Reset

            LogManager.Setup().SetupExtensions(ext => ext.RegisterConfigSettings(null));

            var missingTypes = new List<string>();

            foreach (Type type in allTypes)
            {
                if (!type.IsPublic || !type.IsClass || type.IsAbstract)
                    continue;

                if (typeof(NLog.Targets.Target).IsAssignableFrom(type))
                {
                    var configAttribs = type.GetCustomAttributes<NLog.Targets.TargetAttribute>(false);
                    Assert.NotEmpty(configAttribs);

                    foreach (var configName in configAttribs)
                    {
                        if (!ConfigurationItemFactory.Default.TargetFactory.TryCreateInstance(configName.Name, out var target))
                        {
                            Console.WriteLine(configName.Name);
                            missingTypes.Add(configName.Name);
                        }
                        else if (type != target.GetType())
                        {
                            Console.WriteLine(type.Name);
                            missingTypes.Add(type.Name);
                        }
                    }
                }
                else if (typeof(NLog.Layouts.Layout).IsAssignableFrom(type))
                {
                    var configAttribs = type.GetCustomAttributes<NLog.Layouts.LayoutAttribute>(false);
                    Assert.NotEmpty(configAttribs);

                    foreach (var configName in configAttribs)
                    {
                        if (!ConfigurationItemFactory.Default.LayoutFactory.TryCreateInstance(configName.Name, out var layout))
                        {
                            Console.WriteLine(configName.Name);
                            missingTypes.Add(configName.Name);
                        }
                        else if (type != layout.GetType())
                        {
                            Console.WriteLine(type.Name);
                            missingTypes.Add(type.Name);
                        }
                    }
                }
                else if (typeof(NLog.LayoutRenderers.LayoutRenderer).IsAssignableFrom(type))
                {
                    var configAttribs = type.GetCustomAttributes<NLog.LayoutRenderers.LayoutRendererAttribute>(false);
                    Assert.NotEmpty(configAttribs);

                    foreach (var configName in configAttribs)
                    {
                        if (!ConfigurationItemFactory.Default.LayoutRendererFactory.TryCreateInstance(configName.Name, out var layoutRenderer))
                        {
                            Console.WriteLine(configName.Name);
                            missingTypes.Add(configName.Name);
                        }
                        else if (type != layoutRenderer.GetType())
                        {
                            Console.WriteLine(type.Name);
                            missingTypes.Add(type.Name);
                        }
                    }

                    if (typeof(NLog.LayoutRenderers.Wrappers.WrapperLayoutRendererBase).IsAssignableFrom(type))
                    {
                        var wrapperAttribs = type.GetCustomAttributes<NLog.LayoutRenderers.AmbientPropertyAttribute>(false);
                        if (wrapperAttribs?.Any() == true)
                        {
                            foreach (var ambientName in wrapperAttribs)
                            {
                                if (!ConfigurationItemFactory.Default.AmbientRendererFactory.TryCreateInstance(ambientName.Name, out var layoutRenderer))
                                {
                                    Console.WriteLine(ambientName.Name);
                                    missingTypes.Add(ambientName.Name);
                                }
                                else if (type != layoutRenderer.GetType())
                                {
                                    Console.WriteLine(type.Name);
                                    missingTypes.Add(type.Name);
                                }
                            }
                        }
                    }
                }
            }

            Assert.Empty(missingTypes);
        }
    }
}