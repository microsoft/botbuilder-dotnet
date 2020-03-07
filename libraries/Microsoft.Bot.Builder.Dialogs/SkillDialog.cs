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
            await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: $"ActivityType: {dc.Context.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);

            // Handle EndOfConversation from the skill (this will be sent to the this dialog by the SkillHandler if received from the Skill)
            if (dc.Context.Activity.Type == ActivityTypes.EndOfConversation)
            {
                await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: $"Got {ActivityTypes.EndOfConversation}", cancellationToken: cancellationToken).ConfigureAwait(false);
                return await dc.EndDialogAsync(dc.Context.Activity.Value, cancellationToken).ConfigureAwait(false);
            }

            // Forward only Message and Event activities to the skill
            if (dc.Context.Activity.Type == ActivityTypes.Message || dc.Context.Activity.Type == ActivityTypes.Event)
            {
                // Create deep clone of the original activity to avoid altering it before forwarding it.
                var skillActivity = ObjectPath.Clone(dc.Context.Activity);
                skillActivity.DeliveryMode = dc.ActiveDialog.State[DeliverModeStateKey] as string;

                // Just forward to the remote skill
                var eocActivity = await SendToSkillAsync(dc.Context, skillActivity, cancellationToken).ConfigureAwait(false);
                if (eocActivity != null)
                {
                    return await dc.EndDialogAsync(eocActivity.Value, cancellationToken).ConfigureAwait(false);
                }
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

                await SendToSkillAsync(turnContext, activity, cancellationToken).ConfigureAwait(false);
            }

            await base.EndDialogAsync(turnContext, instance, reason, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates the required properties are set in the options argument passed to the BeginDialog call.
        /// </summary>
        private static BeginSkillDialogOptions ValidateBeginDialogArgs(object options)
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

            // Only accept Message or Event activities
            if (dialogArgs.Activity.Type != ActivityTypes.Message && dialogArgs.Activity.Type != ActivityTypes.Event)
            {
                // Just forward to the remote skill
                throw new ArgumentException($"Only {ActivityTypes.Message} and {ActivityTypes.Event} activities are supported. Received activity of type {dialogArgs.Activity.Type}.", nameof(options));
            }

            return dialogArgs;
        }

        private async Task<Activity> SendToSkillAsync(ITurnContext context, Activity activity, CancellationToken cancellationToken)
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

            // Always save state before forwarding
            // (the dialog stack won't get updated with the skillDialog and things won't work if you don't)
            var skillInfo = DialogOptions.Skill;
            await DialogOptions.ConversationState.SaveChangesAsync(context, true, cancellationToken).ConfigureAwait(false);

            var response = await DialogOptions.SkillClient.PostActivityAsync<Activity[]>(DialogOptions.BotId, skillInfo.AppId, skillInfo.SkillEndpoint, DialogOptions.SkillHostEndpoint, skillConversationId, activity, cancellationToken).ConfigureAwait(false);

            // Inspect the skill response status
            if (!(response.Status >= 200 && response.Status <= 299))
            {
                throw new HttpRequestException($"Error invoking the skill id: \"{skillInfo.Id}\" at \"{skillInfo.SkillEndpoint}\" (status is {response.Status}). \r\n {response.Body}");
            }

            Activity eocActivity = null;
            if (activity.DeliveryMode == DeliveryModes.ExpectReplies && response.Body.Any())
            {
                // Process replies in the response.Body.
                foreach (var fromSkillActivity in response.Body)
                {
                    if (fromSkillActivity.Type == ActivityTypes.EndOfConversation)
                    {
                        // Capture the EndOfConversation activity if it was sent from skill
                        eocActivity = fromSkillActivity;
                    }
                    else
                    {
                        // Send the response back to the channel. 
                        await context.SendActivityAsync(fromSkillActivity, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            return eocActivity;
        }
    }
}
