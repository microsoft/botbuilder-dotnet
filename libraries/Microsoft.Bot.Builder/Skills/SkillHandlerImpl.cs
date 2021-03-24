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
    /// This class inherited all the implementations of <see cref="SkillHandler"/> class since we needed similar code for <see cref="CloudSkillHandler"/>.
    /// The <see cref="CloudSkillHandler"/> class differs from <see cref="SkillHandler"/> class only in authentication by making use of <see cref="BotFrameworkAuthentication"/> class.
    /// This class is internal since it is only used in skill handler classes.
    /// </summary>
    internal class SkillHandlerImpl
    {
        private readonly string _skillConversationReferenceKey;
        private readonly BotAdapter _adapter;
        private readonly IBot _bot;
        private readonly SkillConversationIdFactoryBase _conversationIdFactory;
        private readonly Func<string> _getOAuthScope;
        private readonly ILogger _logger;

        internal SkillHandlerImpl(
            string skillConversationReferenceKey,
            BotAdapter adapter,
            IBot bot,
            SkillConversationIdFactoryBase conversationIdFactory,
            Func<string> getOAuthScope,
            ILogger logger = null)
        {
            if (string.IsNullOrWhiteSpace(skillConversationReferenceKey))
            {
                throw new ArgumentNullException(nameof(skillConversationReferenceKey));
            }

            _skillConversationReferenceKey = skillConversationReferenceKey;
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _conversationIdFactory = conversationIdFactory ?? throw new ArgumentNullException(nameof(conversationIdFactory));
            _getOAuthScope = getOAuthScope ?? (() => string.Empty);
            _logger = logger ?? NullLogger.Instance;
        }

        internal async Task<ResourceResponse> OnSendToConversationAsync(ClaimsIdentity claimsIdentity, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            return await ProcessActivityAsync(claimsIdentity, conversationId, null, activity, cancellationToken).ConfigureAwait(false);
        }

        internal async Task<ResourceResponse> OnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            return await ProcessActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
        }

        internal async Task OnDeleteActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            var skillConversationReference = await GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);

            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                turnContext.TurnState.Add(_skillConversationReferenceKey, skillConversationReference);
                await turnContext.DeleteActivityAsync(activityId, cancellationToken).ConfigureAwait(false);
            });

            await _adapter.ContinueConversationAsync(claimsIdentity, skillConversationReference.ConversationReference, skillConversationReference.OAuthScope, callback, cancellationToken).ConfigureAwait(false);
        }

        internal async Task<ResourceResponse> OnUpdateActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            var skillConversationReference = await GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);

            ResourceResponse resourceResponse = null;
            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                turnContext.TurnState.Add(_skillConversationReferenceKey, skillConversationReference);
                activity.ApplyConversationReference(skillConversationReference.ConversationReference);
                turnContext.Activity.Id = activityId;
                turnContext.Activity.CallerId = $"{CallerIdConstants.BotToBotPrefix}{JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims)}";
                resourceResponse = await turnContext.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            });

            await _adapter.ContinueConversationAsync(claimsIdentity, skillConversationReference.ConversationReference, skillConversationReference.OAuthScope, callback, cancellationToken).ConfigureAwait(false);

            return resourceResponse ?? new ResourceResponse(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
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
            turnContext.Activity.Locale = endOfConversationActivity.Locale;
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
            turnContext.Activity.Locale = eventActivity.Locale;
            turnContext.Activity.LocalTimestamp = eventActivity.LocalTimestamp;
            turnContext.Activity.Timestamp = eventActivity.Timestamp;
            turnContext.Activity.ChannelData = eventActivity.ChannelData;
            turnContext.Activity.Properties = eventActivity.Properties;
        }

        private async Task<SkillConversationReference> GetSkillConversationReferenceAsync(string conversationId, CancellationToken cancellationToken)
        {
            SkillConversationReference skillConversationReference;
            try
            {
                skillConversationReference = await _conversationIdFactory.GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);
            }
            catch (NotImplementedException)
            {
                _logger.LogWarning("Got NotImplementedException when trying to call GetSkillConversationReferenceAsync() on the ConversationIdFactory, attempting to use deprecated GetConversationReferenceAsync() method instead.");

                // Attempt to get SkillConversationReference using deprecated method.
                // this catch should be removed once we remove the deprecated method. 
                // We need to use the deprecated method for backward compatibility.
#pragma warning disable 618
                var conversationReference = await _conversationIdFactory.GetConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);
#pragma warning restore 618
                skillConversationReference = new SkillConversationReference
                {
                    ConversationReference = conversationReference,
                    OAuthScope = _getOAuthScope()
                };
            }

            if (skillConversationReference == null)
            {
                _logger.LogError($"Unable to get skill conversation reference for conversationId {conversationId}.");
                throw new KeyNotFoundException();
            }

            return skillConversationReference;
        }

        private async Task<ResourceResponse> ProcessActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string replyToActivityId, Activity activity, CancellationToken cancellationToken)
        {
            var skillConversationReference = await GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);

            ResourceResponse resourceResponse = null;

            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                turnContext.TurnState.Add(_skillConversationReferenceKey, skillConversationReference);
                activity.ApplyConversationReference(skillConversationReference.ConversationReference);
                turnContext.Activity.Id = replyToActivityId;
                turnContext.Activity.CallerId = $"{CallerIdConstants.BotToBotPrefix}{JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims)}";
                switch (activity.Type)
                {
                    case ActivityTypes.EndOfConversation:
                        await _conversationIdFactory.DeleteConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);
                        ApplyEoCToTurnContextActivity(turnContext, activity);
                        await _bot.OnTurnAsync(turnContext, ct).ConfigureAwait(false);
                        break;
                    case ActivityTypes.Event:
                        ApplyEventToTurnContextActivity(turnContext, activity);
                        await _bot.OnTurnAsync(turnContext, ct).ConfigureAwait(false);
                        break;
                    default:
                        resourceResponse = await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                        break;
                }
            });

            await _adapter.ContinueConversationAsync(claimsIdentity, skillConversationReference.ConversationReference, skillConversationReference.OAuthScope, callback, cancellationToken).ConfigureAwait(false);

            return resourceResponse ?? new ResourceResponse(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        }
    }
}
