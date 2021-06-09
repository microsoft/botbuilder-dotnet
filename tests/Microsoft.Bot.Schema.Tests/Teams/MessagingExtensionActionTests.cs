// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MessagingExtensionActionTests
    {
        [Fact]
        public void MessagingExtensionActionInits()
        {
            var data = new Dictionary<string, string>() { { "key", "value" } };
            var context = new TaskModuleRequestContext("theme");
            var commandId = "commandId";
            var commandContext = "message";
            var botMessagePreviewAction = "send";
            var botActivityPreview = new List<Activity>() { new Activity(text: "hi"), new Activity(text: "yo yo yo") };
            var messagePayload = new MessageActionsPayload("msgId", "1234", "message");
            var state = "secureOAuthState1234";

            var msgExtAction = new MessagingExtensionAction(data, context, commandId, commandContext, botMessagePreviewAction, botActivityPreview, messagePayload)
            { 
                State = state
            };

            Assert.NotNull(msgExtAction);
            Assert.IsType<MessagingExtensionAction>(msgExtAction);
            Assert.Equal(data, msgExtAction.Data);
            Assert.Equal(context, msgExtAction.Context);
            Assert.Equal(commandId, msgExtAction.CommandId);
            Assert.Equal(commandContext, msgExtAction.CommandContext);
            Assert.Equal(botMessagePreviewAction, msgExtAction.BotMessagePreviewAction);
            Assert.Equal(botActivityPreview, msgExtAction.BotActivityPreview);
            Assert.Equal(messagePayload, msgExtAction.MessagePayload);
            Assert.Equal(state, msgExtAction.State);
        }
        
        [Fact]
        public void MessagingExtensionActionInitsNoArgs()
        {
            var msgExtAction = new MessagingExtensionAction();

            Assert.NotNull(msgExtAction);
            Assert.IsType<MessagingExtensionAction>(msgExtAction);
        }
    }
}
