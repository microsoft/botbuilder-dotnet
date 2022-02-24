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
    public class CloudSkillHandler : CloudChannelServiceHandler
    {
        /// <summary>
        /// The skill conversation reference.
        /// </summary>
        public static readonly string SkillConversationReferenceKey = $"{typeof(CloudSkillHandler).Namespace}.SkillConversationReference";

        private readonly BotAdapter _adapter;
        private readonly IBot _bot;
        private readonly SkillConversationIdFactoryBase _conversationIdFactory;
        private readonly BotFrameworkAuthentication _auth;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudSkillHandler"/> class using BotFrameworkAuth.
        /// </summary>
        /// <param name="adapter">An instance of the <see cref="BotAdapter"/> that will handle the request.</param>
        /// <param name="bot">The <see cref="IBot"/> instance.</param>
        /// <param name="conversationIdFactory">A <see cref="SkillConversationIdFactoryBase"/> to unpack the conversation ID and map it to the calling bot.</param>
        /// <param name="auth">The BotFrameworkAuthentication object used to authenticate skills requests.</param>
        /// <param name="logger">The ILogger implementation this adapter should use.</param>
        public CloudSkillHandler(
            BotAdapter adapter,
            IBot bot,
            SkillConversationIdFactoryBase conversationIdFactory,
            BotFrameworkAuthentication auth,
            ILogger logger = null)
            : base(auth)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _conversationIdFactory = conversationIdFactory ?? throw new ArgumentNullException(nameof(conversationIdFactory));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _logger = logger ?? NullLogger.Instance;
        }

        /// <inheritdoc />
        protected override async Task<ResourceResponse> OnSendToConversationAsync(ClaimsIdentity claimsIdentity, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            return await ProcessActivityAsync(claimsIdentity, conversationId, null, activity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override async Task<ResourceResponse> OnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            return await ProcessActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnDeleteActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            var skillConversationReference = await GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);

            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                turnContext.TurnState.Add(SkillConversationReferenceKey, skillConversationReference);
                await turnContext.DeleteActivityAsync(activityId, ct).ConfigureAwait(false);
            });

            await _adapter.ContinueConversationAsync(claimsIdentity, skillConversationReference.ConversationReference, skillConversationReference.OAuthScope, callback, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task<ResourceResponse> OnUpdateActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            var skillConversationReference = await GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);

            ResourceResponse resourceResponse = null;
            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                turnContext.TurnState.Add(SkillConversationReferenceKey, skillConversationReference);
                activity.ApplyConversationReference(skillConversationReference.ConversationReference);
                turnContext.Activity.Id = activityId;
                turnContext.Activity.CallerId = $"{CallerIdConstants.BotToBotPrefix}{claimsIdentity.Claims.GetAppIdFromClaims()}";
                resourceResponse = await turnContext.UpdateActivityAsync(activity, ct).ConfigureAwait(false);
            });

            await _adapter.ContinueConversationAsync(claimsIdentity, skillConversationReference.ConversationReference, skillConversationReference.OAuthScope, callback, cancellationToken).ConfigureAwait(false);

            return resourceResponse ?? new ResourceResponse(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        }

        private static void ApplySkillActivityToTurnContext(ITurnContext turnContext, Activity activity)
        {
            // adapter.ContinueConversation() sends an event activity with ContinueConversation in the name.
            // this warms up the incoming middlewares but once that's done and we hit the custom callback,
            // we need to swap the values back to the ones received from the skill so the bot gets the actual activity.
            turnContext.Activity.ChannelData = activity.ChannelData;
            turnContext.Activity.Code = activity.Code;
            turnContext.Activity.Entities.Clear();
            ((List<Entity>)turnContext.Activity.Entities).AddRange(activity.Entities);
            turnContext.Activity.Locale = activity.Locale;
            turnContext.Activity.LocalTimestamp = activity.LocalTimestamp;
            turnContext.Activity.Name = activity.Name;
            turnContext.Activity.Properties.RemoveAll();
            turnContext.Activity.Properties.Merge(activity.Properties);
            turnContext.Activity.RelatesTo = activity.RelatesTo;
            turnContext.Activity.ReplyToId = activity.ReplyToId;
            turnContext.Activity.Timestamp = activity.Timestamp;
            turnContext.Activity.Text = activity.Text;
            turnContext.Activity.Type = activity.Type;
            turnContext.Activity.Value = activity.Value;
        }

        private async Task<ResourceResponse> ProcessActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string replyToActivityId, Activity activity, CancellationToken cancellationToken)
        {
            var skillConversationReference = await GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);

            ResourceResponse resourceResponse = null;

            var callback = new BotCallbackHandler(async (turnContext, ct) =>
            {
                turnContext.TurnState.Add(SkillConversationReferenceKey, skillConversationReference);
                activity.ApplyConversationReference(skillConversationReference.ConversationReference);
                turnContext.Activity.Id = replyToActivityId;
                turnContext.Activity.CallerId = $"{CallerIdConstants.BotToBotPrefix}{claimsIdentity.Claims.GetAppIdFromClaims()}";
                switch (activity.Type)
                {
                    case ActivityTypes.EndOfConversation:
                        await _conversationIdFactory.DeleteConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);
                        await SendToBotAsync(activity, turnContext, ct).ConfigureAwait(false);
                        break;
                    case ActivityTypes.Event:
                        await SendToBotAsync(activity, turnContext, ct).ConfigureAwait(false);
                        break;
                    case ActivityTypes.Command:
                    case ActivityTypes.CommandResult:
                        if (activity.Name.StartsWith("application/", StringComparison.Ordinal))
                        {
                            // Send to channel and capture the resource response for the SendActivityCall so we can return it.
                            resourceResponse = await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            await SendToBotAsync(activity, turnContext, ct).ConfigureAwait(false);
                        }

                        break;

                    default:
                        // Capture the resource response for the SendActivityCall so we can return it.
                        resourceResponse = await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                        break;
                }
            });

            await _adapter.ContinueConversationAsync(claimsIdentity, skillConversationReference.ConversationReference, skillConversationReference.OAuthScope, callback, cancellationToken).ConfigureAwait(false);

            return resourceResponse ?? new ResourceResponse(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        }

        private async Task<SkillConversationReference> GetSkillConversationReferenceAsync(string conversationId, CancellationToken cancellationToken)
        {
            SkillConversationReference skillConversationReference;
            try
            {
                skillConversationReference = await _conversationIdFactory.GetSkillConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);
            }
            catch (NotImplementedException ex)
            {
                Log.SkillConversationReferenceNotImplemented(_logger, ex);

                // Attempt to get SkillConversationReference using deprecated method.
                // this catch should be removed once we remove the deprecated method. 
                // We need to use the deprecated method for backward compatibility.
#pragma warning disable 618
                var conversationReference = await _conversationIdFactory.GetConversationReferenceAsync(conversationId, cancellationToken).ConfigureAwait(false);
#pragma warning restore 618
                skillConversationReference = new SkillConversationReference
                {
                    ConversationReference = conversationReference,
                    OAuthScope = _auth.GetOriginatingAudience()
                };
            }

            if (skillConversationReference == null)
            {
                Log.SkillConversationReferenceNotFound(_logger, conversationId);
                throw new KeyNotFoundException();
            }

            return skillConversationReference;
        }

        private async Task SendToBotAsync(Activity activity, ITurnContext turnContext, CancellationToken ct)
        {
            ApplySkillActivityToTurnContext(turnContext, activity);
            await _bot.OnTurnAsync(turnContext, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Log messages for <see cref="CloudSkillHandler"/>.
        /// </summary>
        /// <remarks>
        /// Messages implemented using <see cref="LoggerMessage.Define(LogLevel, EventId, string)"/> to maximize performance.
        /// For more information, see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/loggermessage?view=aspnetcore-5.0.
        /// </remarks>
        private static class Log
        {
            private static readonly Action<ILogger, Exception> _skillConversationReferenceNotImplemented =
                LoggerMessage.Define(LogLevel.Warning, new EventId(1, nameof(SkillConversationReferenceNotImplemented)), "Got NotImplementedException when trying to call GetSkillConversationReferenceAsync() on the ConversationIdFactory, attempting to use deprecated GetConversationReferenceAsync() method instead.");

            private static readonly Action<ILogger, string, Exception> _skillConversationReferenceNotFound =
                LoggerMessage.Define<string>(LogLevel.Error, new EventId(2, nameof(SkillConversationReferenceNotFound)), "Unable to get skill conversation reference for conversationId {String}.");

            public static void SkillConversationReferenceNotImplemented(ILogger logger, NotImplementedException ex) => _skillConversationReferenceNotImplemented(logger, ex);

            public static void SkillConversationReferenceNotFound(ILogger logger, string conversationId) => _skillConversationReferenceNotFound(logger, conversationId, null);
        }
    }
}
