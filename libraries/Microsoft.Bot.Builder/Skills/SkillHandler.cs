// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class SkillHandler : ChannelServiceHandler
    {
        public static readonly string SkillConversationReferenceKey = $"{typeof(SkillHandler).Namespace}.SkillConversationReference";

        private readonly BotAdapter _adapter;
        private readonly IBot _bot;
        private readonly SkillConversationIdFactoryBase _conversationIdIdFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillHandler"/> class,
        /// using a credential provider.
        /// </summary>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="conversationIdFactory">A <see cref="SkillConversationIdFactoryBase"/> to unpack the conversation ID and map it to the calling bot.</param>
        /// <param name="credentialProvider">The credential provider.</param>
        /// <param name="authConfig">The authentication configuration.</param>
        /// <param name="channelProvider">The channel provider.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        /// <exception cref="ArgumentNullException">throw ArgumentNullException.</exception>
        /// <remarks>Use a <see cref="MiddlewareSet"/> object to add multiple middleware
        /// components in the constructor. Use the Use(<see cref="IMiddleware"/>) method to
        /// add additional middleware to the adapter after construction.
        /// </remarks>
        public SkillHandler(
            BotAdapter adapter,
            IBot bot,
            SkillConversationIdFactoryBase conversationIdFactory,
            ICredentialProvider credentialProvider,
            AuthenticationConfiguration authConfig,
            IChannelProvider channelProvider = null,
            ILogger logger = null)
            : base(credentialProvider, authConfig, channelProvider)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _conversationIdIdFactory = conversationIdFactory ?? throw new ArgumentNullException(nameof(conversationIdFactory));
            _logger = logger ?? NullLogger.Instance;
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
            return await ProcessActivityAsync(claimsIdentity, conversationId, null, activity, cancellationToken).ConfigureAwait(false);
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
            return await ProcessActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
        }

        private static void ApplyEoCToTurnContextActivity(ITurnContext turnContext, Activity endOfConversationActivity)
        {
            // transform the turnContext.Activity to be the EndOfConversation.
            turnContext.Activity.Type = endOfConversationActivity.Type;
            turnContext.Activity.Text = endOfConversationActivity.Text;
            turnContext.Activity.Code = endOfConversationActivity.Code;

            turnContext.Activity.ReplyToId = endOfConversationActivity.ReplyToId;
            turnContext.Activity.Value = endOfConversationActivity.Value;
            turnContext.Activity.Entities = endOfConversationActivity.Entities;
            turnContext.Activity.LocalTimestamp = endOfConversationActivity.LocalTimestamp;
            turnContext.Activity.Timestamp = endOfConversationActivity.Timestamp;
            turnContext.Activity.ChannelData = endOfConversationActivity.ChannelData;
            turnContext.Activity.Properties = endOfConversationActivity.Properties;
        }

        private static void ApplyEventToTurnContextActivity(ITurnContext turnContext, Activity eventActivity)
        {
            // transform the turnContext.Activity to be the EventActivity.
            turnContext.Activity.Type = eventActivity.Type;
            turnContext.Activity.Name = eventActivity.Name;
            turnContext.Activity.Value = eventActivity.Value;
            turnContext.Activity.RelatesTo = eventActivity.RelatesTo;

            turnContext.Activity.ReplyToId = eventActivity.ReplyToId;
            turnContext.Activity.Value = eventActivity.Value;
            turnContext.Activity.Entities = eventActivity.Entities;
            turnContext.Activity.LocalTimestamp = eventActivity.LocalTimestamp;
            turnContext.Activity.Timestamp = eventActivity.Timestamp;
            turnContext.Activity.ChannelData = eventActivity.ChannelData;
            turnContext.Activity.Properties = eventActivity.Properties;
        }

        private async Task<ResourceResponse> ProcessActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string replyToActivityId, Activity activity, CancellationToken cancellationToken)
        {
            var conversationReference = await _conversationIdIdFactory.GetConversationReferenceAsync(conversationId, CancellationToken.None).ConfigureAwait(false);

            if (conversationReference == null)
            {
                throw new KeyNotFoundException();
            }

            var skillConversationReference = activity.GetConversationReference();

            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                turnContext.TurnState.Add(SkillConversationReferenceKey, skillConversationReference);

                activity.ApplyConversationReference(conversationReference);

                turnContext.Activity.Id = replyToActivityId;
                switch (activity.Type)
                {
                    case ActivityTypes.EndOfConversation:
                        await _conversationIdIdFactory.DeleteConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);
                        ApplyEoCToTurnContextActivity(turnContext, activity);
                        await _bot.OnTurnAsync(turnContext, ct).ConfigureAwait(false);
                        break;
                    case ActivityTypes.Event:
                        ApplyEventToTurnContextActivity(turnContext, activity);
                        await _bot.OnTurnAsync(turnContext, ct).ConfigureAwait(false);
                        break;
                    default:
                        await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                        break;
                }
            });

            await _adapter.ContinueConversationAsync(claimsIdentity, conversationReference, callback, cancellationToken).ConfigureAwait(false);
            return new ResourceResponse(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        }
    }
}
