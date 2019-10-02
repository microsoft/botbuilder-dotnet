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
        private string _conversationId;
        private IConnectorClient _connectorClient;
        private ITeamsConnectorClient _teamsConnectorClient;
        private TeamsChannelData _teamsChannelData;

        public TeamsInfo(string conversationId, ConnectorClient connectorClient, TeamsChannelData teamsChannelData)
        {
            if (connectorClient != null)
            {
                _connectorClient = connectorClient;
                _teamsConnectorClient = new TeamsConnectorClient(connectorClient.BaseUri, connectorClient.Credentials, connectorClient.HttpClient);
            }

            _conversationId = conversationId;
            _teamsChannelData = teamsChannelData;
        }

        public async Task<TeamDetails> GetTeamDetailsAsync(CancellationToken cancellationToken = default)
        {
            if (_teamsConnectorClient == null)
            {
                throw new NotImplementedException("This method is only implemented for the MS Teams channel.");
            }

            if (_teamsChannelData?.Team?.Id == null)
            {
                throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            }

            return await _teamsConnectorClient.Teams.FetchTeamDetailsAsync(_teamsChannelData.Team.Id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IList<ChannelInfo>> GetChannelsAsync(CancellationToken cancellationToken = default)
        {
            if (_teamsConnectorClient == null)
            {
                throw new NotImplementedException("This method is only implemented for the MS Teams channel.");
            }

            if (_teamsChannelData?.Team?.Id == null)
            {
                throw new InvalidOperationException("This method is only valid within the scope of MS Teams Team.");
            }

            var channelList = await _teamsConnectorClient.Teams.FetchChannelListAsync(_teamsChannelData.Team?.Id, cancellationToken).ConfigureAwait(false);
            return channelList.Conversations;
        }

        public Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(CancellationToken cancellationToken = default)
        {
            if (_teamsChannelData?.Team?.Id != null)
            {
                return GetMembersAsync(_teamsChannelData.Team.Id, cancellationToken);
            }
            else
            {
                return GetMembersAsync(_conversationId, cancellationToken);
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
