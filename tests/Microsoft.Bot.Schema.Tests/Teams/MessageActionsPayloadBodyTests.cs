// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MessageActionsPayloadBodyTests
    {
        [Fact]
        public void MessageActionsPayloadBodyInits()
        {
            var contentType = "text/plain";
            var content = "Have a wonderful day!";

            var msgActionsPayloadBody = new MessageActionsPayloadBody(contentType, content);

            Assert.NotNull(msgActionsPayloadBody);
            Assert.IsType<MessageActionsPayloadBody>(msgActionsPayloadBody);
            Assert.Equal(contentType, msgActionsPayloadBody.ContentType);
            Assert.Equal(content, msgActionsPayloadBody.Content);
        }
        
        [Fact]
        public void MessageActionsPayloadBodyInitsWithNoArgs()
        {
            var msgActionsPayloadBody = new MessageActionsPayloadBody();

            Assert.NotNull(msgActionsPayloadBody);
            Assert.IsType<MessageActionsPayloadBody>(msgActionsPayloadBody);
        }
    }
}
