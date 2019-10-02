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
    public class TeamsInfo
    {
        private IConnectorClient _connectorClient;
        private ITeamsConnectorClient _teamsConnectorClient;

        public TeamsInfo(ConnectorClient connectorClient)
        {
            _connectorClient = connectorClient ?? throw new ArgumentNullException(nameof(connectorClient));
            _teamsConnectorClient = new TeamsConnectorClient(connectorClient.BaseUri, connectorClient.Credentials, connectorClient.HttpClient);
        }

        public async Task<TeamDetails> GetTeamDetailsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var teamId = turnContext.Activity.GetChannelData<TeamsChannelData>()?.Team?.Id;

            if (teamId == null)
            {
                throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            }

            return await _teamsConnectorClient.Teams.FetchTeamDetailsAsync(teamId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IList<ChannelInfo>> GetChannelsAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var teamId = turnContext.Activity.GetChannelData<TeamsChannelData>()?.Team?.Id;

            if (teamId == null)
            {
                throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            }

            var channelList = await _teamsConnectorClient.Teams.FetchChannelListAsync(teamId, cancellationToken).ConfigureAwait(false);
            return channelList.Conversations;
        }

        public Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var teamId = turnContext.Activity.GetChannelData<TeamsChannelData>()?.Team?.Id;

            if (teamId != null)
            {
                return GetMembersAsync(teamId, cancellationToken);
            }
            else
            {
                var conversationId = turnContext.Activity?.Conversation?.Id;
                return GetMembersAsync(conversationId, cancellationToken);
            }
        }

        private async Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(string conversationId, CancellationToken cancellationToken)
        {
            if (conversationId == null)
            {
                throw new InvalidOperationException("The GetMembers operation needs a valid conversation Id.");
            }

            var teamMembers = await _connectorClient.Conversations.GetConversationMembersAsync(conversationId, cancellationToken).ConfigureAwait(false);
            var teamsChannelAccounts = teamMembers.Select(channelAccount => JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>());
            return teamsChannelAccounts;
        }
    }
}
