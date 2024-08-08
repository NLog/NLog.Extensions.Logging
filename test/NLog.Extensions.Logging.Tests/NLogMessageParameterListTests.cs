using System;
using System.Collections.Generic;
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
                new KeyValuePair<string, object>("b", 3),
                new KeyValuePair<string, object>("{OriginalFormat}", "{0}{1}{2}"),
            };
            var list = NLogMessageParameterList.TryParse(items);

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
            var list = NLogMessageParameterList.TryParse(items);

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
            var list = NLogMessageParameterList.TryParse(items);

            Assert.Equal(3, list.Count);
            Assert.Equal(new MessageTemplateParameter("a", 1, null, CaptureType.Normal), list[0]);
            Assert.Equal(new MessageTemplateParameter("b", 2, null, CaptureType.Stringify), list[1]);
            Assert.Equal(new MessageTemplateParameter("c", 3, null, CaptureType.Serialize), list[2]);
            Assert.True(list.HasComplexParameters);
            Assert.False(list.IsPositional);
        }

        [Fact]
        public void CreateNLogMessageParameterIsPositional()
        {
            var items = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("0", 1),
                new KeyValuePair<string, object>("1", 2),
                new KeyValuePair<string, object>("2", 3)
            };
            var list = NLogMessageParameterList.TryParse(items);

            Assert.Empty(list);
            Assert.False(list.HasComplexParameters);
            Assert.True(list.IsPositional);
        }

        [Fact]
        public void CreateNLogMessageParameterMixedPositional()
        {
            var items = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("2", 1),
                new KeyValuePair<string, object>("1", 2),
                new KeyValuePair<string, object>("0", 3)
            };
            var list = NLogMessageParameterList.TryParse(items);

            Assert.Empty(list);
            Assert.True(list.HasComplexParameters);
            Assert.True(list.IsPositional);
        }

        [Fact]
        public void TryParseShouldReturnEmptyListWhenInputIsEmpty()
        {
            var items = new List<KeyValuePair<string, object>>{};
            NLogMessageParameterList parsedList = NLogMessageParameterList.TryParse(items);

            var expectedCount = 0;
            Assert.NotNull(parsedList);
            Assert.Equal(expectedCount, parsedList.Count);
        }
    }
}
