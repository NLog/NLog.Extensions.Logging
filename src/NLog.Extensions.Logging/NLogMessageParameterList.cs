using System;
using System.Collections;
using System.Collections.Generic;

namespace NLog.Extensions.Logging
{
#if !NETSTANDARD1_3
    /// <summary>
    /// Converts Microsoft Extension Logging ParameterList into NLog MessageTemplate ParameterList
    /// </summary>
    internal class NLogMessageParameterList : IList<NLog.MessageTemplates.MessageTemplateParameter>
    {
        private readonly IReadOnlyList<KeyValuePair<string, object>> _parameterList;

        public NLogMessageParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList, bool includesOriginalMessage)
        {
            if (!includesOriginalMessage || !IsValidParameterList(parameterList))
            {
                _parameterList = CreateValidParameterList(parameterList);
            }
            else
            {
                _parameterList = parameterList;
            }
        }

        /// <summary>
        /// Verify that the input parameterList contains non-empty key-values and the orignal-format-property at the end
        /// </summary>
        private bool IsValidParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList)
        {
            int parameterCount = parameterList.Count - 1;
            for (int i = 0; i <= parameterCount; ++i)
            {
                var paramPair = parameterList[i];
                if (!ValidParameterKey(paramPair.Key, i == parameterCount))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Extract all valid properties from the input parameterList, and return them in a newly allocated list
        /// </summary>
        private IReadOnlyList<KeyValuePair<string, object>> CreateValidParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList)
        {
            var validParameterList = new List<KeyValuePair<string, object>>(parameterList.Count + 1);
            for (int i = 0; i < parameterList.Count; ++i)
            {
                var paramPair = parameterList[i];
                if (!ValidParameterKey(paramPair.Key, false))
                    continue;

                validParameterList.Add(parameterList[i]);
            }
            validParameterList.Add(new KeyValuePair<string, object>()); // Simulate NLogLogger.OriginalFormatPropertyName
            return validParameterList;
        }

        private bool ValidParameterKey(string keyValue, bool lastKey)
        {
            if (string.IsNullOrEmpty(keyValue))
                return false;   // Non-empty string not allowed

            if (keyValue == NLogLogger.OriginalFormatPropertyName)
                return lastKey; // Original format message, must be last parameter

            if (lastKey)
                return false;   // Original format message, must be last parameter

            return true;
        }

        public NLog.MessageTemplates.MessageTemplateParameter this[int index]
        {
            get
            {
                var parameter = _parameterList[index];
                var parameterName = parameter.Key;
                var capture = GetCaptureType(parameterName);
                parameterName = NLogLogger.RemoveMarkerFromName(parameterName);
                return new NLog.MessageTemplates.MessageTemplateParameter(parameterName, parameter.Value, null, capture);
            }
            set => throw new NotSupportedException();
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

        public int Count => _parameterList.Count - 1;

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
#endif
}
