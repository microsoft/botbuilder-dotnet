// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace SkillHost.Controllers
{
    [ApiController]
    [Route("/v3/conversations")]
    public class SkillHostController : ControllerBase
    {
        private readonly BotAdapter _adapter;
        private readonly IBot _bot;

        public SkillHostController(BotAdapter adapter, IConfiguration configuration, IBot bot)
        {
            // adapter to use for calling back to channel
            _adapter = adapter;
            _bot = bot;
            BotAppId = configuration.GetValue<string>("MicrosoftAppId");
        }

        public string BotAppId { get; set; }

        public static ConversationInfo GetConversationInfo(string skillConversationId)
        {
            var parts = JsonConvert.DeserializeObject<string[]>(Encoding.UTF8.GetString(Convert.FromBase64String(skillConversationId)));
            return new ConversationInfo()
            {
                ServiceUrl = parts[0],
                ConversationId = parts[1],
            };
        }

        /// <summary>
        /// ReplyToActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId the reply is to (OPTIONAL).</param>
        /// <param name="activity">Activity to send.</param>
        /// <returns>Resource.</returns>
        [HttpPost]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
        public virtual async Task<ResourceResponse> ReplyToActivity(string conversationId, string activityId, [FromBody] Activity activity)
        {
            ResourceResponse resourceResponse = null;
            var conversationInfo = GetConversationInfo(conversationId);
            activity.ServiceUrl = conversationInfo.ServiceUrl;
            activity.Conversation.Id = conversationInfo.ConversationId;

            var originalConversationReference = activity.GetConversationReference();
            originalConversationReference.Bot = activity.From;
            originalConversationReference.User = activity.Recipient;

            if (activity.Type == ActivityTypes.EndOfConversation || activity.Type == ActivityTypes.Event)
            {
                activity.ApplyConversationReference(originalConversationReference, isIncoming: true);

                // TEMPORARY claim
                var claimsIdentity = new ClaimsIdentity(new List<Claim>(), "anonymous");

                // send up to the bot 
                // TODO: WE NEED PROCESSACTIVITY TO BE ON BOTADAPTER.CS
                await ((BotFrameworkHttpAdapter)_adapter).ProcessActivityAsync(claimsIdentity, activity, _bot.OnTurnAsync, CancellationToken.None);
                return new ResourceResponse(id: Guid.NewGuid().ToString("N"));
            }

            await _adapter.ContinueConversationAsync(
                BotAppId,
                originalConversationReference,
                async (context, cancellationToken) =>
                {
                    activity.ApplyConversationReference(originalConversationReference, isIncoming: false);
                    activity.ReplyToId = activityId;
                    resourceResponse = await context.SendActivityAsync(activity, cancellationToken);
                },
                CancellationToken.None);

            return resourceResponse;
        }

        /// <summary>
        /// UpdateActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId to update.</param>
        /// <param name="activity">replacement Activity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation with resourceResponse.</returns>
        [HttpPut]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
        public virtual async Task<ResourceResponse> UpdateActivity(string conversationId, string activityId, [FromBody] Activity activity)
        {
            ResourceResponse resourceResponse = null;
            var conversationInfo = GetConversationInfo(conversationId);
            activity.ServiceUrl = conversationInfo.ServiceUrl;
            activity.Conversation.Id = conversationInfo.ConversationId;

            var originalConversationReference = activity.GetConversationReference();
            originalConversationReference.Bot = activity.From;
            originalConversationReference.User = activity.Recipient;

            await _adapter.ContinueConversationAsync(
                BotAppId,
                originalConversationReference,
                async (context, cancellationToken) =>
                {
                    resourceResponse = await context.UpdateActivityAsync(activity, cancellationToken);
                },
                CancellationToken.None);
            return resourceResponse;
        }

        /// <summary>
        /// DeleteActivity.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">activityId to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpDelete]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}")]
        public virtual async Task DeleteActivity(string conversationId, string activityId)
        {
            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            await _adapter.ContinueConversationAsync(
                BotAppId,
                originalConversationReference,
                async (context, cancellationToken) =>
                {
                    await context.DeleteActivityAsync(activityId, cancellationToken);
                },
                CancellationToken.None);
        }

        /// <summary>
        /// GetConversations.
        /// </summary>
        /// <param name="continuationToken">skip or continuation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet]
        [Route("/v3/conversations")]
        public virtual Task<ConversationsResult> GetConversations(string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetConversationMembers.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation returnin channel accounts.</returns>
        [HttpGet]
        [Route("/v3/conversations/{conversationId}/members")]
        public virtual async Task<ChannelAccount[]> GetConversationMembers(string conversationId)
        {
            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            ChannelAccount[] accounts = null;
            await _adapter.ContinueConversationAsync(
                BotAppId,
                originalConversationReference,
                async (context, cancellationToken) =>
                {
                    var result = await context.Adapter.GetConversationMembersAsync(context, cancellationToken);
                    accounts = result.ToArray();
                },
                CancellationToken.None);
            return accounts;
        }

        /// <summary>
        /// GetConversationPagedMembers.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="pageSize">Suggested page size.</param>
        /// <param name="continuationToken">Continuation Token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation with pagedMembersResult.</returns>
        [HttpGet]
        [Route("/v3/conversations/{conversationId}/pagedmembers")]
        public virtual async Task<PagedMembersResult> GetConversationPagedMembers(string conversationId, int pageSize = -1, string continuationToken = null)
        {
            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            PagedMembersResult result = null;

            await _adapter.ContinueConversationAsync(
                BotAppId,
                originalConversationReference,
                async (context, cancellationToken) =>
                {
                    result = await context.Adapter.GetConversationPagedMembersAsync(context, pageSize, continuationToken, cancellationToken).ConfigureAwait(false);
                },
                CancellationToken.None);
            return result;
        }

        /// <summary>
        /// DeleteConversationMember.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="memberId">ID of the member to delete from this conversation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpDelete]
        [Route("/v3/conversations/{conversationId}/members/{memberId}")]
        public virtual async Task DeleteConversationMember(string conversationId, string memberId)
        {
            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            await _adapter.ContinueConversationAsync(
                this.BotAppId,
                originalConversationReference,
                async (context, cancellationToken) =>
                {
                    await context.Adapter.DeleteConversationMemberAsync(context, memberId, cancellationToken);
                },
                CancellationToken.None);
        }

        /// <summary>
        /// GetActivityMembers.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="activityId">Activity ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation with ChannelAccounts.</returns>
        [HttpGet]
        [Route("/v3/conversations/{conversationId}/activities/{activityId}/members")]
        public virtual async Task<ChannelAccount[]> GetActivityMembers(string conversationId, string activityId)
        {
            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            ChannelAccount[] accounts = null;
            await _adapter.ContinueConversationAsync(
                BotAppId,
                originalConversationReference,
                async (context, cancellationToken) =>
                {
                    var result = await context.Adapter.GetActivityMembersAsync(context, activityId, cancellationToken);
                    accounts = result.ToArray();
                },
                CancellationToken.None);
            return accounts;
        }

        /// <summary>
        /// UploadAttachment.
        /// </summary>
        /// <param name="conversationId">Conversation ID.</param>
        /// <param name="attachmentUpload">Attachment data.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation returning a ResourceResponse.</returns>
        [HttpPost]
        [Route("/v3/conversations/{conversationId}/attachments")]
        public virtual async Task<ResourceResponse> UploadAttachment(string conversationId, [FromBody] AttachmentData attachmentUpload)
        {
            var conversationInfo = GetConversationInfo(conversationId);
            var originalConversationReference = GetConversationReferenceFromInfo(conversationInfo);

            ResourceResponse response = null;
            await _adapter.ContinueConversationAsync(
                BotAppId,
                originalConversationReference,
                async (context, cancellationToken) =>
                {
                    response = await context.Adapter.UploadAttachment(context, attachmentUpload, cancellationToken);
                },
                CancellationToken.None);
            return response;
        }

        private static ConversationReference GetConversationReferenceFromInfo(ConversationInfo conversationInfo)
        {
            var originalConversationReference = new ConversationReference()
            {
                ChannelId = "Skill" /* skillId from claims */,
                ServiceUrl = conversationInfo.ServiceUrl,
                Conversation = new ConversationAccount(id: conversationInfo.ConversationId),
                Bot = new ChannelAccount(id: "unknown", role: RoleTypes.Bot),
                User = new ChannelAccount(id: "unknown", role: "Skill"),
            };
            return originalConversationReference;
        }

        public class ConversationInfo
        {
            public string ServiceUrl { get; set; }

            public string ConversationId { get; set; }
        }
    }
}
