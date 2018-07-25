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
                if (state is IReadOnlyList<KeyValuePair<string, object>> contextProperties)
                {
                    return ScopeProperties.CreateFromState(contextProperties);
                }

                if (!(state is string))
                {
                    var scope = ScopeProperties.CreateFromStateExtractor(state, _scopeStateExtractors);
                    if (scope != null)
                        return scope;
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
                    return NestedDiagnosticsLogicalContext.Push(state.ToString());  // Support ViewComponentLogScope, ActionLogScope and others
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
            List<IDisposable> _properties;

            /// <summary>
            /// Properties, never null and lazy init
            /// </summary>
            List<IDisposable> Properties => _properties ?? (_properties = new List<IDisposable>());

            public static ScopeProperties CreateFromState(IReadOnlyList<KeyValuePair<string, object>> messageProperties)
            {
                ScopeProperties scope = new ScopeProperties();

                for (int i = 0; i < messageProperties.Count; ++i)
                {
                    var property = messageProperties[i];
                    scope.AddProperty(property.Key, property.Value);
                }

                scope.AddDispose(CreateDiagnosticLogicalContext(messageProperties));
                return scope;
            }

            public static bool TryCreateExtractor<T>(ConcurrentDictionary<Type, KeyValuePair<Func<object, object>, Func<object, object>>> stateExractor, T property, out KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor)
            {
                Type propertyType = property.GetType();

                if (!stateExractor.TryGetValue(propertyType, out keyValueExtractor))
                {
                    try
                    {
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

            public static IDisposable CreateFromStateExtractor<TState>(TState state, ConcurrentDictionary<Type, KeyValuePair<Func<object, object>, Func<object, object>>> stateExractor)
            {
                ScopeProperties scope = null;
                var keyValueExtractor = default(KeyValuePair<Func<object, object>, Func<object, object>>);
                if (state is System.Collections.IEnumerable messageProperties)
                {
                    foreach (var property in messageProperties)
                    {
                        if (property == null)
                            return null;

                        if (scope == null)
                        {
                            if (!TryCreateExtractor<object>(stateExractor, property, out keyValueExtractor))
                                return null;

                            scope = new ScopeProperties();
                        }

                        AddKeyValueProperty(scope, keyValueExtractor, property);
                    }
                }
                else
                {
                    if (!TryCreateExtractor(stateExractor, state, out keyValueExtractor))
                        return null;

                    scope = new ScopeProperties();
                    AddKeyValueProperty(scope, keyValueExtractor, state);
                }

                if (scope != null)
                    scope.AddDispose(CreateDiagnosticLogicalContext(state));
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

            public void AddDispose(IDisposable disposable)
            {
                if (disposable != null)
                    Properties.Add(disposable);
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
                    _properties = null;
                    foreach (var property in properties)
                    {
                        try
                        {
                            property.Dispose();
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Debug(ex, "Exception in BeginScope dispose property {0}", property);
                        }
                    }
                }
            }
        }
    }
}
