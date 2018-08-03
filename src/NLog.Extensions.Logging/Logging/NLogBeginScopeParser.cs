using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using NLog.Common;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Converts Microsoft Extension Logging BeginScope into NLog NestedDiagnosticsLogicalContext + MappedDiagnosticsLogicalContext
    /// </summary>
    internal class NLogBeginScopeParser
    {
        private readonly NLogProviderOptions _options;
        private readonly ConcurrentDictionary<Type, KeyValuePair<Func<object, object>, Func<object, object>>> _scopeStateExtractors = new ConcurrentDictionary<Type, KeyValuePair<Func<object, object>, Func<object, object>>>();

        public NLogBeginScopeParser(NLogProviderOptions options)
        {
            _options = options ?? NLogProviderOptions.Default;
        }

        public IDisposable ParseBeginScope<T>(T state)
        {
            if (_options.CaptureMessageProperties)
            {
                if (state is IReadOnlyList<KeyValuePair<string, object>> scopePropertyList)
                {
                    return ScopeProperties.CaptureScopeProperties(scopePropertyList);
                }

                if (!(state is string))
                {
                    if (state is System.Collections.IEnumerable scopePropertyCollection)
                        return ScopeProperties.CaptureScopeProperties(scopePropertyCollection, _scopeStateExtractors);
                    else
                        return ScopeProperties.CaptureScopeProperty(state, _scopeStateExtractors);
                }
                else
                {
                    return NestedDiagnosticsLogicalContext.Push(state);
                }
            }

            return CreateDiagnosticLogicalContext(state);
        }

        public static IDisposable CreateDiagnosticLogicalContext<T>(T state)
        {
            try
            {
#if NETSTANDARD
                return NestedDiagnosticsLogicalContext.Push(state); // AsyncLocal has no requirement to be Serializable
#else
                // TODO Add support for Net46 in NLog (AsyncLocal), then we only have to do this check for legacy Net451 (CallContext)
                if (state?.GetType().IsSerializable ?? true)
                    return NestedDiagnosticsLogicalContext.Push(state);
                else
                    return NestedDiagnosticsLogicalContext.Push(state.ToString());  // Support HostingLogScope, ActionLogScope, FormattedLogValues and others
#endif
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, "Exception in BeginScope push NestedDiagnosticsLogicalContext");
                return null;
            }
        }

        private class ScopeProperties : IDisposable
        {
            Stack<IDisposable> _properties;

            /// <summary>
            /// Properties, never null and lazy init
            /// </summary>
            Stack<IDisposable> Properties => _properties ?? (_properties = new Stack<IDisposable>());

            public ScopeProperties(int initialCapacity = 0)
            {
                if (initialCapacity > 0)
                    _properties = new Stack<IDisposable>(initialCapacity);
            }

            public static ScopeProperties CaptureScopeProperties(IReadOnlyList<KeyValuePair<string, object>> scopePropertyList)
            {
                ScopeProperties scope = new ScopeProperties(scopePropertyList.Count + 1);

                for (int i = 0; i < scopePropertyList.Count; ++i)
                {
                    var property = scopePropertyList[i];
                    if (i == scopePropertyList.Count - 1 && i > 0 && property.Key == NLogLogger.OriginalFormatPropertyName)
                        continue;   // Handle BeginScope("Hello {World}", "Earth")

                    scope.AddProperty(property.Key, property.Value);
                }

                scope.AddDispose(CreateDiagnosticLogicalContext(scopePropertyList));
                return scope;
            }

            public static ScopeProperties CaptureScopeProperties(System.Collections.IEnumerable scopePropertyCollection, ConcurrentDictionary<Type, KeyValuePair<Func<object, object>, Func<object, object>>> stateExractor)
            {
                ScopeProperties scope = new ScopeProperties();

                var keyValueExtractor = default(KeyValuePair<Func<object, object>, Func<object, object>>);
                foreach (var property in scopePropertyCollection)
                {
                    if (property == null)
                        break;

                    if (keyValueExtractor.Key == null)
                    {
                        if (!TryLookupExtractor(stateExractor, property.GetType(), out keyValueExtractor))
                            break;
                    }

                    AddKeyValueProperty(scope, keyValueExtractor, property);
                }

                scope.AddDispose(CreateDiagnosticLogicalContext(scopePropertyCollection));
                return scope;
            }

            public static IDisposable CaptureScopeProperty<TState>(TState scopeProperty, ConcurrentDictionary<Type, KeyValuePair<Func<object, object>, Func<object, object>>> stateExractor)
            {
                if (!TryLookupExtractor(stateExractor, scopeProperty.GetType(), out var keyValueExtractor))
                    return CreateDiagnosticLogicalContext(scopeProperty);

                var scope = new ScopeProperties();
                AddKeyValueProperty(scope, keyValueExtractor, scopeProperty);
                scope.AddDispose(CreateDiagnosticLogicalContext(scopeProperty));
                return scope;
            }

            private static void AddKeyValueProperty(ScopeProperties scope, KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor, object property)
            {
                try
                {
                    var propertyKey = keyValueExtractor.Key.Invoke(property);
                    var propertyValue = keyValueExtractor.Value.Invoke(property);
                    scope.AddProperty(propertyKey?.ToString() ?? string.Empty, propertyValue);
                }
                catch (Exception ex)
                {
                    InternalLogger.Debug(ex, "Exception in BeginScope add property");
                }
            }

            private static bool TryLookupExtractor(ConcurrentDictionary<Type, KeyValuePair<Func<object, object>, Func<object, object>>> stateExractor, Type propertyType, out KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor)
            {
                if (!stateExractor.TryGetValue(propertyType, out keyValueExtractor))
                {
                    try
                    {
                        return TryBuildExtractor(propertyType, out keyValueExtractor);
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Debug(ex, "Exception in BeginScope create property extractor");
                    }
                    finally
                    {
                        stateExractor[propertyType] = keyValueExtractor;
                    }
                }

                return keyValueExtractor.Key != null;
            }

            private static bool TryBuildExtractor(Type propertyType, out KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor)
            {
                keyValueExtractor = default(KeyValuePair<Func<object, object>, Func<object, object>>);

                var itemType = propertyType.GetTypeInfo();
                if (!itemType.IsGenericType || itemType.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
                    return false;

                var keyPropertyInfo = itemType.GetDeclaredProperty("Key");
                var valuePropertyInfo = itemType.GetDeclaredProperty("Value");
                if (valuePropertyInfo == null || keyPropertyInfo == null)
                    return false;

                var keyValuePairObjParam = System.Linq.Expressions.Expression.Parameter(typeof(object), "pair");
                var keyValuePairTypeParam = System.Linq.Expressions.Expression.Convert(keyValuePairObjParam, propertyType);

                var propertyKeyAccess = System.Linq.Expressions.Expression.Property(keyValuePairTypeParam, keyPropertyInfo);
                var propertyKeyAccessObj = System.Linq.Expressions.Expression.Convert(propertyKeyAccess, typeof(object));
                var propertyKeyLambda = System.Linq.Expressions.Expression.Lambda<Func<object, object>>(propertyKeyAccessObj, keyValuePairObjParam).Compile();

                var propertyValueAccess = System.Linq.Expressions.Expression.Property(keyValuePairTypeParam, valuePropertyInfo);
                var propertyValueLambda = System.Linq.Expressions.Expression.Lambda<Func<object, object>>(propertyValueAccess, keyValuePairObjParam).Compile();

                keyValueExtractor = new KeyValuePair<Func<object, object>, Func<object, object>>(propertyKeyLambda, propertyValueLambda);
                return true;
            }

            public void AddDispose(IDisposable disposable)
            {
                if (disposable != null)
                    Properties.Push(disposable);
            }

            public void AddProperty(string key, object value)
            {
                AddDispose(MappedDiagnosticsLogicalContext.SetScoped(key, value));
            }

            public void Dispose()
            {
                var properties = _properties;
                if (properties != null)
                {
                    IDisposable property = null;
                    while (properties.Count > 0)
                    {
                        try
                        {
                            property = properties.Pop();
                            property.Dispose();
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Debug(ex, "Exception in BeginScope dispose property {0}", property);
                        }
                    }
                }
            }

            public override string ToString()
            {
                return (_properties?.Count > 0 ? _properties.Peek()?.ToString() : null) ?? base.ToString();
            }
        }
    }
}
