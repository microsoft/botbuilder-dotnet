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
    public class RemoteDialog : Dialog
    {
        private readonly ConversationState _conversationState;

        public RemoteDialog(ConversationState conversationState)
            : base(nameof(RemoteDialog))
        {
            _conversationState = conversationState;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            await dc.Context.SendActivityAsync("RemoteDialog: InBegin", cancellationToken: cancellationToken);
            var turnContext = dc.Context;
            AddActionToActivity(turnContext.Activity, options);

            // Send message with semantic action to the remote skill.
            return await SendToSkill(dc, turnContext.Activity, cancellationToken);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            await dc.Context.SendActivityAsync($"RemoteDialog: InContinue {dc.Context.Activity.Type}", cancellationToken: cancellationToken);
            if (dc.Context.Activity.Type == ActivityTypes.EndOfConversation)
            {
                await dc.Context.SendActivityAsync("RemoteDialog: got end of conversation", cancellationToken: cancellationToken);

                return await dc.EndDialogAsync(dc.Context.Activity.Value, cancellationToken);
            }

            // Just forward to the remote skill
            return await SendToSkill(dc, dc.Context.Activity, cancellationToken);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await dc.Context.SendActivityAsync($"RemoteDialog: InResume{dc.Context.Activity.Type}", cancellationToken: cancellationToken);
            return await base.ResumeDialogAsync(dc, reason, result, cancellationToken);
        }

        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            await turnContext.SendActivityAsync("RemoteDialog: In EndDialog", cancellationToken: cancellationToken);
            await base.EndDialogAsync(turnContext, instance, reason, cancellationToken);
        }
        
        private static void AddActionToActivity(Activity activity, object options)
        {
            // Set the action and the entities on the activity before sending it to the remote skill.
            activity.SemanticAction = new SemanticAction("BookFlight")
            {
                Entities = new Dictionary<string, Entity>
                {
                    { "bookingInfo", new Entity() },
                },
            };

            var bookingDetails = (BookingDetails)options;
            activity.SemanticAction.Entities["bookingInfo"].SetAs(bookingDetails);
        }

        private async Task<DialogTurnResult> SendToSkill(DialogContext dc, Activity activity, CancellationToken cancellationToken)
        {
            await _conversationState.SaveChangesAsync(dc.Context, true, cancellationToken);
            await dc.Context.Adapter.ForwardActivityAsync(dc.Context, "SkillBot", activity, cancellationToken);
            return EndOfTurn;
        }
    }
}
