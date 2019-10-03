// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Teams
{
    public static class TeamsInfo
    {
        public static async Task<TeamDetails> GetTeamDetailsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var teamId = GetTeamId(turnContext) ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            return await GetTeamsConnectorClient(turnContext).Teams.FetchTeamDetailsAsync(teamId, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<IList<ChannelInfo>> GetChannelsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var teamId = GetTeamId(turnContext) ?? throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            var channelList = await GetTeamsConnectorClient(turnContext).Teams.FetchChannelListAsync(teamId, cancellationToken).ConfigureAwait(false);
            return channelList.Conversations;
        }

        public static Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var connectorClient = GetConnectorClient(turnContext);
            var teamId = GetTeamId(turnContext);

            if (teamId != null)
            {
                return GetMembersAsync(connectorClient, teamId, cancellationToken);
            }
            else
            {
                var conversationId = turnContext.Activity?.Conversation?.Id;
                return GetMembersAsync(connectorClient, conversationId, cancellationToken);
            }
        }

        private static async Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(IConnectorClient connectorClient, string conversationId, CancellationToken cancellationToken)
        {
            if (conversationId == null)
            {
                throw new InvalidOperationException("The GetMembers operation needs a valid conversation Id.");
            }

            var teamMembers = await connectorClient.Conversations.GetConversationMembersAsync(conversationId, cancellationToken).ConfigureAwait(false);
            var teamsChannelAccounts = teamMembers.Select(channelAccount => JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>());
            return teamsChannelAccounts;
        }

        private static string GetTeamId(ITurnContext turnContext)
        {
            return turnContext.Activity.GetChannelData<TeamsChannelData>()?.Team?.Id;
        }

        private static IConnectorClient GetConnectorClient(ITurnContext turnContext)
        {
            return turnContext.TurnState.Get<IConnectorClient>() ?? throw new InvalidOperationException("This method requires a connector client.");
        }

        private static ITeamsConnectorClient GetTeamsConnectorClient(ITurnContext turnContext)
        {
            var connectorClient = GetConnectorClient(turnContext);
            if (connectorClient is ConnectorClient)
            {
                var connectorClientImpl = (ConnectorClient)connectorClient;
                return new TeamsConnectorClient(connectorClientImpl.BaseUri, connectorClientImpl.Credentials, connectorClientImpl.HttpClient);
            }
            else
            {
                return new TeamsConnectorClient(connectorClient.BaseUri, connectorClient.Credentials);
            }
        }
    }
}
