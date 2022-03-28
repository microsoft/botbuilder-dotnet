// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Microsoft.Bot.Connector.Client.Authentication;
using Microsoft.Bot.Connector.Client.Models;

namespace Microsoft.Bot.Connector.Client
{
    internal class ConnectorClientImpl : ConnectorClient
    {
        private readonly ConversationsRestClient _conversations;
        private readonly AttachmentsRestClient _attachments;

        public ConnectorClientImpl(BotFrameworkCredential credential, string scope, Uri endpoint)
        {
            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            var options = new ConnectorOptions();
            var diagnostics = new ClientDiagnostics(options);
            var pipeline = credential.IsAuthenticationDisabledAsync().GetAwaiter().GetResult() || string.IsNullOrWhiteSpace(scope)
                ? HttpPipelineBuilder.Build(options)
                : HttpPipelineBuilder.Build(options, new BearerTokenAuthenticationPolicy(credential.GetTokenCredential(), scope));

            _conversations = new ConversationsRestClient(diagnostics, pipeline, endpoint);
            _attachments = new AttachmentsRestClient(diagnostics, pipeline, endpoint);
        }

        public override async Task<ConversationsResult> GetConversationsAsync(string continuationToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(continuationToken))
            {
                continuationToken = null; // pass null to avoid creation of query parameter
            }

            var response = await _conversations.GetConversationsAsync(continuationToken, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<ConversationResourceResponse> CreateConversationAsync(ConversationParameters parameters, CancellationToken cancellationToken = default)
        {
            var response = await _conversations.CreateConversationAsync(parameters, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<ResourceResponse> SendToConversationAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            var response = await _conversations.SendToConversationAsync(activity.Conversation.Id, activity, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<ResourceResponse> SendConversationHistoryAsync(string conversationId, Transcript history, CancellationToken cancellationToken = default)
        {
            var response = await _conversations.SendConversationHistoryAsync(conversationId, history, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<ResourceResponse> UpdateActivityAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            var response = await _conversations.UpdateActivityAsync(activity.Conversation.Id, activity.Id, activity, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<ResourceResponse> ReplyToActivityAsync(Activity activity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(activity.ReplyToId))
            {
                throw new ArgumentException($"{nameof(activity.ReplyToId)} cannot be null or empty!");
            }

            var response = await _conversations.ReplyToActivityAsync(activity.Conversation.Id, activity.ReplyToId, activity, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task DeleteActivityAsync(string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            await _conversations.DeleteActivityAsync(conversationId, activityId, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<IReadOnlyList<ChannelAccount>> GetConversationMembersAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            var response = await _conversations.GetConversationMembersAsync(conversationId, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<ChannelAccount> GetConversationMemberAsync(string conversationId, string memberId, CancellationToken cancellationToken = default)
        {
            var response = await _conversations.GetConversationMemberAsync(conversationId, memberId, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task DeleteConversationMemberAsync(string conversationId, string memberId, CancellationToken cancellationToken = default)
        {
            await _conversations.DeleteConversationMemberAsync(conversationId, memberId, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<PagedMembersResult> GetConversationPagedMembersAsync(string conversationId, int? pageSize = null, string continuationToken = "", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(continuationToken))
            {
                continuationToken = null; // pass null to avoid creation of query parameter
            }

            var response = await _conversations.GetConversationPagedMembersAsync(conversationId, pageSize, continuationToken, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<IReadOnlyList<ChannelAccount>> GetActivityMembersAsync(string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            var response = await _conversations.GetActivityMembersAsync(conversationId, activityId, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<ResourceResponse> UploadAttachmentAsync(string conversationId, AttachmentData attachmentUpload, CancellationToken cancellationToken = default)
        {
            var response = await _conversations.UploadAttachmentAsync(conversationId, attachmentUpload, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<AttachmentInfo> GetAttachmentInfoAsync(string attachmentId, CancellationToken cancellationToken = default)
        {
            var response = await _attachments.GetAttachmentInfoAsync(attachmentId, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }

        public override async Task<Stream> GetAttachmentAsync(string attachmentId, string viewId, CancellationToken cancellationToken = default)
        {
            var response = await _attachments.GetAttachmentAsync(attachmentId, viewId, cancellationToken).ConfigureAwait(false);
            return response.Value;
        }
    }
}
