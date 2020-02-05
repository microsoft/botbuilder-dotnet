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
            if (!(options is SkillDialogArgs dialogArgs))
            {
                throw new ArgumentNullException(nameof(options), $"Unable to cast {nameof(options)} to {nameof(SkillDialogArgs)}");
            }

            // Store Skill information for this dialog instance
            await _activeSkillProperty.SetAsync(dc.Context, dialogArgs.Skill, cancellationToken).ConfigureAwait(false);

            await dc.Context.TraceActivityAsync($"{GetType().Name}.BeginDialogAsync()", label: $"Using activity of type: {dialogArgs.ActivityType}", cancellationToken: cancellationToken).ConfigureAwait(false);

            Activity skillActivity;
            switch (dialogArgs.ActivityType)
            {
                case ActivityTypes.Event:
                    var eventActivity = Activity.CreateEventActivity();
                    eventActivity.Name = dialogArgs.Name;
                    skillActivity = (Activity)eventActivity;
                    break;

                case ActivityTypes.Message:
                    var messageActivity = Activity.CreateMessageActivity();
                    messageActivity.Text = dialogArgs.Text;
                    skillActivity = (Activity)messageActivity;
                    break;

                default:
                    throw new ArgumentException($"Invalid activity type in {dialogArgs.ActivityType} in {nameof(SkillDialogArgs)}");
            }

            ApplyParentActivityProperties(dc, skillActivity, dialogArgs);
            await SendToSkillAsync(dc, skillActivity, dialogArgs.Skill, cancellationToken).ConfigureAwait(false);
            return EndOfTurn;
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: $"ActivityType: {dc.Context.Activity.Type}", cancellationToken: cancellationToken).ConfigureAwait(false);

            var skillInfo = await _activeSkillProperty.GetAsync(dc.Context, () => null, cancellationToken).ConfigureAwait(false);

            if (dc.Context.Activity.Type == ActivityTypes.EndOfConversation)
            {
                await dc.Context.TraceActivityAsync($"{GetType().Name}.ContinueDialogAsync()", label: "Got EndOfConversation", cancellationToken: cancellationToken).ConfigureAwait(false);
                return await dc.EndDialogAsync(dc.Context.Activity.Value, cancellationToken).ConfigureAwait(false);
            }

            if (dc.Context.Activity.Type == ActivityTypes.Message || dc.Context.Activity.Type == ActivityTypes.Event)
            {
                // Just forward to the remote skill
                await SendToSkillAsync(dc, dc.Context.Activity, skillInfo, cancellationToken).ConfigureAwait(false);
            }

            return EndOfTurn;
        }

        private static void ApplyParentActivityProperties(DialogContext dc, Activity skillActivity, SkillDialogArgs dialogArgs)
        {
            // Apply conversation reference and common properties from incoming activity before sending.
            skillActivity.ApplyConversationReference(dc.Context.Activity.GetConversationReference(), true);
            skillActivity.Value = dialogArgs.Value;
            skillActivity.ChannelData = dc.Context.Activity.ChannelData;
            skillActivity.Properties = dc.Context.Activity.Properties;
        }

        private async Task<InvokeResponse> SendToSkillAsync(DialogContext dc, Activity activity, BotFrameworkSkill skillInfo, CancellationToken cancellationToken)
        {
            // Always save state before forwarding
            // (the dialog stack won't get updated with the skillDialog and things won't work if you don't)
            await _conversationState.SaveChangesAsync(dc.Context, true, cancellationToken).ConfigureAwait(false);

            var skillConversationId = await _dialogOptions.ConversationIdFactory.CreateSkillConversationIdAsync(activity.GetConversationReference(), cancellationToken).ConfigureAwait(false);
            var response = await _dialogOptions.SkillClient.PostActivityAsync(_dialogOptions.BotId, skillInfo.AppId, skillInfo.SkillEndpoint, _dialogOptions.SkillHostEndpoint, skillConversationId, activity, cancellationToken).ConfigureAwait(false);
            if (!(response.Status >= 200 && response.Status <= 299))
            {
                throw new HttpRequestException($"Error invoking the skill id: \"{skillInfo.Id}\" at \"{skillInfo.SkillEndpoint}\" (status is {response.Status}). \r\n {response.Body}");
            }

            return response;
        }
    }
}
