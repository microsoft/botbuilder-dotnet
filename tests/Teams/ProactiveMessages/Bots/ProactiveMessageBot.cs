﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class ProactiveMessageBot : TeamsActivityHandler
    {
        /// <inheritdoc/>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            var connector = new ConnectorClient(connectorClient.Credentials);
            connector.BaseUri = new Uri(turnContext.Activity.ServiceUrl);

            var parameters = new ConversationParameters
            {
                Bot = turnContext.Activity.From,
                Members = new ChannelAccount[] { turnContext.Activity.From },
                ChannelData = new TeamsChannelData
                {
                    Tenant = new TenantInfo
                    {
                        Id = turnContext.Activity.Conversation.TenantId,
                    },
                },
            };

            var converationReference = await connector.Conversations.CreateConversationAsync(parameters);
            var proactiveMessage = MessageFactory.Text($"Hello {turnContext.Activity.From.Name}. You sent me a message. This is a proactive responsive message.");
            proactiveMessage.From = turnContext.Activity.From;
            proactiveMessage.Conversation = new ConversationAccount
            {
                Id = converationReference.Id.ToString(),
            };

            await connector.Conversations.SendToConversationAsync(proactiveMessage, cancellationToken);
        }

        protected override async Task OnTeamsMembersAddedAsync(IList<ChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                var replyActivity = MessageFactory.Text($"{member.Id} was added to the team");
                replyActivity.ApplyConversationReference(turnContext.Activity.GetConversationReference());

                var channelId = turnContext.Activity.Conversation.Id.Split(";")[0];
                replyActivity.Conversation.Id = channelId;
                var resourceResponse = await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            }
        }

        protected override async Task OnTeamsMembersRemovedAsync(IList<ChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                var replyActivity = MessageFactory.Text($"{member.Id} was removed to the team");
                replyActivity.ApplyConversationReference(turnContext.Activity.GetConversationReference());

                var channelId = turnContext.Activity.Conversation.Id.Split(";")[0];
                replyActivity.Conversation.Id = channelId;
                var resourceResponse = await turnContext.SendActivityAsync(replyActivity, cancellationToken);
            }
        }
    }
}
