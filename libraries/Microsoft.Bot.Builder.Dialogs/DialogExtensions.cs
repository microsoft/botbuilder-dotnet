// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Provides extension methods for <see cref="Dialog"/> and derived classes.
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

            // Handle EoC and Reprompt event from a parent bot (can be root bot to skill or skill to skill)
            if (IsFromParentToSkill(turnContext))
            {
                // Handle remote cancellation request from parent.
                if (turnContext.Activity.Type == ActivityTypes.EndOfConversation)
                {
                    if (!dialogContext.Stack.Any())
                    {
                        // No dialogs to cancel, just return.
                        return;
                    }

                    var activeDialogContext = GetActiveDialogContext(dialogContext);

                    var remoteCancelText = "Skill was canceled through an EndOfConversation activity from the parent.";
                    await turnContext.TraceActivityAsync($"{typeof(Dialog).Name}.RunAsync()", label: $"{remoteCancelText}", cancellationToken: cancellationToken).ConfigureAwait(false);

                    // Send cancellation message to the top dialog in the stack to ensure all the parents are canceled in the right order. 
                    await activeDialogContext.CancelAllDialogsAsync(true, cancellationToken: cancellationToken).ConfigureAwait(false);

                    return;
                }

                // Handle a reprompt event sent from the parent.
                if (turnContext.Activity.Type == ActivityTypes.Event && turnContext.Activity.Name == DialogEvents.RepromptDialog)
                {
                    if (!dialogContext.Stack.Any())
                    {
                        // No dialogs to reprompt, just return.
                        return;
                    }

                    await dialogContext.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);
                    return;
                }
            }

            // Continue or start the dialog.
            var result = await dialogContext.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
            if (result.Status == DialogTurnStatus.Empty)
            {
                result = await dialogContext.BeginDialogAsync(dialog.Id, null, cancellationToken).ConfigureAwait(false);
            }

            // Skills should send EoC when the dialog completes.
            if (result.Status == DialogTurnStatus.Complete || result.Status == DialogTurnStatus.Cancelled)
            {
                if (SendEoCToParent(turnContext))
                {
                    var endMessageText = $"Dialog {dialog.Id} has **completed**. Sending EndOfConversation.";
                    await turnContext.TraceActivityAsync($"{typeof(Dialog).Name}.RunAsync()", label: $"{endMessageText}", value: result.Result, cancellationToken: cancellationToken).ConfigureAwait(false);

                    // Send End of conversation at the end.
                    var activity = new Activity(ActivityTypes.EndOfConversation) { Value = result.Result };
                    await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Helper to determine if we should send an EoC to the parent or not.
        /// </summary>
        private static bool SendEoCToParent(ITurnContext turnContext)
        {
            if (turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity claimIdentity && SkillValidation.IsSkillClaim(claimIdentity.Claims))
            {
                // EoC Activities returned by skills are bounced back to the bot by SkillHandler.
                // In those cases we will have a SkillConversationReference instance in state.
                var skillConversationReference = turnContext.TurnState.Get<SkillConversationReference>(SkillHandler.SkillConversationReferenceKey);
                if (skillConversationReference != null)
                {
                    // If the skillConversationReference.OAuthScope is for one of the supported channels, we are at the root and we should not send an EoC.
                    return skillConversationReference.OAuthScope != AuthenticationConstants.ToChannelFromBotOAuthScope && skillConversationReference.OAuthScope != GovernmentAuthenticationConstants.ToChannelFromBotOAuthScope;
                }

                return true;
            }

            return false;
        }

        private static bool IsFromParentToSkill(ITurnContext turnContext)
        {
            if (turnContext.TurnState.Get<SkillConversationReference>(SkillHandler.SkillConversationReferenceKey) != null)
            {
                return false;
            }

            return turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity claimIdentity && SkillValidation.IsSkillClaim(claimIdentity.Claims);
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
