using System;
using System.Collections.Generic;
using System.Text;
using NLog.MessageTemplates;
using Xunit;

namespace NLog.Extensions.Logging.Tests
{
    public class NLogMessageParameterListTests
    {
        [Fact]
        public void CreateNLogMessageParameterListWithEmptyKey()
        {
            var items = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("", 1),
                new KeyValuePair<string, object>("a", 2),
                new KeyValuePair<string, object>("b", 3)
            };
            var list = new NLogMessageParameterList(items);

            Assert.Equal(2, list.Count);
            Assert.Equal(new MessageTemplateParameter("a", 2, null, CaptureType.Normal), list[0]);
            Assert.Equal(new MessageTemplateParameter("b", 3, null, CaptureType.Normal), list[1]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(1)]
        public void CreateNLogMessageParameterListWithOriginalFormatKey(object originalFormat)
        {
            var items = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("a", 2),
                new KeyValuePair<string, object>("{OriginalFormat}", originalFormat),
                new KeyValuePair<string, object>("b", 3)
            };
            var list = new NLogMessageParameterList(items);

            Assert.Equal(2, list.Count);
            Assert.Equal(new MessageTemplateParameter("a", 2, null, CaptureType.Normal), list[0]);
            Assert.Equal(new MessageTemplateParameter("b", 3, null, CaptureType.Normal), list[1]);
        }      
        
        [Fact]
        public void CreateNLogMessageParameterDifferentCaptureTypes()
        {
            var items = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("a", 1),
                new KeyValuePair<string, object>("$b", 2),
                new KeyValuePair<string, object>("@c", 3)
            };
            var list = new NLogMessageParameterList(items);

            Assert.Equal(3, list.Count);
            Assert.Equal(new MessageTemplateParameter("a", 1, null, CaptureType.Normal), list[0]);
            Assert.Equal(new MessageTemplateParameter("b", 2, null, CaptureType.Stringify), list[1]);
            Assert.Equal(new MessageTemplateParameter("c", 3, null, CaptureType.Serialize), list[2]);
        }

    }
}
