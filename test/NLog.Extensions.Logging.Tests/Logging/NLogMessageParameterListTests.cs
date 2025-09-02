using System;
using System.Collections.Generic;
using NLog.MessageTemplates;
using Xunit;

namespace NLog.Extensions.Logging.Tests.Logging
{
    public class NLogMessageParameterListTests
    {
        private readonly NLogMessageParameterList _messageParameterList = NLogMessageParameterList.TryParse(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("nr1", "a"),
                new KeyValuePair<string, object>("@nr2", "b"),
            });

        [Fact]
        public void CopyTo_FullCopy_AllCopied()
        {
            // Arrange
            MessageTemplateParameter[] array = new MessageTemplateParameter[2];
            int arrayIndex = 0;

            // Act
            _messageParameterList.CopyTo(array, arrayIndex);

            // Assert
            AssertParameters(array);
        }

        [Fact]
        public void CopyTo_WithOffset_AllCopied()
        {
            // Arrange
            MessageTemplateParameter[] array = new MessageTemplateParameter[3];
            int arrayIndex = 1;

            // Act
            _messageParameterList.CopyTo(array, arrayIndex);

            // Assert
            AssertParameters(array, 1);
        }

        [Fact]
        public void GetEnumerator_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            using (var enumerator = _messageParameterList.GetEnumerator())
            {
                var list = new List<MessageTemplateParameter>();
                while (enumerator.MoveNext())
                    list.Add(enumerator.Current);
                var array = list.ToArray();

                // Assert
                AssertParameters(array);
            }
        }

        [Fact]
        public void Add_ThrowsNotSupported()
        {
            // Arrange
            MessageTemplateParameter item = new MessageTemplateParameter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _messageParameterList.Add(item));
        }

        [Fact]
        public void Clear_ThrowsNotSupported()
        {
            // Arrange

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _messageParameterList.Clear());
        }

        [Fact]
        public void Contains_ThrowsNotSupported()
        {
            // Arrange
            MessageTemplateParameter item = new MessageTemplateParameter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _messageParameterList.Contains(item));
        }


        [Fact]
        public void IndexOf_ThrowsNotSupported()
        {
            // Arrange
            MessageTemplateParameter item = new MessageTemplateParameter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _messageParameterList.IndexOf(item));
        }

        [Fact]
        public void Insert_ThrowsNotSupported()
        {
            // Arrange
            int index = 0;
            MessageTemplateParameter item = new MessageTemplateParameter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _messageParameterList.Insert(index, item));
        }

        [Fact]
        public void Remove_ThrowsNotSupported()
        {
            // Arrange
            MessageTemplateParameter item = new MessageTemplateParameter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _messageParameterList.Remove(item));
        }

        [Fact]
        public void RemoveAt_ThrowsNotSupported()
        {
            // Arrange

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => _messageParameterList.RemoveAt(0));
        }

        private static void AssertParameters(MessageTemplateParameter[] array, int startIndex = 0)
        {
            AssertParameter(array[startIndex], "nr1", "a", CaptureType.Normal);
            AssertParameter(array[startIndex + 1], "nr2", "b", CaptureType.Serialize);
        }

        private static void AssertParameter(MessageTemplateParameter item, string expectedName, string expectedValue, CaptureType captureType)
        {
            Assert.Equal(expectedName, item.Name);
            Assert.Null(item.Format);
            Assert.Null(item.PositionalIndex);
            Assert.Equal(expectedValue, item.Value);
            Assert.Equal(captureType, item.CaptureType);
        }
    }
}
