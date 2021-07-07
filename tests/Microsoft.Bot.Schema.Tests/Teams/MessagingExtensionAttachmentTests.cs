// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MessagingExtensionAttachmentTests
    {
        [Fact]
        public void MessagingExtensionAttachmentInits()
        {
            var contentType = "text/plain";
            var contentUrl = "https://example.com";
            var content = "some content";
            var name = "super-plain-attachment";
            var thumbnailUrl = "https://url-to-thumbnail.com";
            var preview = new Attachment();

            var msgExtAttachment = new MessagingExtensionAttachment(contentType, contentUrl, content, name, thumbnailUrl, preview);

            Assert.NotNull(msgExtAttachment);
            Assert.IsType<MessagingExtensionAttachment>(msgExtAttachment);
            Assert.Equal(contentType, msgExtAttachment.ContentType);
            Assert.Equal(contentUrl, msgExtAttachment.ContentUrl);
            Assert.Equal(content, msgExtAttachment.Content);
            Assert.Equal(name, msgExtAttachment.Name);
            Assert.Equal(thumbnailUrl, msgExtAttachment.ThumbnailUrl);
            Assert.Equal(preview, msgExtAttachment.Preview);
        }

        [Fact]
        public void MessagingExtensionAttachmentInitsWithNoArgs()
        {
            var msgExtAttachment = new MessagingExtensionAttachment();

            Assert.NotNull(msgExtAttachment);
            Assert.IsType<MessagingExtensionAttachment>(msgExtAttachment);
        }
    }
}
