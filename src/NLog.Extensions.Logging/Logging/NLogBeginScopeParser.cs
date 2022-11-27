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
    /// Converts Microsoft Extension Logging BeginScope into NLog ScopeContext
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
                    object scopeObject = scopePropertyList;
                    scopePropertyList = ParseScopeProperties(scopePropertyList);
                    return ScopeContext.PushNestedStateProperties(scopeObject, scopePropertyList);
                }

                if (!(state is string))
                {
                    if (state is IEnumerable scopePropertyCollection)
                    {
                        return CaptureScopeProperties(scopePropertyCollection, _scopeStateExtractors);
                    }

                    return CaptureScopeProperty(state, _scopeStateExtractors);
                }
            }

            return ScopeContext.PushNestedState(state);
        }

        private IReadOnlyList<KeyValuePair<string, object>> ParseScopeProperties(IReadOnlyList<KeyValuePair<string, object>> scopePropertyList)
        {
            var scopePropertyCount = scopePropertyList.Count;
            if (scopePropertyCount == 0)
                return Array.Empty<KeyValuePair<string, object>>();

            if (!NLogLogger.OriginalFormatPropertyName.Equals(scopePropertyList[scopePropertyCount - 1].Key))
                return IncludeActivityIdsProperties(scopePropertyList);

            var firstProperty = scopePropertyList[0];
            if (scopePropertyCount == 2 && !string.IsNullOrEmpty(firstProperty.Key) && !NLogLogger.OriginalFormatPropertyName.Equals(firstProperty.Key))
            {
                return new[] { firstProperty };
            }
            else if (scopePropertyCount <= 2)
            {
                return Array.Empty<KeyValuePair<string, object>>();
            }
            else
            {
                var propertyList = new List<KeyValuePair<string, object>>(scopePropertyCount - 1);
                for (var i = 0; i < scopePropertyCount; ++i)
                {
                    var property = scopePropertyList[i];
                    if (string.IsNullOrEmpty(property.Key))
                    {
                        continue;
                    }
                    if (NLogLogger.OriginalFormatPropertyName.Equals(property.Key))
                    {
                        continue; // Handle BeginScope("Hello {World}", "Earth")
                    }
                    propertyList.Add(property);
                }
                return propertyList;
            }
        }

#if !NET6_0
        private static IReadOnlyList<KeyValuePair<string, object>> IncludeActivityIdsProperties(IReadOnlyList<KeyValuePair<string, object>> scopePropertyList)
        {
            return scopePropertyList;   // Not supported
        }
#else
        private IReadOnlyList<KeyValuePair<string, object>> IncludeActivityIdsProperties(IReadOnlyList<KeyValuePair<string, object>> scopePropertyList)
        {
            if (_options.IncludeActivityIdsWithBeginScope && "RequestId".Equals(scopePropertyList[0].Key))
            {
                if (scopePropertyList.Count > 1 && "RequestPath".Equals(scopePropertyList[1].Key))
                {
                    var activty = System.Diagnostics.Activity.Current;
                    if (activty != null)
                        return new ScopePropertiesWithActivityIds(scopePropertyList, activty);
                }
            }

            return scopePropertyList;
        }

        private class ScopePropertiesWithActivityIds : IReadOnlyList<KeyValuePair<string, object>>
        {
            private readonly IReadOnlyList<KeyValuePair<string, object>> _originalPropertyList;
            private readonly System.Diagnostics.Activity _currentActivity;

            public ScopePropertiesWithActivityIds(IReadOnlyList<KeyValuePair<string, object>> originalPropertyList, System.Diagnostics.Activity currentActivity)
            {
                _originalPropertyList = originalPropertyList;
                _currentActivity = currentActivity;
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    int offset = index - _originalPropertyList.Count;
                    if (offset < 0)
                    {
                        return _originalPropertyList[index];
                    }
                    else
                    {
                        switch (offset)
                        {
                            case 0: return new KeyValuePair<string, object>(nameof(_currentActivity.SpanId), _currentActivity.GetSpanId());
                            case 1: return new KeyValuePair<string, object>(nameof(_currentActivity.TraceId), _currentActivity.GetTraceId());
                            case 2: return new KeyValuePair<string, object>(nameof(_currentActivity.ParentId), _currentActivity.GetParentId());
                        }
                    }

                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }

            public int Count => _originalPropertyList.Count + 3;

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                    yield return this[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_originalPropertyList).GetEnumerator();
            }
        }
#endif

        public static IDisposable CaptureScopeProperties(IEnumerable scopePropertyCollection, ExtractorDictionary stateExtractor)
        {
            List<KeyValuePair<string, object>> propertyList = null;

            var keyValueExtractor = default(KeyValuePair<Func<object, object>, Func<object, object>>);
            foreach (var property in scopePropertyCollection)
            {
                if (property is null)
                {
                    break;
                }

                if (keyValueExtractor.Key is null && !TryLookupExtractor(stateExtractor, property.GetType(), out keyValueExtractor))
                {
                    break;
                }

                var propertyValue = TryParseKeyValueProperty(keyValueExtractor, property);
                if (!propertyValue.HasValue)
                {
                    continue;
                }

                propertyList = propertyList ?? new List<KeyValuePair<string, object>>((scopePropertyCollection as ICollection)?.Count ?? 0);
                propertyList.Add(propertyValue.Value);
            }

            return ScopeContext.PushNestedStateProperties(scopePropertyCollection, propertyList);
        }

        public static IDisposable CaptureScopeProperty<TState>(TState scopeProperty, ExtractorDictionary stateExtractor)
        {
            if (!TryLookupExtractor(stateExtractor, scopeProperty.GetType(), out var keyValueExtractor))
            {
                return ScopeContext.PushNestedState(scopeProperty);
            }

            object scopePropertyValue = scopeProperty;
            var propertyValue = TryParseKeyValueProperty(keyValueExtractor, scopePropertyValue);
            if (propertyValue.HasValue)
            {
                return ScopeContext.PushNestedStateProperties(scopePropertyValue, new[] { new KeyValuePair<string, object>(propertyValue.Value.Key, propertyValue.Value.Value) });
            }

            return ScopeContext.PushNestedState(scopeProperty);
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

        private static bool TryLookupExtractor(ExtractorDictionary stateExtractor, Type propertyType,
            out KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor)
        {
            if (!stateExtractor.TryGetValue(propertyType, out keyValueExtractor))
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
                    stateExtractor[propertyType] = keyValueExtractor;
                }
            }

            return keyValueExtractor.Key != null;
        }

        private static bool TryBuildExtractor(Type propertyType, out KeyValuePair<Func<object, object>, Func<object, object>> keyValueExtractor)
        {
            keyValueExtractor = default;

            var itemType = propertyType.GetTypeInfo();
            if (!itemType.IsGenericType || itemType.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
            {
                return false;
            }

            var keyPropertyInfo = itemType.GetDeclaredProperty("Key");
            var valuePropertyInfo = itemType.GetDeclaredProperty("Value");
            if (valuePropertyInfo is null || keyPropertyInfo is null)
            {
                return false;
            }

            var keyValuePairObjParam = Expression.Parameter(typeof(object), "pair");
            var keyValuePairTypeParam = Expression.Convert(keyValuePairObjParam, propertyType);

            var propertyKeyAccess = Expression.Property(keyValuePairTypeParam, keyPropertyInfo);
            var propertyKeyAccessObj = Expression.Convert(propertyKeyAccess, typeof(object));
            var propertyKeyLambda = Expression.Lambda<Func<object, object>>(propertyKeyAccessObj, keyValuePairObjParam).Compile();

            var propertyValueAccess = Expression.Property(keyValuePairTypeParam, valuePropertyInfo);
            var propertyValueAccessObj = Expression.Convert(propertyValueAccess, typeof(object));
            var propertyValueLambda = Expression.Lambda<Func<object, object>>(propertyValueAccessObj, keyValuePairObjParam).Compile();

            keyValueExtractor = new KeyValuePair<Func<object, object>, Func<object, object>>(propertyKeyLambda, propertyValueLambda);
            return true;
        }
    }
}