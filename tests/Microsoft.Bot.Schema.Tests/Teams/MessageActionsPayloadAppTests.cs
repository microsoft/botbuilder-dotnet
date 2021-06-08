// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MessageActionsPayloadAppTests
    {
        [Fact]
        public void MessageActionPayloadAppInits()
        {
            var applicationIdentityType = "bot";
            var id = "spiffy-qna-bot-id";
            var displayName = "Contoso Bot";

            var messageActionPayloadApp = new MessageActionsPayloadApp(applicationIdentityType, id, displayName);

            Assert.NotNull(messageActionPayloadApp);
            Assert.IsType<MessageActionsPayloadApp>(messageActionPayloadApp);
            Assert.Equal(applicationIdentityType, messageActionPayloadApp.ApplicationIdentityType);
            Assert.Equal(id, messageActionPayloadApp.Id);
            Assert.Equal(displayName, messageActionPayloadApp.DisplayName);
        }
        
        [Fact]
        public void MessageActionPayloadAppInitsWithNoArgs()
        {
            var messageActionPayloadApp = new MessageActionsPayloadApp();

            Assert.NotNull(messageActionPayloadApp);
            Assert.IsType<MessageActionsPayloadApp>(messageActionPayloadApp);
        }
    }
}
