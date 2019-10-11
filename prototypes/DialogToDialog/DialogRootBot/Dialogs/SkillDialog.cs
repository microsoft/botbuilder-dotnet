// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace DialogRootBot.Dialogs
{
    public class SkillDialog : Dialog
    {
        private readonly ConversationState _conversationState;

        public SkillDialog(ConversationState conversationState)
            : base(nameof(SkillDialog))
        {
            _conversationState = conversationState;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var remoteDialogArgs = (SkillDialogArgs)options;
            await dc.Context.SendActivityAsync($"SkillDialog: InBeginDialog Action: {remoteDialogArgs.TargetAction}", cancellationToken: cancellationToken);
            var turnContext = dc.Context;
            AddActionToActivity(turnContext.Activity, remoteDialogArgs.TargetAction, remoteDialogArgs.Entities);

            // Send message with semantic action to the remote skill.
            return await SendToSkill(dc, turnContext.Activity, cancellationToken);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            await dc.Context.SendActivityAsync($"SkillDialog: InContinueDialog, ActivityType: {dc.Context.Activity.Type}", cancellationToken: cancellationToken);
            if (dc.Context.Activity.Type == ActivityTypes.EndOfConversation)
            {
                // look at the dc.Context.Activity.Code for exit status.
                await dc.Context.SendActivityAsync("SkillDialog: got EndOfConversation", cancellationToken: cancellationToken);
                return await dc.EndDialogAsync(dc.Context.Activity.Value, cancellationToken);
            }

            // Just forward to the remote skill
            return await SendToSkill(dc, dc.Context.Activity, cancellationToken);
        }

        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            await turnContext.SendActivityAsync("SkillDialog: In EndDialog", cancellationToken: cancellationToken);
            await base.EndDialogAsync(turnContext, instance, reason, cancellationToken);
        }

        private static void AddActionToActivity(Activity activity, string action, Dictionary<string, object> entities)
        {
            // Set the action and the entities on the activity before sending it to the remote skill.
            activity.SemanticAction = new SemanticAction(action);
            var entitiesList = new Dictionary<string, Entity>();
            
            foreach (var entity in entities)
            {
                var value = new Entity();
                value.SetAs(entity.Value);
                entitiesList.Add(entity.Key, value);
            }

            activity.SemanticAction.Entities = entitiesList;
        }

        private async Task<DialogTurnResult> SendToSkill(DialogContext dc, Activity activity, CancellationToken cancellationToken)
        {
            // TODO: consider having an extension method in DC that saves state for you.
            // Always save state before forwarding (things won't work if you don't)
            await _conversationState.SaveChangesAsync(dc.Context, true, cancellationToken);

            // TODO: SkillBot is hardcoded here, find a way of making it a parameter.
            await dc.Context.Adapter.ForwardActivityAsync(dc.Context, "SkillBot", activity, cancellationToken);
            return EndOfTurn;
        }
    }
}
