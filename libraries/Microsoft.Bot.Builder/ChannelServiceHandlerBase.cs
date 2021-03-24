// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Base class for Bot Framework protocol implementation.
    /// </summary>
    public abstract class ChannelServiceHandlerBase
    {
        /// <summary>
        /// Sends an activity to the end of a conversation.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="conversationId">The conversation Id.</param>
        /// <param name="activity">The activity to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<ResourceResponse> HandleSendToConversationAsync(string authHeader, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            return await OnSendToConversationAsync(claimsIdentity, conversationId, activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a reply to an activity.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="conversationId">The conversation Id.</param>
        /// <param name="activityId">The activity Id the reply is to.</param>
        /// <param name="activity">The activity to send.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<ResourceResponse> HandleReplyToActivityAsync(string authHeader, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            return await OnReplyToActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Edits a previously sent existing activity.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="conversationId">The conversation Id.</param>
        /// <param name="activityId">The activity Id to update.</param>
        /// <param name="activity">The replacement activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<ResourceResponse> HandleUpdateActivityAsync(string authHeader, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            return await OnUpdateActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an existing activity.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="conversationId">The conversation Id.</param>
        /// <param name="activityId">The activity Id.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task HandleDeleteActivityAsync(string authHeader, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            await OnDeleteActivityAsync(claimsIdentity, conversationId, activityId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Enumerates the members of an activity.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="conversationId">The conversation Id.</param>
        /// <param name="activityId">The activity Id.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IList<ChannelAccount>> HandleGetActivityMembersAsync(string authHeader, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            return await OnGetActivityMembersAsync(claimsIdentity, conversationId, activityId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a new Conversation.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="parameters">Parameters to create the conversation from.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<ConversationResourceResponse> HandleCreateConversationAsync(string authHeader, ConversationParameters parameters, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            return await OnCreateConversationAsync(claimsIdentity, parameters, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Lists the Conversations in which the bot has participated.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="conversationId">The conversation Id.</param>
        /// <param name="continuationToken">A skip or continuation token.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<ConversationsResult> HandleGetConversationsAsync(string authHeader, string conversationId, string continuationToken = default, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            return await OnGetConversationsAsync(claimsIdentity, conversationId, continuationToken, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Enumerates the members of a conversation.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="conversationId">The conversation Id.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IList<ChannelAccount>> HandleGetConversationMembersAsync(string authHeader, string conversationId, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            return await OnGetConversationMembersAsync(claimsIdentity, conversationId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Enumerates the members of a conversation one page at a time.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="conversationId">The conversation Id.</param>
        /// <param name="pageSize">Suggested page size.</param>
        /// <param name="continuationToken">A continuation token.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<PagedMembersResult> HandleGetConversationPagedMembersAsync(string authHeader, string conversationId, int? pageSize = default, string continuationToken = default, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            return await OnGetConversationPagedMembersAsync(claimsIdentity, conversationId, pageSize, continuationToken, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a member from a conversation.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="conversationId">The conversation Id.</param>
        /// <param name="memberId">Id of the member to delete from this conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task HandleDeleteConversationMemberAsync(string authHeader, string conversationId, string memberId, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            await OnDeleteConversationMemberAsync(claimsIdentity, conversationId, memberId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Uploads the historic activities of the conversation.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="conversationId">The conversation Id.</param>
        /// <param name="transcript">Transcript of activities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<ResourceResponse> HandleSendConversationHistoryAsync(string authHeader, string conversationId, Transcript transcript, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            return await OnSendConversationHistoryAsync(claimsIdentity, conversationId, transcript, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Stores data in a compliant store when dealing with enterprises.
        /// </summary>
        /// <param name="authHeader">The authentication header.</param>
        /// <param name="conversationId">The conversation Id.</param>
        /// <param name="attachmentUpload">Attachment data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<ResourceResponse> HandleUploadAttachmentAsync(string authHeader, string conversationId, AttachmentData attachmentUpload, CancellationToken cancellationToken = default)
        {
            var claimsIdentity = await AuthenticateAsync(authHeader, cancellationToken).ConfigureAwait(false);
            return await OnUploadAttachmentAsync(claimsIdentity, conversationId, attachmentUpload, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Helper to authenticate the header token and extract the claims.
        /// </summary>
        /// <param name="authHeader">The auth header containing JWT token.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="ClaimsIdentity"/> representing the claims associated with given header.</returns>
        internal abstract Task<ClaimsIdentity> AuthenticateAsync(string authHeader, CancellationToken cancellationToken);

        /// <summary>
        /// SendToConversation() API for Skill.
        /// </summary>
        /// <remarks>
        /// This method allows you to send an activity to the end of a conversation.
        ///
        /// This is slightly different from ReplyToActivity().
        /// * SendToConversation(conversationId) - will append the activity to the end
        /// of the conversation according to the timestamp or semantics of the channel.
        /// * ReplyToActivity(conversationId,ActivityId) - adds the activity as a reply
        /// to another activity, if the channel supports it. If the channel does not
        /// support nested replies, ReplyToActivity falls back to SendToConversation.
        ///
        /// Use ReplyToActivity when replying to a specific activity in the
        /// conversation.
        ///
        /// Use SendToConversation in all other cases.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>conversationId.</param> 
        /// <param name='activity'>Activity to send.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        protected virtual Task<ResourceResponse> OnSendToConversationAsync(ClaimsIdentity claimsIdentity, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// OnReplyToActivityAsync() API.
        /// </summary>
        /// <remarks>
        /// Override this method allows to reply to an Activity.
        ///
        /// This is slightly different from SendToConversation().
        /// * SendToConversation(conversationId) - will append the activity to the end
        /// of the conversation according to the timestamp or semantics of the channel.
        /// * ReplyToActivity(conversationId,ActivityId) - adds the activity as a reply
        /// to another activity, if the channel supports it. If the channel does not
        /// support nested replies, ReplyToActivity falls back to SendToConversation.
        ///
        /// Use ReplyToActivity when replying to a specific activity in the
        /// conversation.
        ///
        /// Use SendToConversation in all other cases.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='activityId'>activityId the reply is to (OPTIONAL).</param>
        /// <param name='activity'>Activity to send.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        protected virtual Task<ResourceResponse> OnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// OnUpdateActivityAsync() API.
        /// </summary>
        /// <remarks>
        /// Override this method to edit a previously sent existing activity.
        ///
        /// Some channels allow you to edit an existing activity to reflect the new
        /// state of a bot conversation.
        ///
        /// For example, you can remove buttons after someone has clicked "Approve"
        /// button.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='activityId'>activityId to update.</param>
        /// <param name='activity'>replacement Activity.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        protected virtual Task<ResourceResponse> OnUpdateActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// OnDeleteActivityAsync() API.
        /// </summary>
        /// <remarks>
        /// Override this method to Delete an existing activity.
        ///
        /// Some channels allow you to delete an existing activity, and if successful
        /// this method will remove the specified activity.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='activityId'>activityId to delete.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        protected virtual Task OnDeleteActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// OnGetActivityMembersAsync() API.
        /// </summary>
        /// <remarks>
        /// Override this method to enumerate the members of an activity.
        ///
        /// This REST API takes a ConversationId and a ActivityId, returning an array
        /// of ChannelAccount objects representing the members of the particular
        /// activity in the conversation.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='activityId'>Activity ID.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task with result.</returns>
        protected virtual Task<IList<ChannelAccount>> OnGetActivityMembersAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CreateConversation() API.
        /// </summary>
        /// <remarks>
        /// Override this method to create a new Conversation.
        ///
        /// POST to this method with a
        /// * Bot being the bot creating the conversation
        /// * IsGroup set to true if this is not a direct message (default is false)
        /// * Array containing the members to include in the conversation
        ///
        /// The return value is a ResourceResponse which contains a conversation ID
        /// which is suitable for use
        /// in the message payload and REST API URIs.
        ///
        /// Most channels only support the semantics of bots initiating a direct
        /// message conversation.  An example of how to do that would be:
        ///
        /// var resource = await connector.conversations.CreateConversation(new
        /// ConversationParameters(){ Bot = bot, members = new ChannelAccount[] { new
        /// ChannelAccount("user1") } );
        /// await connect.Conversations.OnSendToConversationAsync(resource.Id, new
        /// Activity() ... ) ;
        ///
        /// end. 
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='parameters'>Parameters to create the conversation from.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a conversation resource response.</returns>
        protected virtual Task<ConversationResourceResponse> OnCreateConversationAsync(ClaimsIdentity claimsIdentity, ConversationParameters parameters, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// OnGetConversationsAsync() API for Skill.
        /// </summary>
        /// <remarks>
        /// Override this method to list the Conversations in which this bot has participated.
        /// 
        /// GET from this method with a skip token
        /// 
        /// The return value is a ConversationsResult, which contains an array of
        /// ConversationMembers and a skip token.  If the skip token is not empty, then
        /// there are further values to be returned. Call this method again with the
        /// returned token to get more values.
        /// 
        /// Each ConversationMembers object contains the ID of the conversation and an
        /// array of ChannelAccounts that describe the members of the conversation.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>conversationId.</param> 
        /// <param name='continuationToken'>skip or continuation token.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for ConversationsResult.</returns>
        protected virtual Task<ConversationsResult> OnGetConversationsAsync(ClaimsIdentity claimsIdentity, string conversationId, string continuationToken = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetConversationMembers() API for Skill.
        /// </summary>
        /// <remarks>
        /// Override this method to enumerate the members of a conversation.
        ///
        /// This REST API takes a ConversationId and returns an array of ChannelAccount
        /// objects representing the members of the conversation.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a response.</returns>
        protected virtual Task<IList<ChannelAccount>> OnGetConversationMembersAsync(ClaimsIdentity claimsIdentity, string conversationId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetConversationPagedMembers() API for Skill.
        /// </summary>
        /// <remarks>
        /// Override this method to enumerate the members of a conversation one page at a time.
        ///
        /// This REST API takes a ConversationId. Optionally a pageSize and/or
        /// continuationToken can be provided. It returns a PagedMembersResult, which
        /// contains an array
        /// of ChannelAccounts representing the members of the conversation and a
        /// continuation token that can be used to get more values.
        ///
        /// One page of ChannelAccounts records are returned with each call. The number
        /// of records in a page may vary between channels and calls. The pageSize
        /// parameter can be used as
        /// a suggestion. If there are no additional results the response will not
        /// contain a continuation token. If there are no members in the conversation
        /// the Members will be empty or not present in the response.
        ///
        /// A response to a request that has a continuation token from a prior request
        /// may rarely return members from a previous request.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='pageSize'>Suggested page size.</param>
        /// <param name='continuationToken'>Continuation Token.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a response.</returns>
        protected virtual Task<PagedMembersResult> OnGetConversationPagedMembersAsync(ClaimsIdentity claimsIdentity, string conversationId, int? pageSize = default, string continuationToken = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// DeleteConversationMember() API for Skill.
        /// </summary>
        /// <remarks>
        /// Override this method to deletes a member from a conversation.
        ///
        /// This REST API takes a ConversationId and a memberId (of type string) and
        /// removes that member from the conversation. If that member was the last
        /// member
        /// of the conversation, the conversation will also be deleted.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='memberId'>ID of the member to delete from this conversation.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task.</returns>
        protected virtual Task OnDeleteConversationMemberAsync(ClaimsIdentity claimsIdentity, string conversationId, string memberId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// SendConversationHistory() API for Skill.
        /// </summary>
        /// <remarks>
        /// Override this method to this method allows you to upload the historic activities to the conversation.
        ///
        /// Sender must ensure that the historic activities have unique ids and
        /// appropriate timestamps. The ids are used by the client to deal with
        /// duplicate activities and the timestamps are used by the client to render
        /// the activities in the right order.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='transcript'>Transcript of activities.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        protected virtual Task<ResourceResponse> OnSendConversationHistoryAsync(ClaimsIdentity claimsIdentity, string conversationId, Transcript transcript, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// UploadAttachment() API.
        /// </summary>
        /// <remarks>
        /// 
        /// Override this method to store data in a compliant store when dealing with enterprises.
        /// 
        /// The response is a ResourceResponse which contains an AttachmentId which is
        /// suitable for using with the attachments API.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='attachmentUpload'>Attachment data.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task with result.</returns>
        protected virtual Task<ResourceResponse> OnUploadAttachmentAsync(ClaimsIdentity claimsIdentity, string conversationId, AttachmentData attachmentUpload, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
