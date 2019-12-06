// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Rest;

namespace Microsoft.Bot.Builder.TestProtocol
{
    public class RoutingHandler : ChannelServiceHandler
    {
        private readonly SkillConversationIdFactoryBase _factory;
        private readonly ServiceClientCredentials _credentials;

        public RoutingHandler(
            SkillConversationIdFactoryBase factory,
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfiguration,
            IChannelProvider channelProvider = null)
            : base(credentialProvider, authConfiguration, channelProvider)
        {
            _factory = factory;
            _credentials = MicrosoftAppCredentials.Empty;
        }

        protected override async Task<ResourceResponse> OnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            var conversationReference = await _factory.GetConversationReferenceAsync(conversationId, cancellationToken);
            var connectorClient = GetConnectorClient(conversationReference.ServiceUrl);
            activity.ApplyConversationReference(conversationReference);

            return await connectorClient.Conversations.ReplyToActivityAsync(activity, cancellationToken);
        }

        protected override async Task<ResourceResponse> OnSendToConversationAsync(ClaimsIdentity claimsIdentity, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            var conversationReference = await _factory.GetConversationReferenceAsync(conversationId, cancellationToken);
            var connectorClient = GetConnectorClient(conversationReference.ServiceUrl);
            activity.ApplyConversationReference(conversationReference);

            return await connectorClient.Conversations.SendToConversationAsync(activity, cancellationToken);
        }

        protected override async Task<ResourceResponse> OnUpdateActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            var conversationReference = await _factory.GetConversationReferenceAsync(conversationId, cancellationToken);
            var connectorClient = GetConnectorClient(conversationReference.ServiceUrl);
            activity.ApplyConversationReference(conversationReference);

            return await connectorClient.Conversations.UpdateActivityAsync(activity, cancellationToken);
        }

        protected override async Task OnDeleteActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            var conversationReference = await _factory.GetConversationReferenceAsync(conversationId, cancellationToken);
            var connectorClient = GetConnectorClient(conversationReference.ServiceUrl);

            await connectorClient.Conversations.DeleteActivityAsync(conversationReference.Conversation.Id, activityId, cancellationToken);
        }

        protected override async Task<ConversationResourceResponse> OnCreateConversationAsync(ClaimsIdentity claimsIdentity, ConversationParameters parameters, CancellationToken cancellationToken = default)
        {
            // This call will be used in Teams scenarios.

            // Scenario #1 - creating a thread with an activity in a Channel in a Team
            // In order to know the serviceUrl in the case of Teams we would need to look it up based upon the TeamsChannelData.
            // The inbound activity will contain the TeamsChannelData and so will the ConversationParameters.

            // Scenario #2 - starting a one on one conversation with a particular user
            // - needs further analysis -

            var backServiceUrl = "http://tempuri";

            //var (backConversationId, backServiceUrl) = _factory.GetConversationInfo(string.Empty);
            var connectorClient = GetConnectorClient(backServiceUrl);

            return await connectorClient.Conversations.CreateConversationAsync(parameters, cancellationToken);
        }

        protected override Task OnDeleteConversationMemberAsync(ClaimsIdentity claimsIdentity, string conversationId, string memberId, CancellationToken cancellationToken = default)
        {
            return base.OnDeleteConversationMemberAsync(claimsIdentity, conversationId, memberId, cancellationToken);
        }

        protected override Task<IList<ChannelAccount>> OnGetActivityMembersAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            return base.OnGetActivityMembersAsync(claimsIdentity, conversationId, activityId, cancellationToken);
        }

        protected override Task<IList<ChannelAccount>> OnGetConversationMembersAsync(ClaimsIdentity claimsIdentity, string conversationId, CancellationToken cancellationToken = default)
        {
            // In the case of Teams, the conversationId parameter might actually be the TeamId and not the conversationId.
            // In Teams it is only the conversationId when it is a one on one conversation or a group chat.

            return base.OnGetConversationMembersAsync(claimsIdentity, conversationId, cancellationToken);
        }

        protected override Task<ConversationsResult> OnGetConversationsAsync(ClaimsIdentity claimsIdentity, string conversationId, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            return base.OnGetConversationsAsync(claimsIdentity, conversationId, continuationToken, cancellationToken);
        }

        private ConnectorClient GetConnectorClient(string serviceUrl)
        {
            return new ConnectorClient(new Uri(serviceUrl), _credentials);
        }
    }
}
