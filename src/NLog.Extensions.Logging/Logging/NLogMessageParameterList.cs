using System;
using System.Collections;
using System.Collections.Generic;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Converts Microsoft Extension Logging ParameterList into NLog MessageTemplate ParameterList
    /// </summary>
    internal class NLogMessageParameterList : IList<NLog.MessageTemplates.MessageTemplateParameter>
    {
        private readonly IReadOnlyList<KeyValuePair<string, object>> _parameterList;

        public object OriginalMessage => _originalMessageIndex.HasValue ? _parameterList[_originalMessageIndex.Value].Value : null;
        public int? _originalMessageIndex;

        public bool CustomCaptureTypes => _customCaptureTypes;
        public bool _customCaptureTypes;

        public bool IsPositional => _isPositional;
        public bool _isPositional;

        public NLogMessageParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList)
        {
            if (IsValidParameterList(parameterList, out _originalMessageIndex, out _customCaptureTypes, out _isPositional))
            {
                _parameterList = parameterList;
            }
            else
            {
                _parameterList = CreateValidParameterList(parameterList, out _customCaptureTypes, out _isPositional);
            }
        }

        /// <summary>
        /// Create a <see cref="NLogMessageParameterList"/> if <paramref name="parameterList"/> has values, otherwise <c>null</c>
        /// </summary>
        /// <remarks>
        /// The LogMessageParameterList-constructor initiates all the parsing/scanning
        /// </remarks>
        public static NLogMessageParameterList TryParse(IReadOnlyList<KeyValuePair<string, object>> parameterList)
        {
            return parameterList?.Count > 0 ? new NLogMessageParameterList(parameterList) : null;
        }

        /// <summary>
        /// Verify that the input parameterList contains non-empty key-values and the orignal-format-property at the end
        /// </summary>
        private static bool IsValidParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList, out int? originalMessageIndex, out bool customCaptureTypes, out bool isPositional)
        {
            isPositional = true;
            customCaptureTypes = false;
            originalMessageIndex = null;
            for (int i = 0; i < parameterList.Count; ++i)
            {
                var paramPair = parameterList[i];
                if (string.IsNullOrEmpty(paramPair.Key))
                {
                    originalMessageIndex = null;
                    return false;
                }

                char firstChar = paramPair.Key[0];
                if (!char.IsDigit(firstChar))
                {
                    if (firstChar == '@' || firstChar == '$')
                    {
                        customCaptureTypes = true;
                        isPositional = false;
                    }
                    else if (firstChar == '{' && paramPair.Key == NLogLogger.OriginalFormatPropertyName)
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
                        isPositional = false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Extract all valid properties from the input parameterList, and return them in a newly allocated list
        /// </summary>
        private static IReadOnlyList<KeyValuePair<string, object>> CreateValidParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList, out bool customCaptureTypes, out bool isPositional)
        {
            customCaptureTypes = false;
            isPositional = true;
            var validParameterList = new List<KeyValuePair<string, object>>(parameterList.Count);
            for (int i = 0; i < parameterList.Count; ++i)
            {
                var paramPair = parameterList[i];
                if (string.IsNullOrEmpty(paramPair.Key))
                    continue;

                char firstChar = paramPair.Key[0];
                if (!char.IsDigit(firstChar))
                {
                    if (firstChar == '@' || firstChar == '$')
                    {
                        customCaptureTypes = true;
                        isPositional = false;
                    }
                    else if (firstChar == '{' && paramPair.Key == NLogLogger.OriginalFormatPropertyName)
                    {
                        continue;
                    }
                    else
                    {
                        isPositional = false;
                    }
                }

                validParameterList.Add(parameterList[i]);
            }
            return validParameterList;
        }

        public NLog.MessageTemplates.MessageTemplateParameter this[int index]
        {
            get
            {
                if (index >= _originalMessageIndex)
                    index += 1;

                var parameter = _parameterList[index];
                var parameterName = parameter.Key;
                var capture = GetCaptureType(parameterName);
                if (capture != MessageTemplates.CaptureType.Normal)
                    parameterName = RemoveMarkerFromName(parameterName);
                return new NLog.MessageTemplates.MessageTemplateParameter(parameterName, parameter.Value, null, capture);
            }
            set => throw new NotSupportedException();
        }

        private static string RemoveMarkerFromName(string parameterName)
        {
            var firstChar = parameterName[0];
            if (firstChar == '@' || firstChar == '$')
            {
                parameterName = parameterName.Substring(1);
            }
            return parameterName;
        }

        private static NLog.MessageTemplates.CaptureType GetCaptureType(string parameterName)
        {
            switch (parameterName[0])
            {
                case '@': return NLog.MessageTemplates.CaptureType.Serialize;
                case '$': return NLog.MessageTemplates.CaptureType.Stringify;
                default: return NLog.MessageTemplates.CaptureType.Normal;
            }
}

        public int Count => _parameterList.Count - (_originalMessageIndex.HasValue ? 1 : 0);

        public bool IsReadOnly => true;

        public void Add(NLog.MessageTemplates.MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(NLog.MessageTemplates.MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(NLog.MessageTemplates.MessageTemplateParameter[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; ++i)
                array[i + arrayIndex] = this[i];
        }

        public IEnumerator<NLog.MessageTemplates.MessageTemplateParameter> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
                yield return this[i];
        }

        public int IndexOf(NLog.MessageTemplates.MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, NLog.MessageTemplates.MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(NLog.MessageTemplates.MessageTemplateParameter item)
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
