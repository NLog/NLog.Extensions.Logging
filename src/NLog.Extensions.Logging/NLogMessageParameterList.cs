using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLog.Extensions.Logging
{
#if !NETSTANDARD1_3
    /// <summary>
    /// Converts Microsoft Extension Logging ParameterList into NLog MessageTemplate ParameterList
    /// </summary>
    internal class NLogMessageParameterList : IList<NLog.MessageTemplates.MessageTemplateParameter>
    {
        private IReadOnlyList<KeyValuePair<string, object>> _parameterList;

        public NLogMessageParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList, bool includesOriginalMessage)
        {
            var validParameterList = includesOriginalMessage ? null : new List<KeyValuePair<string, object>>();
            for (int i = 0; i < parameterList.Count; ++i)
            {
                var paramPair = parameterList[i];
                if (!string.IsNullOrEmpty(paramPair.Key) && (paramPair.Key != NLogLogger.OriginalFormatPropertyName || i == parameterList.Count - 1))
                {
                    if (validParameterList != null && paramPair.Key != NLogLogger.OriginalFormatPropertyName)
                    {
                        validParameterList.Add(paramPair);
                    }
                }
                else
                {
                    if (validParameterList == null)
                    {
                        validParameterList = new List<KeyValuePair<string, object>>();
                        for (int j = 0; j < i; ++i)
                            validParameterList.Add(parameterList[j]);
                    }
                }
            }
            validParameterList?.Add(new KeyValuePair<string, object>());
            _parameterList = validParameterList ?? parameterList;
        }

        public NLog.MessageTemplates.MessageTemplateParameter this[int index]
        {
            get
            {
                var parameter = _parameterList[index];
                var parameterName = parameter.Key;
                NLog.MessageTemplates.CaptureType captureType = NLog.MessageTemplates.CaptureType.Normal;
                switch (parameterName[0])
                {
                    case '@':
                        parameterName = parameterName.Substring(1); 
                        captureType = NLog.MessageTemplates.CaptureType.Serialize;
                        break;
                    case '$':
                        parameterName = parameterName.Substring(1); 
                        captureType = NLog.MessageTemplates.CaptureType.Stringify;
                        break;
                }
                return new NLog.MessageTemplates.MessageTemplateParameter(parameter.Key, parameter.Value, null, captureType);
            }
            set => throw new NotSupportedException();
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
            throw new NotSupportedException();
        }

        public IEnumerator<NLog.MessageTemplates.MessageTemplateParameter> GetEnumerator()
        {
            return _parameterList.Take(_parameterList.Count - 1).Select(p => new NLog.MessageTemplates.MessageTemplateParameter(p.Key, p.Value, null)).GetEnumerator();
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
