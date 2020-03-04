// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A specialized <see cref="Dialog"/> that can wrap remote calls to a skill.
    /// </summary>
    /// <remarks>
    /// The options parameter in <see cref="BeginDialogAsync"/> must be a <see cref="SkillDialogArgs"/> instance
    /// with the initial parameters for the dialog.
    /// </remarks>
    public class SkillDialog : Dialog
    {
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

            // Send the activity to the skill.
            await SendToSkillAsync(dc.Context, skillActivity, cancellationToken).ConfigureAwait(false);
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
                // Just forward to the remote skill
                await SendToSkillAsync(dc.Context, dc.Context.Activity, cancellationToken).ConfigureAwait(false);
            }

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
        /// Validates the the required properties are set in the options argument passed to the BeginDialog call.
        /// </summary>
        private static SkillDialogArgs ValidateBeginDialogArgs(object options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!(options is SkillDialogArgs dialogArgs))
            {
                throw new ArgumentException($"Unable to cast {nameof(options)} to {nameof(SkillDialogArgs)}", nameof(options));
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

        private async Task SendToSkillAsync(ITurnContext context, Activity activity, CancellationToken cancellationToken)
        {
            // Create a conversationId to interact with the skill and send the activity
            var skillConversationId = await DialogOptions.ConversationIdFactory.CreateSkillConversationIdAsync(activity.GetConversationReference(), cancellationToken).ConfigureAwait(false);

            // Always save state before forwarding
            // (the dialog stack won't get updated with the skillDialog and things won't work if you don't)
            var skillInfo = DialogOptions.Skill;
            await DialogOptions.ConversationState.SaveChangesAsync(context, true, cancellationToken).ConfigureAwait(false);
            var response = await DialogOptions.SkillClient.PostActivityAsync(DialogOptions.BotId, skillInfo.AppId, skillInfo.SkillEndpoint, DialogOptions.SkillHostEndpoint, skillConversationId, activity, cancellationToken).ConfigureAwait(false);

            // Inspect the skill response status
            if (!(response.Status >= 200 && response.Status <= 299))
            {
                throw new HttpRequestException($"Error invoking the skill id: \"{skillInfo.Id}\" at \"{skillInfo.SkillEndpoint}\" (status is {response.Status}). \r\n {response.Body}");
            }
        }
    }
}
