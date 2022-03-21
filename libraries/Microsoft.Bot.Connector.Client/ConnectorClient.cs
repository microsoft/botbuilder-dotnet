// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Client.Models;

namespace Microsoft.Bot.Connector.Client
{
    /// <summary>
    /// The Bot Connector REST API allows your bot to send and receive messages to channels configured in the [Bot Framework Developer Portal](https://dev.botframework.com).
    /// The Connector service uses industry-standard REST and JSON over HTTPS.
    ///
    /// Client libraries for this REST API are available. See below for a list.
    ///
    /// Many bots will use both the Bot Connector REST API and the associated [Bot State REST API](/en-us/restapi/state).
    /// The Bot State REST API allows a bot to store and retrieve state associated with users and conversations.
    ///
    /// Authentication for both the Bot Connector and Bot State REST APIs is accomplished with JWT Bearer tokens,
    /// and is described in detail in the [Connector Authentication](/en-us/restapi/authentication) document.
    ///
    /// # Client Libraries for the Bot Connector REST API
    /// * [Bot Builder for C#](/en-us/csharp/builder/sdkreference/).
    /// * [Bot Builder for Node.js](/en-us/node/builder/overview/).
    /// * Generate your own from the [Connector API Swagger file](https://raw.githubusercontent.com/Microsoft/BotBuilder/master/CSharp/Library/Microsoft.Bot.Connector.Shared/Swagger/ConnectorAPI.json).
    /// </summary>
    public abstract class ConnectorClient
    {
        /// <summary>
        /// List the Conversations in which this bot has participated.
        /// 
        /// GET from this method with a skip token.
        /// 
        /// The return value is a ConversationsResult, which contains an array of ConversationMembers and a skip token.  If the skip token is not empty, then
        /// there are further values to be returned. Call this method again with the returned token to get more values.
        /// 
        /// Each ConversationMembers object contains the ID of the conversation and an array of ChannelAccounts that describe the members of the conversation.
        /// </summary>
        /// <param name="continuationToken">The skip or continuation token.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>The conversations in which this bot has participated.</returns>
        public abstract Task<ConversationsResult> GetConversationsAsync(string continuationToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new Conversation.
        /// 
        /// POST to this method with a
        /// * Bot being the bot creating the conversation
        /// * IsGroup set to true if this is not a direct message (default is false)
        /// * Array containing the members to include in the conversation
        /// 
        /// The return value is a ResourceResponse which contains a conversation id which is suitable for use in the message payload and REST API uris.
        /// 
        /// Most channels only support the semantics of bots initiating a direct message conversation.  An example of how to do that would be:
        /// ```
        /// var resource = await connector.conversations.CreateConversation(new ConversationParameters(){ Bot = bot, members = new ChannelAccount[] { new ChannelAccount(&quot;user1&quot;) } );
        /// await connect.Conversations.SendToConversationAsync(resource.Id, new Activity() ... ) ;
        /// ```.
        /// </summary>
        /// <param name="parameters"> Parameters to create the conversation from. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        /// <returns>The <see cref="ConversationResourceResponse"/>.</returns>
        public abstract Task<ConversationResourceResponse> CreateConversationAsync(ConversationParameters parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// This method allows you to send an activity to the end of a conversation.
        /// 
        /// This is slightly different from ReplyToActivity().
        /// * SendToConversation(conversationId) - will append the activity to the end of the conversation according to the timestamp or semantics of the channel.
        /// * ReplyToActivity(conversationId,ActivityId) - adds the activity as a reply to another activity, if the channel supports it. If the channel does not support nested replies, ReplyToActivity falls back to SendToConversation.
        /// 
        /// Use ReplyToActivity when replying to a specific activity in the conversation.
        /// Use SendToConversation in all other cases.
        /// </summary>
        /// <param name="activity">The activity to send.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>The <see cref="ResourceResponse"/>.</returns>
        public abstract Task<ResourceResponse> SendToConversationAsync(Activity activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// This method allows you to upload the historic activities to the conversation.
        /// 
        /// Sender must ensure that the historic activities have unique ids and appropriate timestamps.
        /// The ids are used by the client to deal with duplicate activities and the timestamps are used by the client to render the activities in the right order.
        /// </summary>
        /// <param name="conversationId">The conversation ID.</param>
        /// <param name="history">Historic activities.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>The <see cref="ResourceResponse"/>.</returns>
        public abstract Task<ResourceResponse> SendConversationHistoryAsync(string conversationId, Transcript history, CancellationToken cancellationToken = default);

        /// <summary>
        /// Edit an existing activity.
        /// 
        /// Some channels allow you to edit an existing activity to reflect the new state of a bot conversation.
        /// For example, you can remove buttons after someone has clicked &quot;Approve&quot; button.
        /// </summary>
        /// <param name="activity">The replacement Activity.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>The <see cref="ResourceResponse"/>.</returns>
        public abstract Task<ResourceResponse> UpdateActivityAsync(Activity activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// This method allows you to reply to an activity.
        /// This is slightly different from SendToConversation().
        /// 
        /// * SendToConversation(conversationId) - will append the activity to the end of the conversation according to the timestamp or semantics of the channel.
        /// * ReplyToActivity(conversationId,ActivityId) - adds the activity as a reply to another activity, if the channel supports it. If the channel does not support nested replies, ReplyToActivity falls back to SendToConversation.
        /// 
        /// Use ReplyToActivity when replying to a specific activity in the conversation.
        /// Use SendToConversation in all other cases.
        /// </summary>
        /// <param name="activity">The activity to send.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>The <see cref="ResourceResponse"/>.</returns>
        public abstract Task<ResourceResponse> ReplyToActivityAsync(Activity activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete an existing activity.
        /// Some channels allow you to delete an existing activity, and if successful this method will remove the specified activity.
        /// </summary>
        /// <param name="conversationId">The conversation ID.</param>
        /// <param name="activityId">The activityId to delete.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A Task representing the delete operation.</returns>
        public abstract Task DeleteActivityAsync(string conversationId, string activityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Enumerate the members of a conversation.
        /// This REST API takes a ConversationId and returns an array of ChannelAccount objects representing the members of the conversation.
        /// </summary>
        /// <param name="conversationId">The conversation ID.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A collection of members of a conversation.</returns>
        public abstract Task<IReadOnlyList<ChannelAccount>> GetConversationMembersAsync(string conversationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a single member of a conversation.
        /// This REST API takes a ConversationId and MemberId and returns a single ChannelAccount object, if that member is found in this conversation.
        /// </summary>
        /// <param name="conversationId">The conversation ID.</param>
        /// <param name="memberId">The member ID to look up in the conversation.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>The conversation member.</returns>
        public abstract Task<ChannelAccount> GetConversationMemberAsync(string conversationId, string memberId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a member from a conversation.
        /// This REST API takes a ConversationId and a memberId (of type string) and removes that member from the conversation.
        /// If that member was the last member of the conversation, the conversation will also be deleted.
        /// </summary>
        /// <param name="conversationId">The conversation ID.</param>
        /// <param name="memberId">The ID of the member to delete from this conversation.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A task representing the asynchronous operation of deleting conversation member.</returns>
        public abstract Task DeleteConversationMemberAsync(string conversationId, string memberId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Enumerate the members of a conversation one page at a time.
        /// This REST API takes a ConversationId. Optionally a pageSize and/or continuationToken can be provided.
        /// It returns a PagedMembersResult, which contains an array of ChannelAccounts representing the members of the conversation and a continuation token that can be used to get more values.
        /// One page of ChannelAccounts records are returned with each call. The number of records in a page may vary between channels and calls. The pageSize parameter can be used as a suggestion.
        /// If there are no additional results, the response will not contain a continuation token.
        /// If there are no members in the conversation, the Members will be empty or not present in the response.
        /// A response to a request that has a continuation token from a prior request may rarely return members from a previous request.
        /// </summary>
        /// <param name="conversationId">The conversation ID.</param>
        /// <param name="pageSize">Suggested page size.</param>
        /// <param name="continuationToken">Continuation Token.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>The <see cref="PagedMembersResult"/>.</returns>
        public abstract Task<PagedMembersResult> GetConversationPagedMembersAsync(string conversationId, int? pageSize = null, string continuationToken = "", CancellationToken cancellationToken = default);

        /// <summary>
        /// Enumerate the members of an activity.
        /// This REST API takes a ConversationId and an ActivityId, returning an array of ChannelAccount objects representing the members of the particular activity in the conversation.
        /// </summary>
        /// <param name="conversationId">The conversation ID.</param>
        /// <param name="activityId">The activity ID.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>A collection of members of an activity.</returns>
        public abstract Task<IReadOnlyList<ChannelAccount>> GetActivityMembersAsync(string conversationId, string activityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Upload an attachment directly into a channel's blob storage.
        /// This is useful because it allows you to store data in a compliant store when dealing with enterprises.
        /// The response is a ResourceResponse which contains an AttachmentId which is suitable for using with the attachments API.
        /// </summary>
        /// <param name="conversationId">The conversation ID.</param>
        /// <param name="attachmentUpload">The attachment data to upload.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>The <see cref="ResourceResponse"/>.</returns>
        public abstract Task<ResourceResponse> UploadAttachmentAsync(string conversationId, AttachmentData attachmentUpload, CancellationToken cancellationToken = default);

        /// <summary>Get AttachmentInfo structure describing the attachment views.</summary>
        /// <param name="attachmentId">The attachment id.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>The <see cref="AttachmentInfo"/>.</returns>
        public abstract Task<AttachmentInfo> GetAttachmentInfoAsync(string attachmentId, CancellationToken cancellationToken = default);

        /// <summary>Get the named view as binary content.</summary>
        /// <param name="attachmentId">The attachment id.</param>
        /// <param name="viewId">The view id from AttachmentInfo.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>The attachment as a Stream.</returns>
        public abstract Task<Stream> GetAttachmentAsync(string attachmentId, string viewId, CancellationToken cancellationToken = default);
    }
}
