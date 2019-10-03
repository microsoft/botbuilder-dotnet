// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

namespace ChannelPrototype.Controllers
{
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
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    [ApiController]
    [Route("/v3/conversations")]
    public class SkillHostController : ControllerBase
    {
        private BotFrameworkAdapter adapter;
        private IConfiguration configuration;
        private IBot bot;

        public SkillHostController(BotFrameworkHttpAdapter adapter, IConfiguration configuration, IBot bot)
        {
            // adapter to use for calling back to channel
            this.adapter = adapter;
            this.bot = bot;
            this.configuration = configuration;
            this.BotAppId = configuration.GetValue<string>("MicrosoftAppId");
        }

        public string BotAppId { get; set; }

        /// <summary>
        /// CreateConversation
        /// </summary>
        /// <param name="parameters">Parameters to create the conversation from</param>
        [HttpPost]
        [Route("/v3/conversations")]
        public virtual async Task<ConversationResourceResponse> CreateConversation([FromBody]ConversationParameters parameters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// SendToConversation
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="activity">Activity to send</param>
        [HttpPost]
        [Route("/v3/conversations/{conversationId}/activities")]
        public virtual async Task<ResourceResponse> SendToConversation(string conversationId, [FromBody]Activity activity)
        {
            return await ProcessActivityInBot(activity);
        }

        /// <summary>
        /// SendConversationHistory
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="history">Historic activities</param>
        [HttpPost]
        [Route("/v3/conversations/{conversationId}/activities/history")]
        public virtual async Task<ResourceResponse> SendConversationHistory(string conversationId, [FromBody]Transcript history)
        {
            Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            return null;
        }

        /// <summary>
        /// ReplyToActivity
        /// </summary>
        /// <param name="activity">Activity to send</param>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="activityId">activityId the reply is to (OPTIONAL)</param>
        [HttpPost]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
        public virtual async Task<ResourceResponse> ReplyToActivity(string conversationId, string activityId, [FromBody]Activity activity)
        {
            return await ProcessActivityInBot(activity);
        }

        /// <summary>
        /// UpdateActivity
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="activityId">activityId to update</param>
        /// <param name="activity">replacement Activity</param>
        [HttpPut]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
        public virtual async Task<ResourceResponse> UpdateActivity(string conversationId, string activityId, [FromBody]Activity activity)
        {
            var updateActivity = new Activity(type: ActivityTypes.MessageDelete, id: activityId, conversation: new ConversationAccount(id: conversationId));
            updateActivity.Value = activity;
            updateActivity.ApplyConversationReference(activity.GetConversationReference());
            return await ProcessActivityInBot(activity);
        }

        /// <summary>
        /// DeleteActivity
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="activityId">activityId to delete</param>
        [HttpDelete]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
        public virtual async Task DeleteActivity(string conversationId, string activityId)
        {
            var activity = new Activity(type: ActivityTypes.MessageDelete, id: activityId, conversation: new ConversationAccount(id: conversationId));
            await ProcessActivityInBot(activity);
        }

        /// <summary>
        /// GetConversations
        /// </summary>
        /// <param name="continuationToken">skip or continuation token</param>
        [HttpGet]
        [Route("/v3/conversations")]
        public virtual async Task<ConversationsResult> GetConversations(string continuationToken = null)
        {
            Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            return null;
        }

        /// <summary>
        /// GetConversationMembers
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        [HttpGet]
        [Route("/v3/conversations/{conversationId}/members")]
        public virtual async Task<ChannelAccount[]> GetConversationMembers(string conversationId)
        {
            Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            return null;
        }

        /// <summary>
        /// GetConversationPagedMembers
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="pageSize">Suggested page size</param>
        /// <param name="continuationToken">Continuation Token</param>
        [HttpGet]
        [Route("/v3/conversations/{conversationId}/pagedmembers")]
        public virtual async Task<PagedMembersResult> GetConversationPagedMembers(string conversationId, int pageSize = -1, string continuationToken = null)
        {
            Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            return null;
        }

        /// <summary>
        /// DeleteConversationMember
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="memberId">ID of the member to delete from this conversation</param>
        [HttpDelete]
        [Route("/v3/conversations/{conversationId}/members/{memberId}")]
        public virtual async Task DeleteConversationMember(string conversationId, string memberId)
        {
            Response.StatusCode = (int)HttpStatusCode.NotImplemented;
        }

        /// <summary>
        /// GetActivityMembers
        /// </summary>
        /// <remarks>
        /// Markdown=Content\Methods\GetActivityMembers.md
        /// </remarks>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="activityId">Activity ID</param>
        [HttpGet]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}/members")]
        public virtual async Task<ChannelAccount[]> GetActivityMembers(string conversationId, string activityId)
        {
            Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            return null;
        }

        /// <summary>
        /// UploadAttachment
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="attachmentUpload">Attachment data</param>
        [HttpPost]
        [Route("/v3/conversations/{conversationId}/attachments")]
        public virtual async Task<ResourceResponse> UploadAttachment(string conversationId, [FromBody]AttachmentData attachmentUpload)
        {
            Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            return null;
        }

        ///// <summary>
        ///// InitiateHandoff - initiate handoff to agent for given conversation
        ///// </summary>
        ///// <remarks>
        ///// Markdown=Content\Methods\InitiateHandoff.md
        ///// </remarks>
        ///// <param name="conversationId">Conversation ID</param>
        ///// <param name="handoffParameters">Handoff context containing activities and channel-specific data</param>
        //[HttpPost]
        //[Route("{conversationId}/handoff")]
        //public virtual async Task<ResourceResponse> InitiateHandoff(string conversationId, [FromBody]HandoffParameters handoffParameters)
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// GetHandoffStatus - get status of handoff for given conversation
        ///// </summary>
        ///// <remarks>
        ///// Markdown=Content\Methods\GetHandoffStatus.md
        ///// </remarks>
        ///// <param name="conversationId">Conversation ID</param>
        //[HttpGet]
        //[Route("{conversationId}/handoff")]
        //[ProducesResponseType(typeof(HandoffStatus), StatusCodes.Status200OK)]
        //public virtual async Task<HandoffStatus> GetHandoffStatus(string conversationId)
        //{
        //    var botIdentity = GetCurrentBot();
        //    return await ControllerBaseUtility.ExecuteAndRespond(Request.Method, async () =>
        //    {
        //        var result = await ChannelAPI.GetHandoffStatusAsync(GetCurrentBotRequest(), botIdentity.Id, botIdentity.MsaAppId, conversationId).ConfigureAwait(false);
        //        return result.ToString();
        //    });
        //}

        private async Task<ResourceResponse> ProcessActivityInBot(Activity activity)
        {
            // end of conversation goes up to the bot
            if (!activity.Conversation.Properties.ContainsKey("serviceUrl"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }

            // restore original serviceUrl so we can forward on to user
            // clean up from/recipient so that userState is loaded correctly, TurnContext is basedon From=User
            var user = activity.Recipient;
            var skill = activity.From;
            activity.From = user;
            activity.Recipient = skill;
            activity.ServiceUrl = (string)activity.Conversation.Properties["serviceUrl"];

            // We call our adapter using the BotAppId claim, so turnContext has the bot claims
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                // Adding claims for both Emulator and Channel.
                new Claim(AuthenticationConstants.AudienceClaim, BotAppId),
                new Claim(AuthenticationConstants.AppIdClaim, BotAppId),
            });

            // send up to the bot
            await adapter.ProcessActivityAsync(claimsIdentity, activity, bot.OnTurnAsync, CancellationToken.None);

            return new ResourceResponse(id: activity.Id);
        }
    }
}
