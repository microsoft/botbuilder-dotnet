// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Schema.Tests
{
    public class AttachmentTests
    {
        [Fact]
        public void AttachmentInits()
        {
            var contentType = "contentType";
            var contentUrl = "contentUrl";
            var content = new { };
            var name = "name";
            var thumbnailUrl = "thumbnailUrl";
            var properties = new JObject();

            var attachment = new Attachment(contentType, contentUrl, content, name, thumbnailUrl)
            {
                Properties = properties
            };

            Assert.NotNull(attachment);
            Assert.IsType<Attachment>(attachment);
            Assert.Equal(contentType, attachment.ContentType);
            Assert.Equal(contentUrl, attachment.ContentUrl);
            Assert.Equal(content, attachment.Content);
            Assert.Equal(name, attachment.Name);
            Assert.Equal(thumbnailUrl, attachment.ThumbnailUrl);
            Assert.Equal(properties, attachment.Properties);
        }

        [Fact]
        public void AttachmentDataInits()
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
        public void AttachmentDataInitsWithNoArgs()
        {
            var attachmentData = new AttachmentData();

            Assert.NotNull(attachmentData);
            Assert.IsType<AttachmentData>(attachmentData);
        }

        [Fact]
        public void AttachmentInfoInits()
        {
            var name = "name";
            var type = "type";
            var views = new List<AttachmentView>() { new AttachmentView() };

            var attachmentInfo = new AttachmentInfo(name, type, views);

            Assert.NotNull(attachmentInfo);
            Assert.IsType<AttachmentInfo>(attachmentInfo);
            Assert.Equal(name, attachmentInfo.Name);
            Assert.Equal(type, attachmentInfo.Type);
            Assert.Equal(views, attachmentInfo.Views);
        }

        [Fact]
        public void AttachmentInfoInitsWithNoArgs()
        {
            var attachmentInfo = new AttachmentInfo();

            Assert.NotNull(attachmentInfo);
            Assert.IsType<AttachmentInfo>(attachmentInfo);
        }

        [Fact]
        public void AttachmentViewInits()
        {
            var viewId = "viewId";
            var size = 5;

            var attachmentView = new AttachmentView(viewId, size);

            Assert.NotNull(attachmentView);
            Assert.Equal(viewId, attachmentView.ViewId);
            Assert.Equal(size, attachmentView.Size);
        }
    }
}
