// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MessageActionsPayloadAttachmentTests
    {
        [Fact]
        public void MessageActionPayloadAttachmentInits()
        {
            var id = "attachmentId123";
            var contentType = "text/plain";
            var contentUrl = "https://example.com";
            var content = "text";
            var name = "Message Action Attachment Display Name";
            var thumbnailUrl = "https://example-url-to-thumbnail.com";

            var msgPayloadAttachment = new MessageActionsPayloadAttachment(id, contentType, contentUrl, content, name, thumbnailUrl);

            Assert.NotNull(msgPayloadAttachment);
            Assert.IsType<MessageActionsPayloadAttachment>(msgPayloadAttachment);
            Assert.Equal(id, msgPayloadAttachment.Id);
            Assert.Equal(contentType, msgPayloadAttachment.ContentType);
            Assert.Equal(contentUrl, msgPayloadAttachment.ContentUrl);
            Assert.Equal(content, msgPayloadAttachment.Content);
            Assert.Equal(name, msgPayloadAttachment.Name);
            Assert.Equal(thumbnailUrl, msgPayloadAttachment.ThumbnailUrl);
        }
        
        [Fact]
        public void MessageActionPayloadAttachmentInitsWithNoArgs()
        {
            var msgPayloadAttachment = new MessageActionsPayloadAttachment();

            Assert.NotNull(msgPayloadAttachment);
            Assert.IsType<MessageActionsPayloadAttachment>(msgPayloadAttachment);
        }
    }
}
