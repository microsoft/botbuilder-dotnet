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
        public static async Task<ResourceResponse> TeamsSendToChannelAsync(this ITurnContext turnContext, string teamsChannelId, IActivity activity, CancellationToken cancellationToken = default)
        {
            var originalConversationId = turnContext.Activity.Conversation.Id;
            try
            {
                turnContext.Activity.Conversation.Id = teamsChannelId;
                var result = await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                return result;
            }
            finally
            {
                turnContext.Activity.Conversation.Id = originalConversationId;
            }
        }

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

            var channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();

            if (channelData == null)
            {
                throw new InvalidOperationException("This method is only valid within the scope of MS Teams TurnContext.");
            }

            // TODO: do we need to add the tenantId into the TeamsChannelData
            var conversationParameters = new ConversationParameters
            {
                IsGroup = true,
                ChannelData = new TeamsChannelData { Channel = new ChannelInfo(teamsChannelId) },
                Activity = (Activity)message,
            };

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();

            // This call does NOT send the outbound Activity is not being sent through the middleware stack.
            var conversationResourceResponse = await connectorClient.Conversations.CreateConversationAsync(conversationParameters, cancellationToken);

            var conversationReference = turnContext.Activity.GetConversationReference();

            conversationReference.Conversation.Id = conversationResourceResponse.Id;

            return (conversationReference, conversationResourceResponse.ActivityId);
        }

        public static Task<ResourceResponse> TeamsSendToGeneralChannelAsync(this ITurnContext turnContext, IActivity activity, CancellationToken cancellationToken = default)
        {
            // The Team Id is also the Id of the general channel
            var teamId = turnContext.Activity.TeamsGetTeamId();

            if (string.IsNullOrEmpty(teamId))
            {
                throw new Exception("The current Activity was not sent from a Teams Team.");
            }

            return turnContext.TeamsSendToChannelAsync(teamId, activity, cancellationToken);
        }
    }
}
