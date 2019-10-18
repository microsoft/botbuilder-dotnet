// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace Microsoft.Bot.Builder.Teams
{
    public static class TeamsTurnContextExtensions
    {
        public static async Task<(ConversationReference conversationReference, string activityId)> TeamsCreateConversationAsync(this ITurnContext turnContext, string teamsChannelId, IActivity message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(teamsChannelId))
            {
                throw new ArgumentNullException(nameof(teamsChannelId));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // TODO: do we need to add the tenantId into the TeamsChannelData ando/or on the ConversationParameters
            var conversationParameters = new ConversationParameters
            {
                IsGroup = true,
                ChannelData = new TeamsChannelData { Channel = new ChannelInfo(teamsChannelId) },
                Activity = (Activity)message,
            };

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();

            // This call does NOT send the outbound Activity is not being sent through the middleware stack.
            var conversationResourceResponse = await connectorClient.Conversations.CreateConversationAsync(conversationParameters, cancellationToken).ConfigureAwait(false);

            var conversationReference = turnContext.Activity.GetConversationReference();

            conversationReference.Conversation.Id = conversationResourceResponse.Id;

            return (conversationReference, conversationResourceResponse.ActivityId);
        }

        public static async Task TeamsCreateOneToOneConversationAsync(this ITurnContext turnContext, ChannelAccount account, Activity message, CancellationToken cancellationToken = default)
        {
            var connector = turnContext.TurnState.Get<IConnectorClient>();

            var parameters = new ConversationParameters
            {
                IsGroup = false,
                Bot = turnContext.Activity.Recipient,
                Members = new ChannelAccount[] { account },
                ChannelData = new TeamsChannelData
                {
                    Tenant = new TenantInfo
                    {
                        Id = turnContext.Activity.Conversation.TenantId,
                    }
                },
            };

            var converationReference = await connector.Conversations.CreateConversationAsync(parameters).ConfigureAwait(false);


            message.From = turnContext.Activity.Recipient;
            message.Conversation = new ConversationAccount
            {
                Id = converationReference.Id.ToString(),
            };

            await connector.Conversations.SendToConversationAsync(message, cancellationToken).ConfigureAwait(false);
        }

        public static Task<(ConversationReference conversationReference, string activityId)> TeamsSendToGeneralChannelAsync(this ITurnContext turnContext, IActivity activity, CancellationToken cancellationToken = default)
        {
            // The Team Id is also the Id of the general channel
            var teamId = turnContext.Activity.TeamsGetTeamId();

            if (string.IsNullOrEmpty(teamId))
            {
                throw new Exception("The current Activity was not sent from a Teams Team.");
            }

            return turnContext.TeamsCreateConversationAsync(teamId, activity, cancellationToken);
        }
    }
}
