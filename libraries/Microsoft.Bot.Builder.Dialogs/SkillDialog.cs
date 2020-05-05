// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A specialized <see cref="Dialog"/> that can wrap remote calls to a skill.
    /// </summary>
    /// <remarks>
    /// The options parameter in <see cref="BeginDialogAsync"/> must be a <see cref="BeginSkillDialogOptions"/> instance
    /// with the initial parameters for the dialog.
    /// </remarks>
    public class SkillDialog : Dialog
    {
        private const string DeliverModeStateKey = "deliverymode";

        public SkillDialog(SkillDialogOptions dialogOptions, string dialogId = null)
            : base(dialogId)
        {
            DialogOptions = dialogOptions ?? throw new ArgumentNullException(nameof(dialogOptions));
        }

        protected SkillDialogOptions DialogOptions { get; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var dialogArgs = ValidateBeginDialogArgs(options);

            await dc.Context.TraceActivityAsync($"{GetType().Name}.BeginDialogAsync()", label: $"Using activity of type: {dialogArgs.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);

            // Create deep clone of the original activity to avoid altering it before forwarding it.
            var skillActivity = ObjectPath.Clone(dialogArgs.Activity);

            // Apply conversation reference and common properties from incoming activity before sending.
            skillActivity.ApplyConversationReference(dc.Context.Activity.GetConversationReference(), true);

            // Store delivery mode and connection name in dialog state for later use.
            dc.ActiveDialog.State[DeliverModeStateKey] = dialogArgs.Activity.DeliveryMode;

            // Send the activity to the skill.
            var eocActivity = await SendToSkillAsync(dc.Context, skillActivity, cancellationToken).ConfigureAwait(false);
            if (eocActivity != null)
            {
                return await dc.EndDialogAsync(eocActivity.Value, cancellationToken).ConfigureAwait(false);
            }

            return EndOfTurn;
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            if (!OnValidateActivity(dc.Context.Activity))
            {
                return EndOfTurn;
            }

            await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: $"ActivityType: {dc.Context.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);

            // Handle EndOfConversation from the skill (this will be sent to the this dialog by the SkillHandler if received from the Skill)
            if (dc.Context.Activity.Type == ActivityTypes.EndOfConversation)
            {
                await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: $"Got {ActivityTypes.EndOfConversation}", cancellationToken: cancellationToken).ConfigureAwait(false);
                return await dc.EndDialogAsync(dc.Context.Activity.Value, cancellationToken).ConfigureAwait(false);
            }

            // Create deep clone of the original activity to avoid altering it before forwarding it.
            var skillActivity = ObjectPath.Clone(dc.Context.Activity);

            skillActivity.DeliveryMode = dc.ActiveDialog.State[DeliverModeStateKey] as string;

            // Just forward to the remote skill
            var eocActivity = await SendToSkillAsync(dc.Context, skillActivity, cancellationToken).ConfigureAwait(false);
            if (eocActivity != null)
            {
                return await dc.EndDialogAsync(eocActivity.Value, cancellationToken).ConfigureAwait(false);
            }

            return EndOfTurn;
        }

        public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default)
        {
            // Create and send an envent to the skill so it can resume the dialog.
            var repromptEvent = Activity.CreateEventActivity();
            repromptEvent.Name = DialogEvents.RepromptDialog;

            // Apply conversation reference and common properties from incoming activity before sending.
            repromptEvent.ApplyConversationReference(turnContext.Activity.GetConversationReference(), true);

            // connection Name is not applicable for a RePrompt, as we don't expect as OAuthCard in response.
            await SendToSkillAsync(turnContext, (Activity)repromptEvent, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            await RepromptDialogAsync(dc.Context, dc.ActiveDialog, cancellationToken).ConfigureAwait(false);
            return EndOfTurn;
        }

        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default)
        {
            // Send of of conversation to the skill if the dialog has been cancelled. 
            if (reason == DialogReason.CancelCalled || reason == DialogReason.ReplaceCalled)
            {
                await turnContext.TraceActivityAsync($"{GetType().Name}.EndDialogAsync()", label: $"ActivityType: {turnContext.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);
                var activity = (Activity)Activity.CreateEndOfConversationActivity();

                // Apply conversation reference and common properties from incoming activity before sending.
                activity.ApplyConversationReference(turnContext.Activity.GetConversationReference(), true);
                activity.ChannelData = turnContext.Activity.ChannelData;
                activity.Properties = turnContext.Activity.Properties;

                // connection Name is not applicable for an EndDialog, as we don't expect as OAuthCard in response.
                await SendToSkillAsync(turnContext, activity, cancellationToken).ConfigureAwait(false);
            }

            await base.EndDialogAsync(turnContext, instance, reason, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates the activity sent during <see cref="ContinueDialogAsync"/>.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> for the current turn of conversation.</param>
        /// <remarks>
        /// Override this method to implement a custom validator for the activity being sent during the <see cref="ContinueDialogAsync"/>.
        /// This method can be used to ignore activities of a certain type if needed.
        /// If this method returns false, the dialog will end the turn without processing the activity. 
        /// </remarks>
        /// <returns>true if the activity is valid, false if not.</returns>
        protected virtual bool OnValidateActivity(Activity activity)
        {
            return true;
        }

        /// <summary>
        /// Validates the required properties are set in the options argument passed to the BeginDialog call.
        /// </summary>
        private BeginSkillDialogOptions ValidateBeginDialogArgs(object options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!(options is BeginSkillDialogOptions dialogArgs))
            {
                throw new ArgumentException($"Unable to cast {nameof(options)} to {nameof(BeginSkillDialogOptions)}", nameof(options));
            }

            if (dialogArgs.Activity == null)
            {
                throw new ArgumentNullException(nameof(options), $"{nameof(dialogArgs.Activity)} is null in {nameof(options)}");
            }

            return dialogArgs;
        }

        private async Task<Activity> SendToSkillAsync(ITurnContext context, Activity activity, CancellationToken cancellationToken)
        {
            if (activity.Type == ActivityTypes.Invoke)
            {
                // Force ExpectReplies for invoke activities so we can get the replies right away and send them back to the channel if needed.
                // This makes sure that the dialog will receive the Invoke response from the skill and any other activities sent, including EoC.
                activity.DeliveryMode = DeliveryModes.ExpectReplies;
            }

            var skillConversationId = await CreateSkillConversationIdAsync(context, activity, cancellationToken).ConfigureAwait(false);

            // Always save state before forwarding
            // (the dialog stack won't get updated with the skillDialog and things won't work if you don't)
            await DialogOptions.ConversationState.SaveChangesAsync(context, true, cancellationToken).ConfigureAwait(false);

            var skillInfo = DialogOptions.Skill;
            var response = await DialogOptions.SkillClient.PostActivityAsync<ExpectedReplies>(DialogOptions.BotId, skillInfo.AppId, skillInfo.SkillEndpoint, DialogOptions.SkillHostEndpoint, skillConversationId, activity, cancellationToken).ConfigureAwait(false);

            // Inspect the skill response status
            if (!response.IsSuccessStatusCode())
            {
                throw new HttpRequestException($"Error invoking the skill id: \"{skillInfo.Id}\" at \"{skillInfo.SkillEndpoint}\" (status is {response.Status}). \r\n {response.Body}");
            }

            Activity eocActivity = null;
            if (activity.DeliveryMode == DeliveryModes.ExpectReplies && response.Body.Activities != null && response.Body.Activities.Any())
            {
                // Process replies in the response.Body.
                foreach (var activityFromSkill in response.Body.Activities)
                {
                    if (activityFromSkill.Type == ActivityTypes.EndOfConversation)
                    {
                        // Capture the EndOfConversation activity if it was sent from skill
                        eocActivity = activityFromSkill;
                    }
                    else if (await InterceptOAuthCardsAsync(context, activityFromSkill, DialogOptions.ConnectionName, cancellationToken).ConfigureAwait(false))
                    {
                        // do nothing. Token exchange succeeded, so no oauthcard needs to be shown to the user
                    }
                    else
                    {
                        if (activityFromSkill.Type == ActivityTypesEx.InvokeResponse && activityFromSkill.Value is JObject jObject)
                        {
                            // Ensure the value in the invoke response is of type InvokeResponse (it gets deserialized as JObject by default).
                            activityFromSkill.Value = jObject.ToObject<InvokeResponse>();
                        }

                        // Send the response back to the channel. 
                        await context.SendActivityAsync(activityFromSkill, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            return eocActivity;
        }

        /// <summary>
        /// Tells is if we should intercept the OAuthCard message.
        /// </summary>
        /// <remarks>
        /// The SkillDialog only attempts to intercept OAuthCards when the following criteria are met:
        /// 1. An OAuthCard was sent from the skill
        /// 2. The SkillDialog was called with a connectionName
        /// 3. The current adapter supports token exchange
        /// If any of these criteria are false, return false.
        /// </remarks>
        private async Task<bool> InterceptOAuthCardsAsync(ITurnContext turnContext, Activity activity, string connectionName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(connectionName) || !(turnContext.Adapter is IExtendedUserTokenProvider tokenExchangeProvider))
            {
                // The adapter may choose not to support token exchange, in which case we fallback to showing an oauth card to the user.
                return false;
            }

            var oauthCardAttachment = activity.Attachments?.FirstOrDefault(a => a?.ContentType == OAuthCard.ContentType);
            if (oauthCardAttachment != null)
            {
                var oauthCard = ((JObject)oauthCardAttachment.Content).ToObject<OAuthCard>();
                if (!string.IsNullOrWhiteSpace(oauthCard?.TokenExchangeResource?.Uri))
                {
                    try
                    {
                        var result = await tokenExchangeProvider.ExchangeTokenAsync(
                            turnContext,
                            connectionName,
                            turnContext.Activity.From.Id,
                            new TokenExchangeRequest(oauthCard.TokenExchangeResource.Uri),
                            cancellationToken).ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(result?.Token))
                        {
                            // If token above is null, then SSO has failed and hence we return false.
                            // If not, send an invoke to the skill with the token. 
                            return await SendTokenExchangeInvokeToSkillAsync(activity, oauthCard.TokenExchangeResource.Id, oauthCard.ConnectionName, result.Token, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch
                    {
                        // Failures in token exchange are not fatal. They simply mean that the user needs to be shown the OAuth card.
                        return false;
                    }
                }
            }

            return false;
        }

        private async Task<bool> SendTokenExchangeInvokeToSkillAsync(Activity incomingActivity, string id, string connectionName, string token, CancellationToken cancellationToken)
        {
            var activity = incomingActivity.CreateReply();
            activity.Type = ActivityTypes.Invoke;
            activity.Name = SignInConstants.TokenExchangeOperationName;
            activity.Value = new TokenExchangeInvokeRequest
            {
                Id = id,
                Token = token,
                ConnectionName = connectionName
            };

            // route the activity to the skill
            var skillInfo = DialogOptions.Skill;
            var response = await DialogOptions.SkillClient.PostActivityAsync<ExpectedReplies>(DialogOptions.BotId, skillInfo.AppId, skillInfo.SkillEndpoint, DialogOptions.SkillHostEndpoint, incomingActivity.Conversation.Id, activity, cancellationToken).ConfigureAwait(false);

            // Check response status: true if success, false if failure
            return response.IsSuccessStatusCode();
        }

        private async Task<string> CreateSkillConversationIdAsync(ITurnContext context, Activity activity, CancellationToken cancellationToken)
        {
            // Create a conversationId to interact with the skill and send the activity
            var conversationIdFactoryOptions = new SkillConversationIdFactoryOptions
            {
                FromBotOAuthScope = context.TurnState.Get<string>(BotAdapter.OAuthScopeKey),
                FromBotId = DialogOptions.BotId,
                Activity = activity,
                BotFrameworkSkill = DialogOptions.Skill
            };
            var skillConversationId = await DialogOptions.ConversationIdFactory.CreateSkillConversationIdAsync(conversationIdFactoryOptions, cancellationToken).ConfigureAwait(false);
            return skillConversationId;
        }
    }
}
