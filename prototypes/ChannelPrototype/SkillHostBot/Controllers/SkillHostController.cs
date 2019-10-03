// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ChannelPrototype.Controllers
{
    [ApiController]
    [Route("/v3/conversations")]
    public class SkillHostController : ControllerBase
    {
        private readonly BotFrameworkAdapter adapter;
        private readonly IBot bot;
        private IConfiguration configuration;

        public SkillHostController(BotFrameworkHttpAdapter adapter, IConfiguration configuration, IBot bot)
        {
            // adapter to use for calling back to channel
            this.adapter = adapter;
            this.bot = bot;
            this.configuration = configuration;
            this.BotAppId = configuration.GetValue<string>("MicrosoftAppId");
        }

        public string BotAppId { get; set; }

        public static string GetSkillConversationId(IActivity activity)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new string[] { activity.ServiceUrl, activity.Conversation.Id })));
        }

        public static ConversationInfo GetConversationInfo(string skillConversatioId)
        {
            var parts = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Convert.FromBase64String(skillConversatioId)));
            return new ConversationInfo() 
            { 
                ServiceUrl = parts[0], 
                ConversationId = parts[1],
            };
        }

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
            return CallSkillApi<ResourceResponse>(SkillMethod.SendActivity, activity.Conversation.Id, activity);
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
            return CallSkillApi<ResourceResponse>(SkillMethod.SendConversationHistory, conversationId, history);
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
            return CallSkillApi<ResourceResponse>(SkillMethod.SendActivity, activity.Conversation.Id, activity);
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
            return CallSkillApi<ResourceResponse>(SkillMethod.UpdateActivity, activity.Conversation.Id, activity);
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
            return CallSkillApi(SkillMethod.DeleteActivity, conversationId, activityId);
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
            return CallSkillApi<ChannelAccount[]>(SkillMethod.GetConversationMembers, conversationId);
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
            return CallSkillApi<PagedMembersResult>(SkillMethod.GetConversationPagedMembers, conversationId, pageSize, continuationToken);
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
            return CallSkillApi(SkillMethod.DeleteConversationMember, conversationId, memberId);
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
            return CallSkillApi<ChannelAccount[]>(SkillMethod.GetActivityMembers, conversationId, activityId);
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
            return CallSkillApi<ResourceResponse>(SkillMethod.UploadAttachment, conversationId, attachmentUpload);
        }

        private async Task<TResponse> CallSkillApi<TResponse>(SkillMethod method, string conversationId, params object[] args)
        {
            var conversationInfo = GetConversationInfo(conversationId);

            if (args.Length > 0 && args[0] is Activity activity)
            {
                // fix up activity
                activity.Conversation.Id = conversationInfo.ConversationId;
                activity.ServiceUrl = conversationInfo.ServiceUrl;
            }

            var allArgs = new List<object>();
            allArgs.Add(conversationInfo.ConversationId);
            if (args != null && args.Length > 0)
            {
                allArgs.AddRange(args);
            }

            var skillArgs = new SkillArgs()
            {
                Method = method,
                Args = allArgs.ToArray(),
            };

            IEventActivity skillEvent = Activity.CreateEventActivity();
            skillEvent.Name = "Skill";
            skillEvent.ChannelId = "Skill";
            skillEvent.ServiceUrl = conversationInfo.ServiceUrl;
            skillEvent.Conversation = new ConversationAccount(id: conversationInfo.ConversationId);
            skillEvent.From = new ChannelAccount("Skill", role: "Skill");
            skillEvent.Recipient = new ChannelAccount(id: "Bot", role: RoleTypes.Bot);
            skillEvent.Value = skillArgs;

            // We call our adapter using the BotAppId claim, so turnContext has the bot claims
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                // Adding claims for both Emulator and Channel.
                new Claim(AuthenticationConstants.AudienceClaim, BotAppId),
                new Claim(AuthenticationConstants.AppIdClaim, BotAppId),
            });

            // send up to the bot
            await adapter.ProcessActivityAsync(claimsIdentity, (Activity)skillEvent, bot.OnTurnAsync, CancellationToken.None);

            return (TResponse)skillArgs.Result;
        }

        private async Task CallSkillApi(SkillMethod method, string conversationId, params object[] args)
        {
            await CallSkillApi<object>(method, conversationId, args);
            return;
        }

        public class ConversationInfo
        {
            public string ServiceUrl { get; set; }

            public string ConversationId { get; set; }
        }
    }
}
