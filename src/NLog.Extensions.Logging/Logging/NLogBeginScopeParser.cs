using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NLog.Common;

namespace NLog.Extensions.Logging
{
    using ExtractorDictionary = ConcurrentDictionary<Type, KeyValuePair<Func<object, object>, Func<object, object>>>;

    /// <summary>
    /// Converts Microsoft Extension Logging BeginScope into NLog NestedDiagnosticsLogicalContext + MappedDiagnosticsLogicalContext
    /// </summary>
    internal class NLogBeginScopeParser
    {
        private readonly NLogProviderOptions _options;

        private readonly ExtractorDictionary _scopeStateExtractors =
            new ExtractorDictionary();

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
                    if (state is IEnumerable scopePropertyCollection)
                    {
                        return ScopeProperties.CaptureScopeProperties(scopePropertyCollection, _scopeStateExtractors);
                    }

                    return ScopeProperties.CaptureScopeProperty(state, _scopeStateExtractors);
                }
            }

            return NestedDiagnosticsLogicalContext.Push(state);
        }

        private sealed class ScopeProperties : IDisposable
        {
            private readonly IDisposable _mldcScope;
            private readonly IDisposable _ndlcScope;

            private ScopeProperties(IDisposable ndlcScope, IDisposable mldcScope)
            {
                _ndlcScope = ndlcScope;
                _mldcScope = mldcScope;
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

            private static IDisposable CreateScopeProperties(object scopeObject, IReadOnlyList<KeyValuePair<string, object>> propertyList)
            {
                if (propertyList?.Count > 0)
                {
                    return new ScopeProperties(NestedDiagnosticsLogicalContext.Push(scopeObject), MappedDiagnosticsLogicalContext.SetScoped(propertyList));
                }

                return NestedDiagnosticsLogicalContext.Push(scopeObject);
            }

            public static IDisposable CaptureScopeProperties(IReadOnlyList<KeyValuePair<string, object>> scopePropertyList)
            {
                object scopeObject = scopePropertyList;

                if (scopePropertyList.Count > 0 && scopePropertyList[scopePropertyList.Count - 1].Key == NLogLogger.OriginalFormatPropertyName)
                {
                    var propertyList = new List<KeyValuePair<string, object>>(scopePropertyList.Count - 1);
                    for (var i = 0; i < scopePropertyList.Count; ++i)
                    {
                        var property = scopePropertyList[i];
                        if (i == scopePropertyList.Count - 1 && i > 0 && property.Key == NLogLogger.OriginalFormatPropertyName)
                        {
                            continue; // Handle BeginScope("Hello {World}", "Earth")
                        }

                        propertyList.Add(property);
                    }
                    scopePropertyList = propertyList;
                }

                return CreateScopeProperties(scopeObject, scopePropertyList);
            }

            public static IDisposable CaptureScopeProperties(IEnumerable scopePropertyCollection, ExtractorDictionary stateExractor)
            {
                List<KeyValuePair<string, object>> propertyList = null;

                var keyValueExtractor = default(KeyValuePair<Func<object, object>, Func<object, object>>);
                foreach (var property in scopePropertyCollection)
                {
                    if (property == null)
                    {
                        break;
                    }

                    if (keyValueExtractor.Key == null && !TryLookupExtractor(stateExractor, property.GetType(), out keyValueExtractor))
                    {
                        break;
                    }

                    var propertyValue = TryParseKeyValueProperty(keyValueExtractor, property);
                    if (!propertyValue.HasValue)
                    {
                        continue;
                    }

                    propertyList = propertyList ?? new List<KeyValuePair<string, object>>();
                    propertyList.Add(propertyValue.Value);
                }

                return CreateScopeProperties(scopePropertyCollection, propertyList);
            }

            public static IDisposable CaptureScopeProperty<TState>(TState scopeProperty, ExtractorDictionary stateExractor)
            {
                if (!TryLookupExtractor(stateExractor, scopeProperty.GetType(), out var keyValueExtractor))
                {
                    return NestedDiagnosticsLogicalContext.Push(scopeProperty);
                }

                var propertyValue = TryParseKeyValueProperty(keyValueExtractor, scopeProperty);
                if (!propertyValue.HasValue)
                {
                    return NestedDiagnosticsLogicalContext.Push(scopeProperty);
                }

                return new ScopeProperties(NestedDiagnosticsLogicalContext.Push(scopeProperty), MappedDiagnosticsLogicalContext.SetScoped(propertyValue.Value.Key, propertyValue.Value.Value));
            }

            private static KeyValuePair<string, object>? TryParseKeyValueProperty(KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor, object property)
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

            private static bool TryLookupExtractor(ExtractorDictionary stateExractor, Type propertyType,
                out KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor)
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
                {
                    return false;
                }

                var keyPropertyInfo = itemType.GetDeclaredProperty("Key");
                var valuePropertyInfo = itemType.GetDeclaredProperty("Value");
                if (valuePropertyInfo == null || keyPropertyInfo == null)
                {
                    return false;
                }

                var keyValuePairObjParam = Expression.Parameter(typeof(object), "pair");
                var keyValuePairTypeParam = Expression.Convert(keyValuePairObjParam, propertyType);

                var propertyKeyAccess = Expression.Property(keyValuePairTypeParam, keyPropertyInfo);
                var propertyKeyAccessObj = Expression.Convert(propertyKeyAccess, typeof(object));
                var propertyKeyLambda = Expression.Lambda<Func<object, object>>(propertyKeyAccessObj, keyValuePairObjParam).Compile();

                var propertyValueAccess = Expression.Property(keyValuePairTypeParam, valuePropertyInfo);
                var propertyValueLambda = Expression.Lambda<Func<object, object>>(propertyValueAccess, keyValuePairObjParam).Compile();

                keyValueExtractor = new KeyValuePair<Func<object, object>, Func<object, object>>(propertyKeyLambda, propertyValueLambda);
                return true;
            }

            public override string ToString()
            {
                return _ndlcScope?.ToString() ?? base.ToString();
            }
        }
    }
}