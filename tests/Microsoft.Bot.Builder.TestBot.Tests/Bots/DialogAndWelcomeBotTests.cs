﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TestBot.Shared.Bots;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Tests.Framework;
using Xunit;

namespace Microsoft.BotBuilderSamples.Tests.Bots
{
    public class DialogAndWelcomeBotTests
    {
        [Fact]
        public async Task ReturnsWelcomeCardOnConversationUpdate()
        {
            // Arrange
            var mockRootDialog = SimpleMockFactory.CreateMockDialog<Dialog>(null, "mockRootDialog");

            // TODO: do we need state here?
            var memoryStorage = new MemoryStorage();
            var sut = new DialogAndWelcomeBot<Dialog>(new ConversationState(memoryStorage), new UserState(memoryStorage), mockRootDialog.Object, null);
            var conversationUpdateActivity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                Recipient = new ChannelAccount { Id = "theBot" },
            };
            conversationUpdateActivity.MembersAdded.Add(new ChannelAccount { Id = "theUser" });
            var testAdapter = new TestAdapter(Channels.Test);

            // Act
            // Note: it is kind of obscure that we need to use OnTurnAsync to trigger OnMembersAdded so we get the card
            await testAdapter.ProcessActivityAsync(conversationUpdateActivity, sut.OnTurnAsync, CancellationToken.None);
            var reply = testAdapter.GetNextReply();

            // Assert
            var m = (IMessageActivity)reply;
            Assert.Equal(1, m.Attachments.Count);
            Assert.Equal("application/vnd.microsoft.card.adaptive", m.Attachments.FirstOrDefault()?.ContentType);
        }
    }
}
