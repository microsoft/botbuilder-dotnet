using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class AttachmentDataTests
    {
        [Fact]
        public void SuccessfullyInitAttachmentData()
        {
            var type = "type";
            var name = "name";
            var originalBase64 = new byte[0];
            var thumbnailBase64 = new byte[0];

            var attachmentData = new AttachmentData(type, name, originalBase64, thumbnailBase64);

            Assert.NotNull(attachmentData);
            Assert.IsType<AttachmentData>(attachmentData);
            Assert.Equal(type, attachmentData.Type);
            Assert.Equal(name, attachmentData.Name);
            Assert.Equal(originalBase64, attachmentData.OriginalBase64);
            Assert.Equal(thumbnailBase64, attachmentData.ThumbnailBase64);
        }

        [Fact]
        public void SuccessfullyInitWithNoParams()
        {
            var attachmentData = new AttachmentData();

            Assert.NotNull(attachmentData);
            Assert.IsType<AttachmentData>(attachmentData);
        }
    }
}
