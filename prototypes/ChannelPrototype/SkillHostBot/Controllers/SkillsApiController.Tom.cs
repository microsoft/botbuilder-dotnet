// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace SkillHost.Controllers
{
    [ApiController]
    [Route("/v3/conversations")]
    public class SkillsApiController : ControllerBase
    {
        private readonly BotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;

        public SkillsApiController(BotFrameworkHttpAdapter adapter, IConfiguration configuration, IBot bot)
        {
            // adapter to use for calling back to channel
            _adapter = adapter;
            _bot = bot;
            BotAppId = configuration.GetValue<string>("MicrosoftAppId");
        }

        public string BotAppId { get; set; }

        /// <summary>
        /// CreateConversation.
        /// </summary>
        /// <param name="parameters">Parameters to create the conversation from.</param>
        /// <returns>TODO Document this.</returns>
        [HttpPost]
        [Route("/v3/conversations")]
        public virtual Task<ConversationResourceResponse> CreateConversation([FromBody] ConversationParameters parameters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// SendToConversation.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activity">Activity to send.</param>
        /// <returns>TODO Document this.</returns>
        [HttpPost]
        [Route("/v3/conversations/{conversationId}/activities")]
        public virtual Task<ResourceResponse> SendToConversation(string conversationId, [FromBody] Activity activity)
        {
            return InvokeChannelAPI<ResourceResponse>(ChannelApiMethod.SendToConversation, activity.Conversation.Id, activity);
        }

        /// <summary>
        /// SendConversationHistory.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="history">Historic activities.</param>
        /// <returns>TODO Document this.</returns>
        [HttpPost]
        [Route("/v3/conversations/{conversationId}/activities/history")]
        public virtual Task<ResourceResponse> SendConversationHistory(string conversationId, [FromBody] Transcript history)
        {
            return InvokeChannelAPI<ResourceResponse>(ChannelApiMethod.SendConversationHistory, conversationId, history);
        }

        /// <summary>
        /// ReplyToActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId the reply is to (OPTIONAL).</param>
        /// <param name="activity">Activity to send.</param>
        /// <returns>TODO Document this.</returns>
        [HttpPost]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
        public virtual Task<ResourceResponse> ReplyToActivity(string conversationId, string activityId, [FromBody] Activity activity)
        {
            return InvokeChannelAPI<ResourceResponse>(ChannelApiMethod.ReplyToActivity, activity.Conversation.Id, activityId, activity);
        }

        /// <summary>
        /// UpdateActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId to update.</param>
        /// <param name="activity">replacement Activity.</param>
        /// <returns>TODO Document this.</returns>
        [HttpPut]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
        public virtual Task<ResourceResponse> UpdateActivity(string conversationId, string activityId, [FromBody] Activity activity)
        {
            return InvokeChannelAPI<ResourceResponse>(ChannelApiMethod.UpdateActivity, activity.Conversation.Id, activity);
        }

        /// <summary>
        /// DeleteActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId to delete.</param>
        /// <returns>TODO Document this.</returns>
        [HttpDelete]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
        public virtual Task DeleteActivity(string conversationId, string activityId)
        {
            return InvokeChannelApi(ChannelApiMethod.DeleteActivity, conversationId, activityId);
        }

        /// <summary>
        /// GetConversations.
        /// </summary>
        /// <param name="continuationToken">skip or continuation token.</param>
        /// <returns>TODO Document this.</returns>
        [HttpGet]
        [Route("/v3/conversations")]
        public virtual Task<ConversationsResult> GetConversations(string continuationToken = null)
        {
            Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            return Task.FromResult<ConversationsResult>(null);
        }

        /// <summary>
        /// GetConversationMembers.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <returns>TODO Document this.</returns>
        [HttpGet]
        [Route("/v3/conversations/{conversationId}/members")]
        public virtual Task<ChannelAccount[]> GetConversationMembers(string conversationId)
        {
            return InvokeChannelAPI<ChannelAccount[]>(ChannelApiMethod.GetConversationMembers, conversationId);
        }

        /// <summary>
        /// GetConversationPagedMembers.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="pageSize">Suggested page size.</param>
        /// <param name="continuationToken">Continuation Token.</param>
        /// <returns>TODO Document this.</returns>
        [HttpGet]
        [Route("/v3/conversations/{conversationId}/pagedmembers")]
        public virtual Task<PagedMembersResult> GetConversationPagedMembers(string conversationId, int pageSize = -1, string continuationToken = null)
        {
            return InvokeChannelAPI<PagedMembersResult>(ChannelApiMethod.GetConversationPagedMembers, conversationId, pageSize, continuationToken);
        }

        /// <summary>
        /// DeleteConversationMember.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="memberId">ID of the member to delete from this conversation.</param>
        /// <returns>TODO Document this.</returns>
        [HttpDelete]
        [Route("/v3/conversations/{conversationId}/members/{memberId}")]
        public virtual Task DeleteConversationMember(string conversationId, string memberId)
        {
            return InvokeChannelApi(ChannelApiMethod.DeleteConversationMember, conversationId, memberId);
        }

        /// <summary>
        /// GetActivityMembers.
        /// </summary>
        /// <remarks>
        /// Markdown=Content\Methods\GetActivityMembers.md.
        /// </remarks>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">Activity ID.</param>
        /// <returns>TODO Document this.</returns>
        [HttpGet]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}/members")]
        public virtual Task<ChannelAccount[]> GetActivityMembers(string conversationId, string activityId)
        {
            return InvokeChannelAPI<ChannelAccount[]>(ChannelApiMethod.GetActivityMembers, conversationId, activityId);
        }

        /// <summary>
        /// UploadAttachment.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="attachmentUpload">Attachment data.</param>
        /// <returns>TODO Document this.</returns>
        [HttpPost]
        [Route("/v3/conversations/{conversationId}/attachments")]
        public virtual Task<ResourceResponse> UploadAttachment(string conversationId, [FromBody] AttachmentData attachmentUpload)
        {
            return InvokeChannelAPI<ResourceResponse>(ChannelApiMethod.UploadAttachment, conversationId, attachmentUpload);
        }

        private async Task<TResponse> InvokeChannelAPI<TResponse>(ChannelApiMethod method, string conversationId, params object[] args)
        {
            var skillConversation = new SkillConversation(conversationId);

            var channelApiInvokeActivity = Activity.CreateInvokeActivity();
            channelApiInvokeActivity.Name = "ChannelAPI";
            channelApiInvokeActivity.ChannelId = "unknown";
            channelApiInvokeActivity.ServiceUrl = skillConversation.ServiceUrl;
            channelApiInvokeActivity.Conversation = new ConversationAccount(id: skillConversation.ConversationId);
            channelApiInvokeActivity.From = new ChannelAccount(id: "unknown");
            channelApiInvokeActivity.Recipient = new ChannelAccount(id: "unknown", role: RoleTypes.Bot);

            var activityPayload = args?.Where(arg => arg is Activity).Cast<Activity>().FirstOrDefault();
            if (activityPayload != null)
            {
                // fix up activityPayload with original conversation.Id and id
                activityPayload.Conversation.Id = skillConversation.ConversationId;
                activityPayload.ServiceUrl = skillConversation.ServiceUrl;

                // use the activityPayload for channel accounts, it will be in From=Bot/Skill Recipient=User, 
                // We want to send it to the bot as From=User, Recipient=Bot so we have correct state context.
                channelApiInvokeActivity.ChannelId = activityPayload.ChannelId;
                channelApiInvokeActivity.From = activityPayload.Recipient;
                channelApiInvokeActivity.Recipient = activityPayload.From;

                // We want ActivityPayload to also be in User->Bot context, if it is outbound it will go through context.SendActivity which will flip outgoing to Bot->User
                // regardless this gives us same memory context of User->Bot which is useful for things like EndOfConversation processing being in the correct memory context.
                activityPayload.From = channelApiInvokeActivity.From;
                activityPayload.Recipient = channelApiInvokeActivity.Recipient;
            }

            var channelApiArgs = new ChannelApiArgs()
            {
                Method = method,
                Args = args,
            };
            channelApiInvokeActivity.Value = channelApiArgs;

            // We call our adapter using the BotAppId claim, so turnContext has the bot claims
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                // Adding claims for both Emulator and Channel.
                new Claim(AuthenticationConstants.AudienceClaim, BotAppId),
                new Claim(AuthenticationConstants.AppIdClaim, BotAppId),
                new Claim(AuthenticationConstants.ServiceUrlClaim, skillConversation.ServiceUrl),
            });

            // send up to the bot to process it...
            await _adapter.ProcessActivityAsync(claimsIdentity, (Activity)channelApiInvokeActivity, _bot.OnTurnAsync, CancellationToken.None);

            return (TResponse)channelApiArgs.Result;
        }

        private async Task InvokeChannelApi(ChannelApiMethod method, string conversationId, params object[] args)
        {
            await InvokeChannelAPI<object>(method, conversationId, args);
        }
    }
}
