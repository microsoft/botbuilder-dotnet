// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// A base class for a skill controller.
    /// </summary>
    public class ChannelServiceController : ControllerBase
    {
        private readonly ChannelServiceHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelServiceController"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="handler">A <see cref="ChannelServiceHandler"/> that will handle the incoming request.</param>
        /// <exception cref="ArgumentNullException">throw ArgumentNullException.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the Use(<see cref="IMiddleware"/>) method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public ChannelServiceController(ChannelServiceHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        /// <summary>
        /// SendToConversation.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activity">Activity to send.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("{conversationId}/activities")]
        public virtual async Task<ActionResult<ResourceResponse>> SendToConversationAsync(string conversationId, [FromBody] Activity activity)
        {
            return await _handler.HandleSendToConversationAsync(HttpContext.Request.Headers["Authorization"], conversationId, activity).ConfigureAwait(false);
        }

        /// <summary>
        /// ReplyToActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId the reply is to (OPTIONAL).</param>
        /// <param name="activity">Activity to send.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("{conversationId}/activities/{activityId}")]
        public virtual async Task<ActionResult<ResourceResponse>> ReplyToActivityAsync(string conversationId, string activityId, [FromBody] Activity activity)
        {
            return await _handler.HandleReplyToActivityAsync(HttpContext.Request.Headers["Authorization"], conversationId, activityId, activity).ConfigureAwait(false);
        }

        /// <summary>
        /// UpdateActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId to update.</param>
        /// <param name="activity">replacement Activity.</param>
        /// <returns>TODO Document.</returns>
        [HttpPut("{conversationId}/activities/{activityId}")]
        public virtual async Task<ActionResult<ResourceResponse>> UpdateActivityAsync(string conversationId, string activityId, [FromBody] Activity activity)
        {
            return await _handler.HandleUpdateActivityAsync(HttpContext.Request.Headers["Authorization"], conversationId, activityId, activity).ConfigureAwait(false);
        }

        /// <summary>
        /// DeleteActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId to delete.</param>
        /// <returns>TODO Document.</returns>
        [HttpDelete("{conversationId}/activities/{activityId}")]
        public virtual async Task DeleteActivityAsync(string conversationId, string activityId)
        {
            await _handler.HandleDeleteActivityAsync(HttpContext.Request.Headers["Authorization"], conversationId, activityId).ConfigureAwait(false);
        }

        /// <summary>
        /// GetActivityMembers.
        /// </summary>
        /// <remarks>
        /// Markdown=Content\Methods\GetActivityMembers.md.
        /// </remarks>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">Activity ID.</param>
        /// <returns>TODO Document.</returns>
        [HttpGet("{conversationId}/activities/{activityId}/members")]
        public virtual Task<ActionResult<ChannelAccount[]>> GetActivityMembersAsync(string conversationId, string activityId)
        {
            throw new NotSupportedException("GetActivityMembersAsync is not supported");
        }

        /// <summary>
        /// CreateConversation.
        /// </summary>
        /// <param name="parameters">Parameters to create the conversation from.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost]
        public virtual Task<ActionResult<ConversationResourceResponse>> CreateConversationAsync([FromBody] ConversationParameters parameters)
        {
            throw new NotSupportedException("CreateConversationAsync is not supported");
        }

        /// <summary>
        /// GetConversations.
        /// </summary>
        /// <param name="continuationToken">skip or continuation token.</param>
        /// <returns>TODO Document.</returns>
        [HttpGet]
        public virtual Task<ActionResult<ConversationsResult>> GetConversationsAsync(string continuationToken = null)
        {
            throw new NotSupportedException("GetConversationsAsync is not supported");
        }

        /// <summary>
        /// GetConversationMembers.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <returns>TODO Document.</returns>
        [HttpGet("{conversationId}/members")]
        public virtual Task<ActionResult<IList<ChannelAccount>>> GetConversationMembersAsync(string conversationId)
        {
            throw new NotSupportedException("GetConversationMembersAsync is not supported");
        }

        /// <summary>
        /// GetConversationPagedMembers.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="pageSize">Suggested page size.</param>
        /// <param name="continuationToken">Continuation Token.</param>
        /// <returns>TODO Document.</returns>
        [HttpGet("{conversationId}/pagedmembers")]
        public virtual Task<PagedMembersResult> GetConversationPagedMembersAsync(string conversationId, int pageSize = -1, string continuationToken = null)
        {
            throw new NotSupportedException("GetConversationPagedMembersAsync is not supported");
        }

        /// <summary>
        /// DeleteConversationMember.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="memberId">ID of the member to delete from this conversation.</param>
        /// <returns>TODO Document.</returns>
        [HttpDelete("{conversationId}/members/{memberId}")]
        public virtual Task DeleteConversationMemberAsync(string conversationId, string memberId)
        {
            throw new NotSupportedException("DeleteConversationMemberAsync is not supported");
        }

        /// <summary>
        /// SendConversationHistory.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="history">Historic activities.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("{conversationId}/activities/history")]
        public virtual Task<ActionResult<ResourceResponse>> SendConversationHistoryAsync(string conversationId, [FromBody] Transcript history)
        {
            throw new NotSupportedException("SendConversationHistoryAsync is not supported");
        }

        /// <summary>
        /// UploadAttachment.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="attachmentUpload">Attachment data.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("{conversationId}/attachments")]
        public virtual Task<ActionResult<ResourceResponse>> UploadAttachmentAsync(string conversationId, [FromBody] AttachmentData attachmentUpload)
        {
            throw new NotSupportedException("UploadAttachmentAsync is not supported");
        }
    }
}
