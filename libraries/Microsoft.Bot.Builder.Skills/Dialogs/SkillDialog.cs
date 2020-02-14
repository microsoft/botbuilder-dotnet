// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.Skills.Dialogs;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A sample dialog that can wrap remote calls to a skill.
    /// </summary>
    /// <remarks>
    /// The options parameter in <see cref="BeginDialogAsync"/> must be a <see cref="SkillDialogArgs"/> instance
    /// with the initial parameters for the dialog.
    /// </remarks>
    public class SkillDialog : Dialog
    {
        private readonly IStatePropertyAccessor<BotFrameworkSkill> _activeSkillProperty;
        private readonly ConversationState _conversationState;
        private readonly SkillDialogOptions _dialogOptions;

        public SkillDialog(SkillDialogOptions dialogOptions, ConversationState conversationState)
            : base(nameof(SkillDialog))
        {
            _dialogOptions = dialogOptions ?? throw new ArgumentNullException(nameof(dialogOptions));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _activeSkillProperty = conversationState.CreateProperty<BotFrameworkSkill>($"{typeof(SkillDialog).FullName}.ActiveSkillProperty");
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var dialogArgs = ValidateBeginDialogOptions(options);

            await dc.Context.TraceActivityAsync($"{GetType().Name}.BeginDialogAsync()", label: $"Using activity of type: {dialogArgs.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);

            // Store Skill information for this dialog instance
            await _activeSkillProperty.SetAsync(dc.Context, dialogArgs.Skill, cancellationToken).ConfigureAwait(false);

            // Create deep clone of the original activity to avoid altering it before forwarding it.
            var skillActivity = ObjectPath.Clone(dialogArgs.Activity);

            // Apply conversation reference and common properties from incoming activity before sending.
            skillActivity.ApplyConversationReference(dc.Context.Activity.GetConversationReference(), true);

            // Send the activity to the skill.
            await SendToSkillAsync(dc, skillActivity, dialogArgs.Skill, cancellationToken).ConfigureAwait(false);
            return EndOfTurn;
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: $"ActivityType: {dc.Context.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);

            // Retrieve the current skill information from ConversationState
            var skillInfo = await _activeSkillProperty.GetAsync(dc.Context, () => null, cancellationToken).ConfigureAwait(false);

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
                await SendToSkillAsync(dc, dc.Context.Activity, skillInfo, cancellationToken).ConfigureAwait(false);
            }

            return EndOfTurn;
        }

        private static SkillDialogArgs ValidateBeginDialogOptions(object options)
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

        private async Task SendToSkillAsync(DialogContext dc, Activity activity, BotFrameworkSkill skillInfo, CancellationToken cancellationToken)
        {
            // Always save state before forwarding
            // (the dialog stack won't get updated with the skillDialog and things won't work if you don't)
            await _conversationState.SaveChangesAsync(dc.Context, true, cancellationToken).ConfigureAwait(false);

            // Create a conversationId to interact with the skill and send the activity
            var skillConversationId = await _dialogOptions.ConversationIdFactory.CreateSkillConversationIdAsync(activity.GetConversationReference(), cancellationToken).ConfigureAwait(false);
            var response = await _dialogOptions.SkillClient.PostActivityAsync(_dialogOptions.BotId, skillInfo.AppId, skillInfo.SkillEndpoint, _dialogOptions.SkillHostEndpoint, skillConversationId, activity, cancellationToken).ConfigureAwait(false);

            // Inspect the skill response status
            if (!(response.Status >= 200 && response.Status <= 299))
            {
                throw new HttpRequestException($"Error invoking the skill id: \"{skillInfo.Id}\" at \"{skillInfo.SkillEndpoint}\" (status is {response.Status}). \r\n {response.Body}");
            }
        }
    }
}
