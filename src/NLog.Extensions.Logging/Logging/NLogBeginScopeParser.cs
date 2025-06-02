using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NLog.Common;

namespace NLog.Extensions.Logging
{
    using ExtractorDictionary = ConcurrentDictionary<Type, Func<object, KeyValuePair<string, object?>>>;

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
                if (state is IReadOnlyList<KeyValuePair<string, object?>> scopePropertyList)
                {
                    if (scopePropertyList is IList)
                        return ScopeContext.PushNestedStateProperties(null, scopePropertyList);  // Probably List/Array without nested state

                    object scopeObject = scopePropertyList;
                    scopePropertyList = ParseScopeProperties(scopePropertyList);
                    return ScopeContext.PushNestedStateProperties(scopeObject, scopePropertyList);
                }
                else if (state is IReadOnlyCollection<KeyValuePair<string, object?>> scopeProperties)
                {
                    if (scopeProperties is IDictionary)
                        return ScopeContext.PushNestedStateProperties(null, scopeProperties);    // Probably Dictionary without nested state
                    else
                        return ScopeContext.PushNestedStateProperties(scopeProperties, scopeProperties);
                }
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER || NET471_OR_GREATER
                else if (state is System.Runtime.CompilerServices.ITuple tuple && tuple.Length == 2 && tuple[0] is string)
                {
                    return ScopeContext.PushProperty(tuple[0]?.ToString() ?? string.Empty, tuple[1]);
                }
#endif

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

        private IReadOnlyList<KeyValuePair<string, object?>> ParseScopeProperties(IReadOnlyList<KeyValuePair<string, object?>> scopePropertyList)
        {
            var scopePropertyCount = scopePropertyList.Count;
            if (scopePropertyCount == 0)
                return scopePropertyList;

            if (!NLogLogger.OriginalFormatPropertyName.Equals(scopePropertyList[scopePropertyCount - 1].Key))
                return IncludeActivityIdsProperties(scopePropertyList);
            else if (scopePropertyCount == 1)
                return Array.Empty<KeyValuePair<string, object?>>();
            else
                scopePropertyCount -= 1;    // Handle BeginScope("Hello {World}", "Earth")

            var firstProperty = scopePropertyList[0];
            if (scopePropertyCount == 1 && !string.IsNullOrEmpty(firstProperty.Key))
            {
                return new[] { firstProperty };
            }
            else
            {
                var propertyList = new List<KeyValuePair<string, object?>>(scopePropertyCount);
                for (var i = 0; i < scopePropertyCount; ++i)
                {
                    var property = scopePropertyList[i];
                    if (string.IsNullOrEmpty(property.Key))
                    {
                        continue;
                    }
                    propertyList.Add(property);
                }
                return propertyList;
            }
        }

#if NET5_0_OR_GREATER
        private IReadOnlyList<KeyValuePair<string, object?>> IncludeActivityIdsProperties(IReadOnlyList<KeyValuePair<string, object?>> scopePropertyList)
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

        private sealed class ScopePropertiesWithActivityIds : IReadOnlyList<KeyValuePair<string, object?>>
        {
            private readonly IReadOnlyList<KeyValuePair<string, object?>> _originalPropertyList;
            private readonly System.Diagnostics.Activity _currentActivity;

            public ScopePropertiesWithActivityIds(IReadOnlyList<KeyValuePair<string, object?>> originalPropertyList, System.Diagnostics.Activity currentActivity)
            {
                _originalPropertyList = originalPropertyList;
                _currentActivity = currentActivity;
            }

            public KeyValuePair<string, object?> this[int index]
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
                            case 0: return new KeyValuePair<string, object?>(nameof(_currentActivity.SpanId), _currentActivity.GetSpanId());
                            case 1: return new KeyValuePair<string, object?>(nameof(_currentActivity.TraceId), _currentActivity.GetTraceId());
                            case 2: return new KeyValuePair<string, object?>(nameof(_currentActivity.ParentId), _currentActivity.GetParentId());
                        }
                    }

                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }

            public int Count => _originalPropertyList.Count + 3;

            public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                    yield return this[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_originalPropertyList).GetEnumerator();
            }
        }
#else
        private static IReadOnlyList<KeyValuePair<string, object?>> IncludeActivityIdsProperties(IReadOnlyList<KeyValuePair<string, object?>> scopePropertyList)
        {
            return scopePropertyList;   // Not supported
        }
#endif

        public static IDisposable CaptureScopeProperties(IEnumerable scopePropertyCollection, ExtractorDictionary stateExtractor)
        {
            List<KeyValuePair<string, object?>>? propertyList = null;

            Func<object, KeyValuePair<string, object?>>? keyValueExtractor = null;
            foreach (var property in scopePropertyCollection)
            {
                if (property is null)
                {
                    break;
                }

                if (keyValueExtractor is null && (!TryLookupExtractor(stateExtractor, property, out keyValueExtractor) || keyValueExtractor is null))
                    break;

                var propertyValue = TryParseKeyValueProperty(keyValueExtractor, property);
                if (!propertyValue.HasValue || string.IsNullOrEmpty(propertyValue.Value.Key))
                {
                    continue;
                }

                propertyList = propertyList ?? new List<KeyValuePair<string, object?>>((scopePropertyCollection as ICollection)?.Count ?? 0);
                propertyList.Add(propertyValue.Value);
            }

            if (scopePropertyCollection is IList || scopePropertyCollection is IDictionary)
                return ScopeContext.PushNestedStateProperties(null, propertyList);   // Probably List/Array/Dictionary without nested state
            else
                return ScopeContext.PushNestedStateProperties(scopePropertyCollection, propertyList);
        }

        public static IDisposable CaptureScopeProperty<TState>(TState scopeProperty, ExtractorDictionary stateExtractor)
        {
            if (!TryLookupExtractor(stateExtractor, scopeProperty, out var keyValueExtractor) || keyValueExtractor is null)
            {
                return ScopeContext.PushNestedState(scopeProperty);
            }

            object? scopePropertyValue = scopeProperty;
            var propertyValue = TryParseKeyValueProperty(keyValueExtractor, scopePropertyValue);
            if (propertyValue.HasValue)
            {
                return ScopeContext.PushNestedStateProperties(scopePropertyValue, new[] { new KeyValuePair<string, object?>(propertyValue.Value.Key, propertyValue.Value.Value) });
            }

            return ScopeContext.PushNestedState(scopeProperty);
        }

        private static KeyValuePair<string, object?>? TryParseKeyValueProperty(Func<object, KeyValuePair<string, object?>> keyValueExtractor, object? property)
        {
            if (property is null)
                return default;

            try
            {
                return keyValueExtractor.Invoke(property);
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, "Exception in BeginScope add property {0}", property);
                return null;
            }
        }

        private static bool TryLookupExtractor<TState>(ExtractorDictionary stateExtractor, TState propertyValue,
            out Func<object, KeyValuePair<string, object?>>? keyValueExtractor)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER || NET471_OR_GREATER
            if (propertyValue is System.Runtime.CompilerServices.ITuple tuple && tuple.Length == 2 && tuple[0] is string)
            {
                keyValueExtractor = (obj) => new KeyValuePair<string, object?>(
                    ((System.Runtime.CompilerServices.ITuple)obj)[0]?.ToString() ?? string.Empty,
                    ((System.Runtime.CompilerServices.ITuple)obj)[1]);
                return true;
            }
#endif

            var propertyType = propertyValue?.GetType() ?? typeof(TState);
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
                    if (keyValueExtractor != null)
                        stateExtractor[propertyType] = keyValueExtractor;
                }
            }

            return keyValueExtractor != null;
        }

        private static bool TryBuildExtractor(Type propertyType, out Func<object , KeyValuePair<string, object?>>? keyValueExtractor)
        {
            keyValueExtractor = null;

            var itemType = propertyType.GetTypeInfo();
            if (!itemType.IsGenericType)
                return false;

            if (itemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
#if NETSTANDARD || NETFRAMEWORK
                var keyPropertyInfo = typeof(KeyValuePair<,>).MakeGenericType(itemType.GenericTypeArguments).GetTypeInfo().GetDeclaredProperty("Key");
                var valuePropertyInfo = typeof(KeyValuePair<,>).MakeGenericType(itemType.GenericTypeArguments).GetTypeInfo().GetDeclaredProperty("Value");
                if (valuePropertyInfo is null || keyPropertyInfo is null)
                {
                    return false;
                }

                var keyValuePairObjParam = Expression.Parameter(typeof(object), "KeyValuePair");
                var keyValuePairTypeParam = Expression.Convert(keyValuePairObjParam, propertyType);
                var propertyKeyAccess = Expression.Property(keyValuePairTypeParam, keyPropertyInfo);
                var propertyValueAccess = Expression.Property(keyValuePairTypeParam, valuePropertyInfo);
                return BuildKeyValueExtractor(keyValuePairObjParam, propertyKeyAccess, propertyValueAccess, out keyValueExtractor);
#else
                if (itemType.GenericTypeArguments[0] == typeof(string))
                {
                    if (itemType.GenericTypeArguments[1] == typeof(object))
                        return BuildKeyValueExtractor<string, object>(propertyType, out keyValueExtractor);
                    if (itemType.GenericTypeArguments[1] == typeof(string))
                        return BuildKeyValueExtractor<string, string>(propertyType, out keyValueExtractor);
                    if (itemType.GenericTypeArguments[1] == typeof(int))
                        return BuildKeyValueExtractor<string, int>(propertyType, out keyValueExtractor);
                    if (itemType.GenericTypeArguments[1] == typeof(long))
                        return BuildKeyValueExtractor<string, long>(propertyType, out keyValueExtractor);
                    if (itemType.GenericTypeArguments[1] == typeof(decimal))
                        return BuildKeyValueExtractor<string, decimal>(propertyType, out keyValueExtractor);
                    if (itemType.GenericTypeArguments[1] == typeof(double))
                        return BuildKeyValueExtractor<string, double>(propertyType, out keyValueExtractor);
                    if (itemType.GenericTypeArguments[1] == typeof(bool))
                        return BuildKeyValueExtractor<string, bool>(propertyType, out keyValueExtractor);
                    if (itemType.GenericTypeArguments[1] == typeof(Guid))
                        return BuildKeyValueExtractor<string, Guid>(propertyType, out keyValueExtractor);
                }
#endif
            }

#if NETSTANDARD || NETFRAMEWORK
            if (itemType.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
            {
                var keyPropertyInfo = typeof(ValueTuple<,>).MakeGenericType(itemType.GenericTypeArguments).GetTypeInfo().GetDeclaredField("Item1");
                var valuePropertyInfo = typeof(ValueTuple<,>).MakeGenericType(itemType.GenericTypeArguments).GetTypeInfo().GetDeclaredField("Item2");
                if (valuePropertyInfo is null || keyPropertyInfo is null)
                {
                    return false;
                }

                var keyValuePairObjParam = Expression.Parameter(typeof(object), "ValueTuple");
                var keyValuePairTypeParam = Expression.Convert(keyValuePairObjParam, propertyType);
                var propertyKeyAccess = Expression.Field(keyValuePairTypeParam, keyPropertyInfo);
                var propertyValueAccess = Expression.Field(keyValuePairTypeParam, valuePropertyInfo);
                return BuildKeyValueExtractor(keyValuePairObjParam, propertyKeyAccess, propertyValueAccess, out keyValueExtractor);
            }
#endif
            return false;
        }

#if !NETSTANDARD && !NETFRAMEWORK
        private static bool BuildKeyValueExtractor<TKey, TValue>(Type propertyType, out Func<object, KeyValuePair<string, object?>>? keyValueExtractor)
        {
            var keyPropertyInfo = typeof(KeyValuePair<TKey, TValue>).GetTypeInfo().GetDeclaredProperty("Key");
            var valuePropertyInfo = typeof(KeyValuePair<TKey, TValue>).GetTypeInfo().GetTypeInfo().GetDeclaredProperty("Value");
            if (valuePropertyInfo is null || keyPropertyInfo is null)
            {
                keyValueExtractor = default;
                return false;
            }

            var keyValuePairObjParam = Expression.Parameter(typeof(object), "KeyValuePair");
            var keyValuePairTypeParam = Expression.Convert(keyValuePairObjParam, propertyType);
            var propertyKeyAccess = Expression.Property(keyValuePairTypeParam, keyPropertyInfo);
            var propertyValueAccess = Expression.Property(keyValuePairTypeParam, valuePropertyInfo);
            return BuildKeyValueExtractor(keyValuePairObjParam, propertyKeyAccess, propertyValueAccess, out keyValueExtractor);
        }
#endif
        private static bool BuildKeyValueExtractor(ParameterExpression keyValuePairObjParam, MemberExpression propertyKeyAccess, MemberExpression propertyValueAccess, out Func<object, KeyValuePair<string, object?>> keyValueExtractor)
        {
            var propertyKeyAccessObj = Expression.Convert(propertyKeyAccess, typeof(object));
            var propertyKeyLambda = Expression.Lambda<Func<object, object>>(propertyKeyAccessObj, keyValuePairObjParam).Compile();

            var propertyValueAccessObj = Expression.Convert(propertyValueAccess, typeof(object));
            var propertyValueLambda = Expression.Lambda<Func<object, object?>>(propertyValueAccessObj, keyValuePairObjParam).Compile();

            keyValueExtractor = (obj) =>
            {
                return new KeyValuePair<string, object?>(
                    propertyKeyLambda.Invoke(obj)?.ToString() ?? string.Empty,
                    propertyValueLambda.Invoke(obj));
            };
            return true;
        }
    }
}