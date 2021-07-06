// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class AttachmentExtensionsTests
    {
        [Theory]
        [ClassData(typeof(TestPreviewAttachment))]
        public void ToMessagingExtensionAttachment(Attachment previewAttachment)
        {
            var contentType = "contentType";
            var contentUrl = "http://my-content-url.com";
            var content = new { };
            var name = "name";
            var thumbnailUrl = "http://my-thumbnail-url.com";
            var attachment = new Attachment(contentType, contentUrl, new { }, name, thumbnailUrl);

            var messagingExtensionAttachment = AttachmentExtensions.ToMessagingExtensionAttachment(attachment, previewAttachment);

            Assert.NotNull(messagingExtensionAttachment);
            Assert.IsType<MessagingExtensionAttachment>(messagingExtensionAttachment);
            Assert.Equal(contentType, messagingExtensionAttachment.ContentType);
            Assert.Equal(contentUrl, messagingExtensionAttachment.ContentUrl);
            Assert.Equal(content, messagingExtensionAttachment.Content);
            Assert.Equal(name, messagingExtensionAttachment.Name);
            Assert.Equal(thumbnailUrl, messagingExtensionAttachment.ThumbnailUrl);
            
            if (previewAttachment != null)
            {
                Assert.Equal(previewAttachment, messagingExtensionAttachment.Preview);
            }
            else
            {
                var preview = messagingExtensionAttachment.Preview;
                Assert.Equal(contentType, preview.ContentType);
                Assert.Equal(contentUrl, preview.ContentUrl);
                Assert.Equal(name, preview.Name);
                Assert.Equal(thumbnailUrl, preview.ThumbnailUrl);
            }
        }

        internal class TestPreviewAttachment : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { new Attachment() };
                yield return new object[] { null };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
