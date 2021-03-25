// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// A Bot Framework Handler for skills.
    /// </summary>
    public class CloudSkillHandler : CloudChannelServiceHandler
    {
        /// <summary>
        /// The skill conversation reference.
        /// </summary>
        public static readonly string SkillConversationReferenceKey = $"{typeof(CloudSkillHandler).Namespace}.SkillConversationReference";

        private readonly SkillHandlerImpl _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudSkillHandler"/> class using BotFrameworkAuth.
        /// </summary>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="conversationIdFactory">A <see cref="SkillConversationIdFactoryBase"/> to unpack the conversation ID and map it to the calling bot.</param>
        /// <param name="auth">auth.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public CloudSkillHandler(
            BotAdapter adapter,
            IBot bot,
            SkillConversationIdFactoryBase conversationIdFactory,
            BotFrameworkAuthentication auth,
            ILogger logger = null)
            : base(auth)
        {
            if (adapter == null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            if (conversationIdFactory == null)
            {
                throw new ArgumentNullException(nameof(conversationIdFactory));
            }

            _inner = new SkillHandlerImpl(
                SkillConversationReferenceKey,
                adapter,
                bot,
                conversationIdFactory,
                auth.GetOriginatingAudience,
                logger ?? NullLogger.Instance);
        }

        /// <summary>
        /// SendToConversation() API for Skill.
        /// </summary>
        /// <remarks>
        /// This method allows you to send an activity to the end of a conversation.
        ///
        /// This is slightly different from ReplyToActivity().
        /// * SendToConversation(conversationId) - will append the activity to the end
        /// of the conversation according to the timestamp or semantics of the channel.
        /// * ReplyToActivity(conversationId,ActivityId) - adds the activity as a reply
        /// to another activity, if the channel supports it. If the channel does not
        /// support nested replies, ReplyToActivity falls back to SendToConversation.
        ///
        /// Use ReplyToActivity when replying to a specific activity in the
        /// conversation.
        ///
        /// Use SendToConversation in all other cases.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>conversationId.</param> 
        /// <param name='activity'>Activity to send.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        protected override async Task<ResourceResponse> OnSendToConversationAsync(ClaimsIdentity claimsIdentity, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            return await _inner.OnSendToConversationAsync(claimsIdentity, conversationId, activity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// ReplyToActivity() API for Skill.
        /// </summary>
        /// <remarks>
        /// This method allows you to reply to an activity.
        ///
        /// This is slightly different from SendToConversation().
        /// * SendToConversation(conversationId) - will append the activity to the end
        /// of the conversation according to the timestamp or semantics of the channel.
        /// * ReplyToActivity(conversationId,ActivityId) - adds the activity as a reply
        /// to another activity, if the channel supports it. If the channel does not
        /// support nested replies, ReplyToActivity falls back to SendToConversation.
        ///
        /// Use ReplyToActivity when replying to a specific activity in the
        /// conversation.
        ///
        /// Use SendToConversation in all other cases.
        /// </remarks>
        /// <param name="claimsIdentity">claimsIdentity for the bot, should have AudienceClaim, AppIdClaim and ServiceUrlClaim.</param>
        /// <param name='conversationId'>Conversation ID.</param>
        /// <param name='activityId'>activityId the reply is to (OPTIONAL).</param>
        /// <param name='activity'>Activity to send.</param>
        /// <param name='cancellationToken'>The cancellation token.</param>
        /// <returns>task for a resource response.</returns>
        protected override async Task<ResourceResponse> OnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            return await _inner.OnReplyToActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnDeleteActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            await _inner.OnDeleteActivityAsync(claimsIdentity, conversationId, activityId, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task<ResourceResponse> OnUpdateActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            return await _inner.OnUpdateActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
        }
    }
}
