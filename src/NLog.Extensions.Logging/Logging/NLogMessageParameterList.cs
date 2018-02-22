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

        public NLogMessageParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList)
        {
            if (IsValidParameterList(parameterList, out _originalMessageIndex))
            {
                _parameterList = parameterList;
            }
            else
            {
                _parameterList = CreateValidParameterList(parameterList);
            }
        }

        /// <summary>
        /// Verify that the input parameterList contains non-empty key-values and the orignal-format-property at the end
        /// </summary>
        private bool IsValidParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList, out int? originalMessageIndex)
        {
            originalMessageIndex = null;
            for (int i = 0; i < parameterList.Count; ++i)
            {
                var paramPair = parameterList[i];
                if (string.IsNullOrEmpty(paramPair.Key))
                {
                    originalMessageIndex = null;
                    return false;
                }

                if (paramPair.Key == NLogLogger.OriginalFormatPropertyName)
                {
                    if (originalMessageIndex.HasValue)
                    {
                        originalMessageIndex = null;
                        return false;
                    }

                    originalMessageIndex = i;
                }
            }

            return true;
        }

        /// <summary>
        /// Extract all valid properties from the input parameterList, and return them in a newly allocated list
        /// </summary>
        private IReadOnlyList<KeyValuePair<string, object>> CreateValidParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList)
        {
            var validParameterList = new List<KeyValuePair<string, object>>(parameterList.Count);
            for (int i = 0; i < parameterList.Count; ++i)
            {
                var paramPair = parameterList[i];
                if (string.IsNullOrEmpty(paramPair.Key))
                    continue;

                if (paramPair.Key == NLogLogger.OriginalFormatPropertyName)
                    continue;

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
            var captureType = NLog.MessageTemplates.CaptureType.Normal;

            switch (parameterName[0])
            {
                case '@':
                    captureType = NLog.MessageTemplates.CaptureType.Serialize;
                    break;
                case '$':
                    captureType = NLog.MessageTemplates.CaptureType.Stringify;
                    break;
            }
            return captureType;
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
