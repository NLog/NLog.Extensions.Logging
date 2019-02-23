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
                return NestedDiagnosticsLogicalContext.Push(state); // AsyncLocal has no requirement to be Serializable
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, "Exception in BeginScope push NestedDiagnosticsLogicalContext");
                return null;
            }
        }

        private class ScopeProperties : IDisposable
        {
            private readonly IDisposable _ndlcScope;
            private readonly IDisposable _mldcScope;

            public ScopeProperties(IDisposable ndlcScope, IDisposable mldcScope)
            {
                _ndlcScope = ndlcScope;
                _mldcScope = mldcScope;
            }

            private static IDisposable CreateScopeProperties(object scopeObject, IReadOnlyList<KeyValuePair<string, object>> propertyList)
            {
                if (propertyList?.Count > 0)
                    return new ScopeProperties(CreateDiagnosticLogicalContext(scopeObject), MappedDiagnosticsLogicalContext.SetScoped(propertyList));
                else
                    return CreateDiagnosticLogicalContext(scopeObject);
            }

            public static IDisposable CaptureScopeProperties(IReadOnlyList<KeyValuePair<string, object>> scopePropertyList)
            {
                if (scopePropertyList.Count == 0 || scopePropertyList[scopePropertyList.Count - 1].Key != NLogLogger.OriginalFormatPropertyName)
                {
                    return CreateScopeProperties(scopePropertyList, scopePropertyList);
                }
                else
                {
                    List<KeyValuePair<string, object>> propertyList = new List<KeyValuePair<string, object>>(scopePropertyList.Count - 1);
                    for (int i = 0; i < scopePropertyList.Count; ++i)
                    {
                        var property = scopePropertyList[i];
                        if (i == scopePropertyList.Count - 1 && i > 0 && property.Key == NLogLogger.OriginalFormatPropertyName)
                            continue;   // Handle BeginScope("Hello {World}", "Earth")

                        propertyList.Add(property);
                    }
                    return CreateScopeProperties(scopePropertyList, propertyList);
                }
            }

            public static IDisposable CaptureScopeProperties(System.Collections.IEnumerable scopePropertyCollection, ConcurrentDictionary<Type, KeyValuePair<Func<object, object>, Func<object, object>>> stateExractor)
            {
                List<KeyValuePair<string, object>> propertyList = null;

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

                    var propertyValue = TryParseKeyValueProperty(keyValueExtractor, property);
                    if (!propertyValue.HasValue)
                        continue;

                    propertyList = propertyList ?? new List<KeyValuePair<string, object>>();
                    propertyList.Add(propertyValue.Value);
                }

                return CreateScopeProperties(scopePropertyCollection, propertyList);
            }

            public static IDisposable CaptureScopeProperty<TState>(TState scopeProperty, ConcurrentDictionary<Type, KeyValuePair<Func<object, object>, Func<object, object>>> stateExractor)
            {
                if (!TryLookupExtractor(stateExractor, scopeProperty.GetType(), out var keyValueExtractor))
                    return CreateDiagnosticLogicalContext(scopeProperty);

                var propertyValue = TryParseKeyValueProperty(keyValueExtractor, scopeProperty);
                if (!propertyValue.HasValue)
                    return CreateDiagnosticLogicalContext(scopeProperty);

                return new ScopeProperties(CreateDiagnosticLogicalContext(scopeProperty), MappedDiagnosticsLogicalContext.SetScoped(propertyValue.Value.Key, propertyValue.Value.Value));
            }

            private static KeyValuePair<string,object>? TryParseKeyValueProperty(KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor, object property)
            {
                string propertyName = null;

                try
                {
                    var propertyKey = keyValueExtractor.Key.Invoke(property);
                    propertyName = propertyKey?.ToString() ?? string.Empty;
                    var propertyValue = keyValueExtractor.Value.Invoke(property);
                    return new KeyValuePair<string, object>(propertyName, propertyValue);
                }
                catch (Exception ex)
                {
                    InternalLogger.Debug(ex, "Exception in BeginScope add property {0}", propertyName);
                    return null;
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

            public void Dispose()
            {
                try
                {
                    _mldcScope?.Dispose();
                }
                catch (Exception ex)
                {
                    InternalLogger.Debug(ex, "Exception in BeginScope dispose MappedDiagnosticsLogicalContext");
                }
                try
                {
                    _ndlcScope?.Dispose();
                }
                catch (Exception ex)
                {
                    InternalLogger.Debug(ex, "Exception in BeginScope dispose NestedDiagnosticsLogicalContext");
                }
            }

            public override string ToString()
            {
                return _ndlcScope?.ToString() ?? base.ToString();
            }
        }
    }
}
