using System;
using System.Collections;
using System.Collections.Generic;
using NLog.MessageTemplates;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Converts Microsoft Extension Logging ParameterList into NLog MessageTemplate ParameterList
    /// </summary>
    internal class NLogMessageParameterList : IList<MessageTemplateParameter>
    {
        private static readonly NLogMessageParameterList EmptyList = new NLogMessageParameterList(Array.Empty<KeyValuePair<string, object>>(), default, default, default, default);
        private static readonly NLogMessageParameterList EmptyPositionalList = new NLogMessageParameterList(Array.Empty<KeyValuePair<string, object>>(), default, default, default, isPositional: true);
        private static readonly NLogMessageParameterList OriginalMessageList = new NLogMessageParameterList(new[] { new KeyValuePair<string, object>(NLogLogger.OriginalFormatPropertyName, string.Empty) }, 0, default, default, default);

        private readonly IReadOnlyList<KeyValuePair<string, object>> _parameterList;
        private readonly int? _originalMessageIndex;
        private readonly bool _hasMessageTemplateCapture;
        private readonly bool _isMixedPositional;
        private readonly bool _isPositional;

        public bool HasComplexParameters => _hasMessageTemplateCapture || _isMixedPositional;
        public bool IsPositional => _isPositional;
        public int Count => _parameterList.Count - (_originalMessageIndex.HasValue ? 1 : 0);
        public bool IsReadOnly => true;

        private NLogMessageParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList, int? originalMessageIndex, bool hasMessageTemplateCapture, bool isMixedPositional, bool isPositional)
        {
            _parameterList = parameterList;
            _originalMessageIndex = originalMessageIndex;
            _hasMessageTemplateCapture = hasMessageTemplateCapture;
            _isMixedPositional = isMixedPositional;
            _isPositional = isPositional;
        }

        /// <summary>
        /// Create a <see cref="NLogMessageParameterList"/> if <paramref name="parameterList"/> has values, otherwise <c>null</c>
        /// </summary>
        /// <remarks>
        /// The LogMessageParameterList-constructor initiates all the parsing/scanning
        /// </remarks>
        public static NLogMessageParameterList TryParse(IReadOnlyList<KeyValuePair<string, object>> parameterList)
        {
            var parameterCount = parameterList.Count;
            if (parameterCount > 1 || (parameterCount == 1 && !NLogLogger.OriginalFormatPropertyName.Equals(parameterList[0].Key)))
            {
                if (IsValidParameterList(parameterList, out var originalMessageIndex, out var hasMessageTemplateCapture, out var isMixedPositional, out var isPositional))
                {
                    if (isPositional)
                    {
                        return EmptyPositionalList;   // Skip allocation, not needed to create Parameters-array
                    }
                    else
                    {
                        return new NLogMessageParameterList(parameterList, originalMessageIndex, hasMessageTemplateCapture, isMixedPositional, isPositional);
                    }
                }
                else
                {
                    return new NLogMessageParameterList(CreateValidParameterList(parameterList), originalMessageIndex, hasMessageTemplateCapture, isMixedPositional || isPositional, isPositional);
                }      
            }
            else if (parameterCount == 1)
            {
                return OriginalMessageList; // Skip allocation
            }
            else
            {
                return EmptyList;           // Skip allocation
            }
        }

        public bool HasMessageTemplateSyntax(bool parseMessageTemplates)
        {
            return _originalMessageIndex.HasValue && (HasComplexParameters || (parseMessageTemplates && Count > 0));
        }

        public string GetOriginalMessage(IReadOnlyList<KeyValuePair<string, object>> messageProperties)
        {
            if (_originalMessageIndex < messageProperties?.Count)
            {
                return messageProperties[_originalMessageIndex.Value].Value as string;
            }
            return null;
        }

        /// <summary>
        /// Verify that the input parameterList contains non-empty key-values and the original-format-property at the end
        /// </summary>
        private static bool IsValidParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList, out int? originalMessageIndex, out bool hasMessageTemplateCapture, out bool isMixedPositional, out bool isPositional)
        {
            hasMessageTemplateCapture = false;
            isMixedPositional = false;
            originalMessageIndex = null;
            isPositional = false;
            string parameterName;

            var parameterCount = parameterList.Count;
            for (int i = 0; i < parameterCount; ++i)
            {
                if (!TryGetParameterName(parameterList, i, out parameterName))
                {
                    originalMessageIndex = null;
                    return false;
                }

                char firstChar = parameterName[0];
                if (firstChar >= '0' && firstChar <= '9')
                {
                    if (!isPositional)
                        isMixedPositional = i != 0;
                    isMixedPositional |= i != (firstChar - '0');
                    isPositional = true;
                }
                else
                {
                    if (NLogLogger.OriginalFormatPropertyName.Equals(parameterName))
                    {
                        if (originalMessageIndex.HasValue)
                        {
                            originalMessageIndex = null;
                            return false;
                        }

                        originalMessageIndex = i;
                    }
                    else
                    {
                        isMixedPositional = isPositional;
                        hasMessageTemplateCapture |= GetCaptureType(firstChar) != CaptureType.Normal;
                    }
                }
            }

            isPositional = isPositional && !isMixedPositional;
            return true;
        }

        private static bool TryGetParameterName(IReadOnlyList<KeyValuePair<string, object>> parameterList, int i, out string parameterKey)
        {
            try
            {
                parameterKey = parameterList[i].Key;
            }
            catch (IndexOutOfRangeException ex)
            {
                // Catch a issue in MEL
                throw new FormatException($"Invalid format string. Expected {parameterList.Count - 1} format parameters, but failed to lookup parameter index {i}", ex);
            }

            if (string.IsNullOrEmpty(parameterKey))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Extract all valid properties from the input parameterList, and return them in a newly allocated list
        /// </summary>
        private static IReadOnlyList<KeyValuePair<string, object>> CreateValidParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList)
        {
            var parameterCount = parameterList.Count;
            var validParameterList = new List<KeyValuePair<string, object>>(parameterCount);

            for (int i = 0; i < parameterCount; ++i)
            {
                if (!TryGetParameterName(parameterList, i, out var parameterName))
                    continue;

                if (NLogLogger.OriginalFormatPropertyName.Equals(parameterName))
                    continue;

                validParameterList.Add(parameterList[i]);
            }

            return validParameterList;
        }

        public MessageTemplateParameter this[int index]
        {
            get
            {
                if (index >= _originalMessageIndex)
                    index += 1;

                var parameter = _parameterList[index];
                return _hasMessageTemplateCapture ?
                    GetMessageTemplateParameter(parameter.Key, parameter.Value) :
                    new MessageTemplateParameter(parameter.Key, parameter.Value, null, CaptureType.Normal);
            }
            set => throw new NotSupportedException();
        }

        private static MessageTemplateParameter GetMessageTemplateParameter(string parameterName, object parameterValue)
        {
            var capture = GetCaptureType(parameterName[0]);
            if (capture != CaptureType.Normal)
                parameterName = parameterName.Substring(1);
            return new MessageTemplateParameter(parameterName, parameterValue, null, capture);
        }

        private static CaptureType GetCaptureType(char firstChar)
        {
            if (firstChar == '@')
                return CaptureType.Serialize;
            else if (firstChar == '$')
                return CaptureType.Stringify;
            else
                return CaptureType.Normal;
        }

        public void Add(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(MessageTemplateParameter[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; ++i)
                array[i + arrayIndex] = this[i];
        }

        public IEnumerator<MessageTemplateParameter> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
                yield return this[i];
        }

        public int IndexOf(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
