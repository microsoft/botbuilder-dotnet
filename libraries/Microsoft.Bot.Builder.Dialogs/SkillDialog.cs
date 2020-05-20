// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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
        private const string SkillConversationIdPath = "this.skillConversationReferenceId";

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

            // create skillConversationReference record.
            var skillConversationId = await CreateSkillConversationIdAsync(dc.Context, skillActivity, cancellationToken).ConfigureAwait(false);
            dc.State.SetValue(SkillConversationIdPath, skillConversationId);

            // Send the activity to the skill.
            var eocActivity = await SendToSkillAsync(dc, skillConversationId, skillActivity, cancellationToken).ConfigureAwait(false);

            // if it has a EndOfConversationActivity then it has ended
            if (eocActivity != null)
            {
                // end dialog the eocActivity.Value as result
                return await dc.EndDialogAsync(eocActivity.Value, cancellationToken).ConfigureAwait(false);
            }

            // conversation is not done, signal to continue calling us so we can route to skill.
            return EndOfTurn;
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            if (!OnValidateActivity(dc.Context.Activity))
            {
                return EndOfTurn;
            }

            await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: $"ActivityType: {dc.Context.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);

            // Create deep clone of the original activity to avoid altering it before forwarding it.
            var skillActivity = ObjectPath.Clone(dc.Context.Activity);
            skillActivity.DeliveryMode = dc.ActiveDialog.State[DeliverModeStateKey] as string;

            var skillConversationId = dc.State.GetValue<string>(SkillConversationIdPath);

            // Just forward to the remote skill
            var eocActivity = await SendToSkillAsync(dc, skillConversationId, skillActivity, cancellationToken).ConfigureAwait(false);

            // if skill sent back a EndOfConversationActivity then dialog has ended
            if (eocActivity != null)
            {
                // end dialog with the eocActivity.value
                return await dc.EndDialogAsync(eocActivity.Value, cancellationToken).ConfigureAwait(false);
            }

            // if this activity is actually a EOC then we got it from a channel
            if (dc.Context.Activity.Type == ActivityTypes.EndOfConversation)
            {
                // clean up and end dialog. Skill should have been told we are ending, so we can delete record.
                await DialogOptions.ConversationIdFactory.DeleteConversationReferenceAsync(skillConversationId, cancellationToken).ConfigureAwait(false);
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // end turn so next activity will be routed to the skill.
            return EndOfTurn;
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            // NOTE: this is handled by OnPreBubbleEvent() so we have dc.
            await dc.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);
            return EndOfTurn;
        }

        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default)
        {
            var skillConversationId = instance.State[SkillConversationIdPath.Replace("this.", string.Empty)].ToString();

            // Send EndOfConversation to the skill if the dialog has been cancelled. 
            if (reason == DialogReason.CancelCalled || reason == DialogReason.ReplaceCalled)
            {
                await turnContext.TraceActivityAsync($"{GetType().Name}.EndDialogAsync()", label: $"ActivityType: {turnContext.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);
                var activity = (Activity)Activity.CreateEndOfConversationActivity();

                // Apply conversation reference and common properties from incoming activity before sending.
                activity.ApplyConversationReference(turnContext.Activity.GetConversationReference(), true);
                activity.ChannelData = turnContext.Activity.ChannelData;
                activity.Properties = turnContext.Activity.Properties;

                // connection Name is not applicable for an EndDialog, as we don't expect as OAuthCard in response.

                // send directly (don't use SendToSkill) because we only have a turn context and we don't care about responses to end 
                await DialogOptions.SkillClient.PostActivityAsync<ExpectedReplies>(DialogOptions.BotId, DialogOptions.Skill.AppId, DialogOptions.Skill.SkillEndpoint, DialogOptions.SkillHostEndpoint, skillConversationId, activity, cancellationToken).ConfigureAwait(false);
            }

            // remove SkillConversationReference record.
            await DialogOptions.ConversationIdFactory.DeleteConversationReferenceAsync(skillConversationId, cancellationToken).ConfigureAwait(false);

            await base.EndDialogAsync(turnContext, instance, reason, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<bool> OnPreBubbleEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            // look for dialogEvents.RepromptDialog so we have a Dc based RepromptDialog
            if (e.Name == DialogEvents.RepromptDialog)
            {
                // Create and send an event to the skill so it can resume the dialog.
                var repromptEvent = Activity.CreateEventActivity();
                repromptEvent.Name = DialogEvents.RepromptDialog;

                // Apply conversation reference and common properties from incoming activity before sending.
                repromptEvent.ApplyConversationReference(dc.Context.Activity.GetConversationReference(), true);

                // connection Name is not applicable for a RePrompt, as we don't expect as OAuthCard in response.

                var skillConversationId = dc.State.GetValue<string>(SkillConversationIdPath);

                await SendToSkillAsync(dc, skillConversationId, (Activity)repromptEvent, cancellationToken).ConfigureAwait(false);

                // handled
                return true;
            }

            return await base.OnPreBubbleEventAsync(dc, e, cancellationToken).ConfigureAwait(false);
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

        private async Task<Activity> SendToSkillAsync(DialogContext dc, string skillConversationReferenceId, Activity activity, CancellationToken cancellationToken)
        {
            if (activity.Type == ActivityTypes.Invoke)
            {
                // Force ExpectReplies for invoke activities so we can get the replies right away and send them back to the channel if needed.
                // This makes sure that the dialog will receive the Invoke response from the skill and any other activities sent, including EoC.
                activity.DeliveryMode = DeliveryModes.ExpectReplies;
            }

            // Always save state before forwarding
            // (the dialog stack won't get updated with the skillDialog and things won't work if you don't)
            await dc.State.SaveAllChangesAsync(cancellationToken).ConfigureAwait(false);

            // if we are async mode then we mark SkillConversationReference that this skillHost is waiting on the skill to complete the operation.
            var skillConversationReference = await DialogOptions.ConversationIdFactory.GetSkillConversationReferenceAsync(skillConversationReferenceId, cancellationToken).ConfigureAwait(false);
            if (activity.DeliveryMode != DeliveryModes.ExpectReplies)
            {
                skillConversationReference.SkillHostWaiting = true;
            }
            else
            {
                skillConversationReference.SkillHostWaiting = false;
            }

            await DialogOptions.ConversationIdFactory.SaveSkillConversationReferenceAsync(skillConversationReference, cancellationToken).ConfigureAwait(false);

            var skillInfo = DialogOptions.Skill;
            var response = await DialogOptions.SkillClient.PostActivityAsync<ExpectedReplies>(DialogOptions.BotId, skillInfo.AppId, skillInfo.SkillEndpoint, DialogOptions.SkillHostEndpoint, skillConversationReferenceId, activity, cancellationToken).ConfigureAwait(false);

            // Inspect the skill response status
            if (!response.IsSuccessStatusCode())
            {
                throw new HttpRequestException($"Error invoking the skill id: \"{skillInfo.Id}\" at \"{skillInfo.SkillEndpoint}\" (status is {response.Status}). \r\n {response.Body}");
            }

            List<Activity> activitiesFromSkill = null;
            if (activity.DeliveryMode == DeliveryModes.ExpectReplies && response.Body.Activities != null)
            {
                // activitiesFromSkill came as inline response body.
                activitiesFromSkill = response.Body.Activities.ToList();
            }
            else
            {
                // we are async mode then get SKillConversationReference (it could be different)
                skillConversationReference = await DialogOptions.ConversationIdFactory.GetSkillConversationReferenceAsync(skillConversationReferenceId, cancellationToken).ConfigureAwait(false);

                // we mark SkillConversationReference that this skillHost is no longer waiting on the skill to complete the operation.
                skillConversationReference.SkillHostWaiting = false;

                if (skillConversationReference.Activities.Any())
                {
                    activitiesFromSkill = new List<Activity>(skillConversationReference.Activities);
                    skillConversationReference.Activities.Clear();
                }

                // save updated record.
                await DialogOptions.ConversationIdFactory.SaveSkillConversationReferenceAsync(skillConversationReference, cancellationToken).ConfigureAwait(false);
            }

            // if we have any skill activities to process in this turn context.
            if (activitiesFromSkill != null && activitiesFromSkill.Any())
            {
                var changesPending = false;

                foreach (var activityFromSkill in activitiesFromSkill)
                {
                    if (activityFromSkill.Type == ActivityTypes.EndOfConversation)
                    {
                        // return the EndOfConversation activity, we are done.
                        return activityFromSkill;
                    }

                    if (activityFromSkill.Type == ActivityTypes.Event)
                    {
                        // emit event to accumulate plan changes.
                        if (await dc.EmitEventAsync(DialogEvents.ActivityReceived, activityFromSkill, cancellationToken: cancellationToken).ConfigureAwait(false))
                        {
                            changesPending = true;
                        }
                    }
                    else if (await InterceptOAuthCardsAsync(dc.Context, activityFromSkill, DialogOptions.ConnectionName, cancellationToken).ConfigureAwait(false))
                    {
                        // do nothing. Token exchange succeeded, so no oauthcard needs to be shown to the user
                    }
                    else
                    {
                        // capture value from InvokeResponse 
                        if (activityFromSkill.Type == ActivityTypesEx.InvokeResponse && activityFromSkill.Value is JObject jObject)
                        {
                            // Ensure the value in the invoke response is of type InvokeResponse (it gets deserialized as JObject by default).
                            activityFromSkill.Value = jObject.ToObject<InvokeResponse>();
                        }

                        // Send the response back to the channel. 
                        await dc.Context.SendActivityAsync(activityFromSkill, cancellationToken).ConfigureAwait(false);
                    }
                }

                // If we have changesPending then we need to call rootDc.ContinueDialog()
                if (changesPending)
                {
                    // find root
                    var rootDc = dc;
                    while (rootDc.Parent != null)
                    {
                        rootDc = rootDc.Parent;
                    }

                    // call rootDC to continue (to handle any plan changes)
                    if (rootDc != null)
                    {
                        // call ContinueDialogAsync() to apply changes and continue execution.
                        await rootDc.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            return null;
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
