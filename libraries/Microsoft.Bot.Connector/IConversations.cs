// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Rest;

    /// <summary>
    /// Conversations operations.
    /// </summary>
    public partial interface IConversations
    {
        /// <summary>
        /// GetConversations.
        /// </summary>
        /// <remarks>
        /// List the Conversations in which this bot has participated.
        ///
        /// GET from this method with a skip token
        ///
        /// The return value is a ConversationsResult, which contains an array
        /// of ConversationMembers and a skip token.  If the skip token is not
        /// empty, then
        /// there are further values to be returned. Call this method again
        /// with the returned token to get more values.
        ///
        /// Each ConversationMembers object contains the ID of the conversation
        /// and an array of ChannelAccounts that describe the members of the
        /// conversation.
        /// </remarks>
        /// <param name='continuationToken'>
        /// skip or continuation token.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse<ConversationsResult>> GetConversationsWithHttpMessagesAsync(string continuationToken = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// CreateConversation.
        /// </summary>
        /// <remarks>
        /// Create a new Conversation.
        ///
        /// POST to this method with a
        /// * Bot being the bot creating the conversation
        /// * IsGroup set to true if this is not a direct message (default is
        /// false)
        /// * Array containing the members to include in the conversation
        ///
        /// The return value is a ResourceResponse which contains a
        /// conversation id which is suitable for use
        /// in the message payload and REST API uris.
        ///
        /// Most channels only support the semantics of bots initiating a
        /// direct message conversation.  An example of how to do that would
        /// be:
        ///
        /// ```
        /// var resource = await connector.conversations.CreateConversation(new
        /// ConversationParameters(){ Bot = bot, members = new ChannelAccount[]
        /// { new ChannelAccount("user1") } );
        /// await connect.Conversations.SendToConversationAsync(resource.Id,
        /// new Activity() ... ) ;
        ///
        /// ``` .
        /// </remarks>
        /// <param name='parameters'>
        /// Parameters to create the conversation from.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse<ConversationResourceResponse>> CreateConversationWithHttpMessagesAsync(ConversationParameters parameters, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// SendToConversation.
        /// </summary>
        /// <remarks>
        /// This method allows you to send an activity to the end of a
        /// conversation.
        ///
        /// This is slightly different from ReplyToActivity().
        /// * SendToConversation(conversationId) - will append the activity to
        /// the end of the conversation according to the timestamp or semantics
        /// of the channel.
        /// * ReplyToActivity(conversationId,ActivityId) - adds the activity as
        /// a reply to another activity, if the channel supports it. If the
        /// channel does not support nested replies, ReplyToActivity falls back
        /// to SendToConversation.
        ///
        /// Use ReplyToActivity when replying to a specific activity in the
        /// conversation.
        ///
        /// Use SendToConversation in all other cases.
        /// </remarks>
        /// <param name='conversationId'>
        /// Conversation ID.
        /// </param>
        /// <param name='activity'>
        /// Activity to send.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse<ResourceResponse>> SendToConversationWithHttpMessagesAsync(string conversationId, Activity activity, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// SendConversationHistory.
        /// </summary>
        /// <remarks>
        /// This method allows you to upload the historic activities to the
        /// conversation.
        ///
        /// Sender must ensure that the historic activities have unique ids and
        /// appropriate timestamps. The ids are used by the client to deal with
        /// duplicate activities and the timestamps are used by the client to
        /// render the activities in the right order.
        /// </remarks>
        /// <param name='conversationId'>
        /// Conversation ID.
        /// </param>
        /// <param name='transcript'>
        /// Transcript of activities.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse<ResourceResponse>> SendConversationHistoryWithHttpMessagesAsync(string conversationId, Transcript transcript, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// UpdateActivity.
        /// </summary>
        /// <remarks>
        /// Edit an existing activity.
        ///
        /// Some channels allow you to edit an existing activity to reflect the
        /// new state of a bot conversation.
        ///
        /// For example, you can remove buttons after someone has clicked
        /// "Approve" button.
        /// </remarks>
        /// <param name='conversationId'>
        /// Conversation ID.
        /// </param>
        /// <param name='activityId'>
        /// activityId to update.
        /// </param>
        /// <param name='activity'>
        /// replacement Activity.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse<ResourceResponse>> UpdateActivityWithHttpMessagesAsync(string conversationId, string activityId, Activity activity, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// ReplyToActivity.
        /// </summary>
        /// <remarks>
        /// This method allows you to reply to an activity.
        ///
        /// This is slightly different from SendToConversation().
        /// * SendToConversation(conversationId) - will append the activity to
        /// the end of the conversation according to the timestamp or semantics
        /// of the channel.
        /// * ReplyToActivity(conversationId,ActivityId) - adds the activity as
        /// a reply to another activity, if the channel supports it. If the
        /// channel does not support nested replies, ReplyToActivity falls back
        /// to SendToConversation.
        ///
        /// Use ReplyToActivity when replying to a specific activity in the
        /// conversation.
        ///
        /// Use SendToConversation in all other cases.
        /// </remarks>
        /// <param name='conversationId'>
        /// Conversation ID.
        /// </param>
        /// <param name='activityId'>
        /// activityId the reply is to (OPTIONAL).
        /// </param>
        /// <param name='activity'>
        /// Activity to send.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse<ResourceResponse>> ReplyToActivityWithHttpMessagesAsync(string conversationId, string activityId, Activity activity, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// DeleteActivity.
        /// </summary>
        /// <remarks>
        /// Delete an existing activity.
        ///
        /// Some channels allow you to delete an existing activity, and if
        /// successful this method will remove the specified activity.
        /// </remarks>
        /// <param name='conversationId'>
        /// Conversation ID.
        /// </param>
        /// <param name='activityId'>
        /// activityId to delete.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse> DeleteActivityWithHttpMessagesAsync(string conversationId, string activityId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// GetConversationMembers.
        /// </summary>
        /// <remarks>
        /// Enumerate the members of a conversation.
        ///
        /// This REST API takes a ConversationId and returns an array of
        /// ChannelAccount objects representing the members of the
        /// conversation.
        /// </remarks>
        /// <param name='conversationId'>
        /// Conversation ID.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse<IList<ChannelAccount>>> GetConversationMembersWithHttpMessagesAsync(string conversationId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// GetConversationPagedMembers.
        /// </summary>
        /// <remarks>
        /// Enumerate the members of a conversation one page at a time.
        ///
        /// This REST API takes a ConversationId. Optionally a pageSize and/or
        /// continuationToken can be provided. It returns a PagedMembersResult,
        /// which contains an array
        /// of ChannelAccounts representing the members of the conversation and
        /// a continuation token that can be used to get more values.
        ///
        /// One page of ChannelAccounts records are returned with each call.
        /// The number of records in a page may vary between channels and
        /// calls. The pageSize parameter can be used as
        /// a suggestion. If there are no additional results the response will
        /// not contain a continuation token. If there are no members in the
        /// conversation the Members will be empty or not present in the
        /// response.
        ///
        /// A response to a request that has a continuation token from a prior
        /// request may rarely return members from a previous request.
        /// </remarks>
        /// <param name='conversationId'>
        /// Conversation ID.
        /// </param>
        /// <param name='pageSize'>
        /// Suggested page size.
        /// </param>
        /// <param name='continuationToken'>
        /// Continuation Token.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse<PagedMembersResult>> GetConversationPagedMembersWithHttpMessagesAsync(string conversationId, int? pageSize = default(int?), string continuationToken = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// DeleteConversationMember.
        /// </summary>
        /// <remarks>
        /// Deletes a member from a conversation.
        ///
        /// This REST API takes a ConversationId and a memberId (of type
        /// string) and removes that member from the conversation. If that
        /// member was the last member
        /// of the conversation, the conversation will also be deleted.
        /// </remarks>
        /// <param name='conversationId'>
        /// Conversation ID.
        /// </param>
        /// <param name='memberId'>
        /// ID of the member to delete from this conversation.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse> DeleteConversationMemberWithHttpMessagesAsync(string conversationId, string memberId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// GetActivityMembers.
        /// </summary>
        /// <remarks>
        /// Enumerate the members of an activity.
        ///
        /// This REST API takes a ConversationId and a ActivityId, returning an
        /// array of ChannelAccount objects representing the members of the
        /// particular activity in the conversation.
        /// </remarks>
        /// <param name='conversationId'>
        /// Conversation ID.
        /// </param>
        /// <param name='activityId'>
        /// Activity ID.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse<IList<ChannelAccount>>> GetActivityMembersWithHttpMessagesAsync(string conversationId, string activityId, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// UploadAttachment.
        /// </summary>
        /// <remarks>
        /// Upload an attachment directly into a channel's blob storage.
        ///
        /// This is useful because it allows you to store data in a compliant
        /// store when dealing with enterprises.
        ///
        /// The response is a ResourceResponse which contains an AttachmentId
        /// which is suitable for using with the attachments API.
        /// </remarks>
        /// <param name='conversationId'>
        /// Conversation ID.
        /// </param>
        /// <param name='attachmentUpload'>
        /// Attachment data.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="ErrorResponseException">
        /// Thrown when the operation returned an invalid status code.
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response.
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null.
        /// </exception>
        /// <returns>A task that represents the <see cref="HttpOperationResponse"/>.</returns>
        Task<HttpOperationResponse<ResourceResponse>> UploadAttachmentWithHttpMessagesAsync(string conversationId, AttachmentData attachmentUpload, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
