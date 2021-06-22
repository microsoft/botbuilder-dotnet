// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MessagingExtensionResultTests
    {
        [Fact]
        public void MessagingExtensionResultInits()
        {
            var attachmentLayout = "list";
            var type = "message";
            var attachments = new List<MessagingExtensionAttachment>() { new MessagingExtensionAttachment() };
            var suggestedActions = new MessagingExtensionSuggestedAction(
                new List<CardAction>()
                { 
                    new CardAction("showImage"),
                    new CardAction("openUrl"),
                });
            var text = "my cup runneth over";
            var activityPreview = new Activity();

            var msgExtResult = new MessagingExtensionResult(attachmentLayout, type, attachments, suggestedActions, text, activityPreview);

            Assert.NotNull(msgExtResult);
            Assert.IsType<MessagingExtensionResult>(msgExtResult);
            Assert.Equal(attachmentLayout, msgExtResult.AttachmentLayout);
            Assert.Equal(type, msgExtResult.Type);
            Assert.Equal(attachments, msgExtResult.Attachments);
            Assert.Equal(suggestedActions, msgExtResult.SuggestedActions);
            Assert.Equal(text, msgExtResult.Text);
            Assert.Equal(activityPreview, msgExtResult.ActivityPreview);
        }
        
        [Fact]
        public void MessagingExtensionResultInitsWithNoArgs()
        {
            var msgExtResult = new MessagingExtensionResult();

            Assert.NotNull(msgExtResult);
            Assert.IsType<MessagingExtensionResult>(msgExtResult);
        }
    }
}
