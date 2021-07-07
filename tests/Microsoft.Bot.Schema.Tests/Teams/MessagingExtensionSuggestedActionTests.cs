// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Schema.Teams;
using Xunit;

namespace Microsoft.Bot.Schema.Tests.Teams
{
    public class MessagingExtensionSuggestedActionTests
    {
        [Fact]
        public void MessagingExtensionSuggestedActionInits()
        {
            var cardActions = new List<CardAction>()
            {
                new CardAction("openUrl"),
                new CardAction("imBack"),
                new CardAction("postBack"),
            };
            
            var msgExtSuggestedAction = new MessagingExtensionSuggestedAction(cardActions);

            Assert.NotNull(msgExtSuggestedAction);
            Assert.IsType<MessagingExtensionSuggestedAction>(msgExtSuggestedAction);
            Assert.Equal(cardActions, msgExtSuggestedAction.Actions);
            Assert.Equal(cardActions.Count, msgExtSuggestedAction.Actions.Count);
        }
        
        [Fact]
        public void MessagingExtensionSuggestedActionInitsWithNoArgs()
        {
            var msgExtSuggestedAction = new MessagingExtensionSuggestedAction();

            Assert.NotNull(msgExtSuggestedAction);
            Assert.IsType<MessagingExtensionSuggestedAction>(msgExtSuggestedAction);
        }
    }
}
