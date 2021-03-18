// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// A base class for a skill controller.
    /// </summary>
    // Note: this class is marked as abstract to prevent the ASP runtime from registering it as a controller.
    [ChannelServiceExceptionFilter]
    public abstract class ChannelServiceController : ControllerBase
    {
        private readonly ChannelServiceHandlerBase _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelServiceController"/> class.
        /// </summary>
        /// <param name="handler">A <see cref="ChannelServiceHandler"/> that will handle the incoming request.</param>
        protected ChannelServiceController(ChannelServiceHandlerBase handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// SendToConversation.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activity">Activity to send.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("v3/conversations/{conversationId}/activities")]
        public virtual async Task<IActionResult> SendToConversationAsync(string conversationId, [FromBody] Activity activity)
        {
            var result = await _handler.HandleSendToConversationAsync(HttpContext.Request.Headers["Authorization"], conversationId, activity).ConfigureAwait(false);
            return new JsonResult(result, HttpHelper.BotMessageSerializerSettings);
        }

        /// <summary>
        /// ReplyToActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId the reply is to (OPTIONAL).</param>
        /// <param name="activity">Activity to send.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("v3/conversations/{conversationId}/activities/{activityId}")]
        public virtual async Task<IActionResult> ReplyToActivityAsync(string conversationId, string activityId, [FromBody] Activity activity)
        {
            var result = await _handler.HandleReplyToActivityAsync(HttpContext.Request.Headers["Authorization"], conversationId, activityId, activity).ConfigureAwait(false);
            return new JsonResult(result, HttpHelper.BotMessageSerializerSettings);
        }

        /// <summary>
        /// UpdateActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId to update.</param>
        /// <param name="activity">replacement Activity.</param>
        /// <returns>TODO Document.</returns>
        [HttpPut("v3/conversations/{conversationId}/activities/{activityId}")]
        public virtual async Task<IActionResult> UpdateActivityAsync(string conversationId, string activityId, [FromBody] Activity activity)
        {
            var result = await _handler.HandleUpdateActivityAsync(HttpContext.Request.Headers["Authorization"], conversationId, activityId, activity).ConfigureAwait(false);
            return new JsonResult(result, HttpHelper.BotMessageSerializerSettings);
        }

        /// <summary>
        /// DeleteActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId to delete.</param>
        /// <returns>TODO Document.</returns>
        [HttpDelete("v3/conversations/{conversationId}/activities/{activityId}")]
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
        [HttpGet("v3/conversations/{conversationId}/activities/{activityId}/members")]
        public virtual async Task<IActionResult> GetActivityMembersAsync(string conversationId, string activityId)
        {
            var result = await _handler.HandleGetActivityMembersAsync(HttpContext.Request.Headers["Authorization"], conversationId, activityId).ConfigureAwait(false);
            return new JsonResult(result, HttpHelper.BotMessageSerializerSettings);
        }

        /// <summary>
        /// CreateConversation.
        /// </summary>
        /// <param name="parameters">Parameters to create the conversation from.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("v3/conversations")]
        public virtual async Task<IActionResult> CreateConversationAsync([FromBody] ConversationParameters parameters)
        {
            var result = await _handler.HandleCreateConversationAsync(HttpContext.Request.Headers["Authorization"], parameters).ConfigureAwait(false);
            return new JsonResult(result, HttpHelper.BotMessageSerializerSettings);
        }

        /// <summary>
        /// GetConversations.
        /// </summary>
        /// <param name="continuationToken">skip or continuation token.</param>
        /// <returns>TODO Document.</returns>
        [HttpGet("v3/conversations")]
        public virtual async Task<IActionResult> GetConversationsAsync(string continuationToken = null)
        {
            var result = await _handler.HandleGetConversationsAsync(HttpContext.Request.Headers["Authorization"], continuationToken).ConfigureAwait(false);
            return new JsonResult(result, HttpHelper.BotMessageSerializerSettings);
        }

        /// <summary>
        /// GetConversationMembers.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <returns>TODO Document.</returns>
        [HttpGet("v3/conversations/{conversationId}/members")]
        public virtual async Task<IActionResult> GetConversationMembersAsync(string conversationId)
        {
            var result = await _handler.HandleGetConversationMembersAsync(HttpContext.Request.Headers["Authorization"], conversationId).ConfigureAwait(false);
            return new JsonResult(result, HttpHelper.BotMessageSerializerSettings);
        }

        /// <summary>
        /// GetConversationPagedMembers.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="pageSize">Suggested page size.</param>
        /// <param name="continuationToken">Continuation Token.</param>
        /// <returns>TODO Document.</returns>
        [HttpGet("v3/conversations/{conversationId}/pagedmembers")]
        public virtual async Task<IActionResult> GetConversationPagedMembersAsync(string conversationId, int pageSize = -1, string continuationToken = null)
        {
            var result = await _handler.HandleGetConversationPagedMembersAsync(HttpContext.Request.Headers["Authorization"], conversationId, pageSize, continuationToken).ConfigureAwait(false);
            return new JsonResult(result, HttpHelper.BotMessageSerializerSettings);
        }

        /// <summary>
        /// DeleteConversationMember.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="memberId">ID of the member to delete from this conversation.</param>
        /// <returns>TODO Document.</returns>
        [HttpDelete("v3/conversations/{conversationId}/members/{memberId}")]
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
        [HttpPost("v3/conversations/{conversationId}/activities/history")]
        public virtual async Task<IActionResult> SendConversationHistoryAsync(string conversationId, [FromBody] Transcript history)
        {
            var result = await _handler.HandleSendConversationHistoryAsync(HttpContext.Request.Headers["Authorization"], conversationId, history).ConfigureAwait(false);
            return new JsonResult(result, HttpHelper.BotMessageSerializerSettings);
        }

        /// <summary>
        /// UploadAttachment.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="attachmentUpload">Attachment data.</param>
        /// <returns>TODO Document.</returns>
        [HttpPost("v3/conversations/{conversationId}/attachments")]
        public virtual async Task<IActionResult> UploadAttachmentAsync(string conversationId, [FromBody] AttachmentData attachmentUpload)
        {
            var result = await _handler.HandleUploadAttachmentAsync(HttpContext.Request.Headers["Authorization"], conversationId, attachmentUpload).ConfigureAwait(false);
            return new JsonResult(result, HttpHelper.BotMessageSerializerSettings);
        }
    }
}
