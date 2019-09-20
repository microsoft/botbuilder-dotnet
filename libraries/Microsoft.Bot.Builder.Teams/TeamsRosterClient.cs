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
    #pragma warning disable CA1001
    internal class TeamsRosterClient
    {
        private ConnectorClient _connectorClient;
        private TeamsConnectorClient _teamsConnectorClient;

        public TeamsRosterClient(ConnectorClient connectorClient)
        {
            _connectorClient = connectorClient;
            _teamsConnectorClient = new TeamsConnectorClient(connectorClient.BaseUri, connectorClient.Credentials, connectorClient.HttpClient);
        }

        public async Task<TeamDetails> GetTeamDetailsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var teamDetails = await _teamsConnectorClient.Teams.FetchTeamDetailsAsync(turnContext.Activity.GetChannelData<TeamsChannelData>().Team.Id, cancellationToken).ConfigureAwait(false);
            return teamDetails;
        }

        public async Task<IList<ChannelInfo>> GetChannelsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var channelList = await _teamsConnectorClient.Teams.FetchChannelListAsync(turnContext.Activity.GetChannelData<TeamsChannelData>().Team.Id, cancellationToken).ConfigureAwait(false);
            return channelList.Conversations;
        }

        public Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity.GetChannelData<TeamsChannelData>()?.Team?.Id != null)
            {
                return GetMembersAsync(turnContext.Activity.GetChannelData<TeamsChannelData>().Team.Id, cancellationToken);
            }
            else
            {
                return GetMembersAsync(turnContext.Activity.Conversation.Id, cancellationToken);
            }
        }

        private async Task<IEnumerable<TeamsChannelAccount>> GetMembersAsync(string conversationId, CancellationToken cancellationToken)
        {
            var teamMembers = await _connectorClient.Conversations.GetConversationMembersAsync(conversationId, cancellationToken).ConfigureAwait(false);
            var teamsChannelAccounts = teamMembers.Select(channelAccount => JObject.FromObject(channelAccount).ToObject<TeamsChannelAccount>());
            return teamsChannelAccounts;
        }
    }
}
