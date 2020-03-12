// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Provides additional, `static` (Shared in Visual Basic) methods for <see cref="Dialog"/> and
    /// derived classes.
    /// </summary>
    public static class DialogExtensions
    {
        /// <summary>
        /// Creates a dialog stack and starts a dialog, pushing it onto the stack.
        /// </summary>
        /// <param name="dialog">The dialog to start.</param>
        /// <param name="turnContext">The context for the current turn of the conversation.</param>
        /// <param name="accessor">The <see cref="IStatePropertyAccessor{DialogState}"/> accessor
        /// with which to manage the state of the dialog stack.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RunAsync(this Dialog dialog, ITurnContext turnContext, IStatePropertyAccessor<DialogState> accessor, CancellationToken cancellationToken)
        {
            var dialogSet = new DialogSet(accessor) { TelemetryClient = dialog.TelemetryClient };
            dialogSet.Add(dialog);

            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken).ConfigureAwait(false);

            if (turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity claimIdentity && SkillValidation.IsSkillClaim(claimIdentity.Claims))
            {
                // The bot is running as a skill.
                if (turnContext.Activity.Type == ActivityTypes.EndOfConversation && dialogContext.Stack.Any() && IsEocComingFromParent(turnContext))
                {
                    // Handle remote cancellation request from parent.
                    var activeDialogContext = GetActiveDialogContext(dialogContext);

                    var remoteCancelText = "Skill was canceled through an EndOfConversation activity from the parent.";
                    await turnContext.TraceActivityAsync($"{typeof(Dialog).Name}.RunAsync()", label: $"{remoteCancelText}", cancellationToken: cancellationToken).ConfigureAwait(false);

                    // Send cancellation message to the top dialog in the stack to ensure all the parents are canceled in the right order. 
                    await activeDialogContext.CancelAllDialogsAsync(true, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // Process a reprompt event sent from the parent.
                    if (turnContext.Activity.Type == ActivityTypes.Event && turnContext.Activity.Name == DialogEvents.RepromptDialog && dialogContext.Stack.Any())
                    {
                        await dialogContext.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    // Run the Dialog with the new message Activity and capture the results so we can send end of conversation if needed.
                    var result = await dialogContext.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
                    if (result.Status == DialogTurnStatus.Empty)
                    {
                        var startMessageText = $"Starting {dialog.Id}.";
                        await turnContext.TraceActivityAsync($"{typeof(Dialog).Name}.RunAsync()", label: $"{startMessageText}", cancellationToken: cancellationToken).ConfigureAwait(false);
                        result = await dialogContext.BeginDialogAsync(dialog.Id, null, cancellationToken).ConfigureAwait(false);
                    }

                    // Send end of conversation if it is completed or cancelled.
                    if (result.Status == DialogTurnStatus.Complete || result.Status == DialogTurnStatus.Cancelled)
                    {
                        var endMessageText = $"Dialog {dialog.Id} has **completed**. Sending EndOfConversation.";
                        await turnContext.TraceActivityAsync($"{typeof(Dialog).Name}.RunAsync()", label: $"{endMessageText}", value: result.Result, cancellationToken: cancellationToken).ConfigureAwait(false);

                        // Send End of conversation at the end.
                        var activity = new Activity(ActivityTypes.EndOfConversation) { Value = result.Result };
                        await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                // The bot is running as a standard bot.
                var results = await dialogContext.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync(dialog.Id, null, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        // We should only cancel the current dialog stack if the EoC activity is coming from a parent (a root bot or another skill).
        // When the EoC is coming back from a child, we should just process that EoC normally through the 
        // dialog stack and let the child dialogs handle that.
        private static bool IsEocComingFromParent(ITurnContext turnContext)
        {
            // To determine the direction we check callerId property which is set to the parent bot
            // by the BotFrameworkHttpClient on outgoing requests.
            return !string.IsNullOrWhiteSpace(turnContext.Activity.CallerId);
        }

        // Recursively walk up the DC stack to find the active DC.
        private static DialogContext GetActiveDialogContext(DialogContext dialogContext)
        {
            var child = dialogContext.Child;
            if (child == null)
            {
                return dialogContext;
            }

            return GetActiveDialogContext(child);
        }
    }
}
