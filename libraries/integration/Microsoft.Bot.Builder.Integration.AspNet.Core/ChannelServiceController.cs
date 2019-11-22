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
    [ChannelServiceExceptionFilter]
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
        public virtual async Task<ActionResult<ChannelAccount[]>> GetActivityMembersAsync(string conversationId, string activityId)
        {
            return await _handler.HandleGetActivityMembersAsync(HttpContext.Request.Headers["Authorization"], conversationId, activityId).ConfigureAwait(false);
        }

        /// <summary>
        /// CreateConversation.
        /// </summary>
        /// <param name="parameters">Parameters to create the conversation from.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost]
        public virtual async Task<ActionResult<ConversationResourceResponse>> CreateConversationAsync([FromBody] ConversationParameters parameters)
        {
            return await _handler.HandleCreateConversationAsync(HttpContext.Request.Headers["Authorization"], parameters).ConfigureAwait(false);
        }

        /// <summary>
        /// GetConversations.
        /// </summary>
        /// <param name="continuationToken">skip or continuation token.</param>
        /// <returns>TODO Document.</returns>
        [HttpGet]
        public virtual async Task<ActionResult<ConversationsResult>> GetConversationsAsync(string continuationToken = null)
        {
            return await _handler.HandleGetConversationsAsync(HttpContext.Request.Headers["Authorization"], continuationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// GetConversationMembers.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <returns>TODO Document.</returns>
        [HttpGet("{conversationId}/members")]
        public virtual async Task<ActionResult<IList<ChannelAccount>>> GetConversationMembersAsync(string conversationId)
        {
            var result = await _handler.HandleGetConversationMembersAsync(HttpContext.Request.Headers["Authorization"], conversationId).ConfigureAwait(false);
            return new ActionResult<IList<ChannelAccount>>(result);
        }

        /// <summary>
        /// GetConversationPagedMembers.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="pageSize">Suggested page size.</param>
        /// <param name="continuationToken">Continuation Token.</param>
        /// <returns>TODO Document.</returns>
        [HttpGet("{conversationId}/pagedmembers")]
        public virtual async Task<PagedMembersResult> GetConversationPagedMembersAsync(string conversationId, int pageSize = -1, string continuationToken = null)
        {
            return await _handler.HandleGetConversationPagedMembersAsync(HttpContext.Request.Headers["Authorization"], conversationId, pageSize, continuationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// DeleteConversationMember.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="memberId">ID of the member to delete from this conversation.</param>
        /// <returns>TODO Document.</returns>
        [HttpDelete("{conversationId}/members/{memberId}")]
        public virtual async Task DeleteConversationMemberAsync(string conversationId, string memberId)
        {
            await _handler.HandleDeleteConversationMemberAsync(HttpContext.Request.Headers["Authorization"], conversationId, memberId).ConfigureAwait(false);
        }

        /// <summary>
        /// SendConversationHistory.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="history">Historic activities.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("{conversationId}/activities/history")]
        public virtual async Task<ActionResult<ResourceResponse>> SendConversationHistoryAsync(string conversationId, [FromBody] Transcript history)
        {
            return await _handler.HandleSendConversationHistoryAsync(HttpContext.Request.Headers["Authorization"], conversationId, history).ConfigureAwait(false);
        }

        /// <summary>
        /// UploadAttachment.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="attachmentUpload">Attachment data.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("{conversationId}/attachments")]
        public virtual async Task<ActionResult<ResourceResponse>> UploadAttachmentAsync(string conversationId, [FromBody] AttachmentData attachmentUpload)
        {
            return await _handler.HandleUploadAttachmentAsync(HttpContext.Request.Headers["Authorization"], conversationId, attachmentUpload).ConfigureAwait(false);
        }
    }
}
