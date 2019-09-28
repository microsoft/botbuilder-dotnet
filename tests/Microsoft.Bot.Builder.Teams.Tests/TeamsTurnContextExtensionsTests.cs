// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Teams.Tests
{
    [TestClass]

    public class TeamsTurnContextExtensionsTests
    {
        [TestMethod]
        public async Task TestTeamsSendToChannelAsync()
        {
            // Arrange
            var destinationConversationAccountId = string.Empty;
            void CaptureSend(Activity[] arg)
            {
                destinationConversationAccountId = arg[0].Conversation.Id;
            }

            var inboundActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount { Id = "originalId" },
            };

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), inboundActivity);

            // Act
            await turnContext.TeamsSendToChannelAsync("teamsChannel123", MessageFactory.Text("hi"));

            // Assert
            Assert.AreEqual("originalId", inboundActivity.Conversation.Id);
            Assert.AreEqual("teamsChannel123", destinationConversationAccountId);
        }

        [TestMethod]
        public async Task TeamsSendToGeneralChannelAsync()
        {
            // Arrange
            var destinationConversationAccountId = string.Empty;
            void CaptureSend(Activity[] arg)
            {
                destinationConversationAccountId = arg[0].Conversation.Id;
            }

            var inboundActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount { Id = "originalId" },
                ChannelData = new TeamsChannelData { Team = new TeamInfo { Id = "team123" } },
            };

            var turnContext = new TurnContext(new SimpleAdapter(CaptureSend), inboundActivity);

            // Act
            await turnContext.TeamsSendToGeneralChannelAsync(MessageFactory.Text("hi"));

            // Assert
            Assert.AreEqual("originalId", inboundActivity.Conversation.Id);
            Assert.AreEqual("team123", destinationConversationAccountId);
        }
    }
}
