// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ChannelPrototype.Controllers
{
    [ApiController]
    [Route("/v3/conversations")]
    public class SkillHostController : ControllerBase
    {
        private BotFrameworkAdapter adapter;
        private IStorage storage;
        private IConfiguration configuration;
        private IBot bot;

        public SkillHostController(BotFrameworkHttpAdapter adapter, IStorage storage, IConfiguration configuration, IBot bot)
        {
            // adapter to use for calling back to channel
            this.adapter = adapter;
            this.storage = storage;
            this.bot = bot;
            this.configuration = configuration;
            this.AppId = configuration.GetValue<string>("MicrosoftAppId");
        }

        public string AppId { get; set; }

        /// <summary>
        /// Supported skills, SkillId => info necessary to start conversation with skill
        /// </summary>
        public static ConcurrentDictionary<string, SkillRegistration> Skills => new ConcurrentDictionary<string, SkillRegistration>();

        /// <summary>
        /// GET original conversationReference 
        /// </summary>
        /// <param name="skillConversationId"></param>
        /// <returns></returns>
        public async Task<ConversationReference> GetOriginalConversationReference(string skillConversationId)
        {
            string key = $"skill/{skillConversationId}";
            var result = await this.storage.ReadAsync(new string[] { key });
            if (result != null && result.ContainsKey(key))
            {
                return JsonConvert.DeserializeObject<ConversationReference>(JsonConvert.SerializeObject(result[key]));
            }
            return null;
        }

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
            ResourceResponse resourceResponse = null;

            var originalConversationReference = await GetOriginalConversationReference(conversationId);
            if (originalConversationReference == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return null;
            }

            if (activity.Type == ActivityTypes.EndOfConversation)
            {
                return await SendToAdapterBot(activity);
            }

            await adapter.ContinueConversationAsync(this.AppId, originalConversationReference, async (context, cancellationToken) =>
            {
                var activityToSend = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                activityToSend.ApplyConversationReference(originalConversationReference);
                resourceResponse = await context.SendActivityAsync(activityToSend, cancellationToken);
            }, CancellationToken.None);

            return resourceResponse;
        }

        private async Task<ResourceResponse> SendToAdapterBot(Activity activity)
        {
            // end of conversation goes up to the bot

            // TEMPORARY claim
            var claimsIdentity = new ClaimsIdentity(new List<Claim>(), "anonymous");

            // send up to the bot
            await adapter.ProcessActivityAsync(claimsIdentity, activity, bot.OnTurnAsync, CancellationToken.None);
            return new ResourceResponse(id: Guid.NewGuid().ToString("N"));
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
            throw new NotImplementedException();
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
            ResourceResponse resourceResponse = null;

            var originalConversationReference = await GetOriginalConversationReference(conversationId);
            if (originalConversationReference == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return null;
            }

            if (activity.Type == ActivityTypes.EndOfConversation)
            {
                return await SendToAdapterBot(activity);
            }

            await adapter.ContinueConversationAsync(this.AppId, originalConversationReference, async (context, cancellationToken) =>
            {
                var activityToSend = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                activityToSend.ApplyConversationReference(originalConversationReference, isIncoming: false);
                activityToSend.ReplyToId = activityId;
                resourceResponse = await context.SendActivityAsync(activityToSend, cancellationToken);
            }, CancellationToken.None);

            return resourceResponse;
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
            ResourceResponse resourceResponse = null;

            var originalConversationReference = await GetOriginalConversationReference(conversationId);
            if (originalConversationReference == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return null;
            }

            await adapter.ContinueConversationAsync(this.AppId, originalConversationReference, async (context, cancellationToken) =>
            {
                var activityToSend = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                activityToSend.ApplyConversationReference(originalConversationReference, isIncoming: false);
                activityToSend.Id = activityId;

                resourceResponse = await context.UpdateActivityAsync(activityToSend, cancellationToken);
            }, CancellationToken.None);
            return resourceResponse;
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
            var originalConversationReference = await GetOriginalConversationReference(conversationId);
            if (originalConversationReference == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            await adapter.ContinueConversationAsync(this.AppId, originalConversationReference, async (context, cancellationToken) =>
            {
                await context.DeleteActivityAsync(activityId);
            }, CancellationToken.None);
        }

        /// <summary>
        /// GetConversations
        /// </summary>
        /// <param name="continuationToken">skip or continuation token</param>
        [HttpGet]
        [Route("/v3/conversations")]
        public virtual async Task<ConversationsResult> GetConversations(string continuationToken = null)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// GetConversationMembers
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        [HttpGet]
        [Route("/v3/conversations/{conversationId}/members")]
        public virtual async Task<ChannelAccount[]> GetConversationMembers(string conversationId)
        {
            var originalConversationReference = await GetOriginalConversationReference(conversationId);
            if (originalConversationReference == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return null;
            }

            ChannelAccount[] accounts = null;
            await adapter.ContinueConversationAsync(this.AppId, originalConversationReference, async (context, cancellationToken) =>
            {
                var result = await adapter.GetConversationMembersAsync(context, cancellationToken);
                accounts = result.ToArray();
            }, CancellationToken.None);

            return accounts;
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
            throw new NotImplementedException();
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
            var originalConversationReference = await GetOriginalConversationReference(conversationId);
            if (originalConversationReference == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            await adapter.ContinueConversationAsync(this.AppId, originalConversationReference, async (context, cancellationToken) =>
            {
                await adapter.DeleteConversationMemberAsync(context, memberId, cancellationToken);
            }, CancellationToken.None);
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
            var originalConversationReference = await GetOriginalConversationReference(conversationId);
            if (originalConversationReference == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return null;
            }

            ChannelAccount[] accounts = null;
            await adapter.ContinueConversationAsync(this.AppId, originalConversationReference, async (context, cancellationToken) =>
            {
                var results = await adapter.GetActivityMembersAsync(context, activityId, cancellationToken);
                accounts = results.ToArray();
            }, CancellationToken.None);
            return accounts;
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
            throw new NotImplementedException();
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
    }
}
