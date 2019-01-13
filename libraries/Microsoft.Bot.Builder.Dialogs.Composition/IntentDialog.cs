using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Composition
{
    /// <summary>
    /// IntentDialog - Dispatches to Dialog based on intent out of a recognizer
    /// </summary>
    public class IntentDialog : ComponentDialog, IRecognizerDialog
    {
        /// <summary>
        /// Recognizer to use to get intents/entities
        /// </summary>
        public IRecognizer Recognizer { get; set; }

        /// <summary>
        /// Route of Intent -> DialogId 
        /// </summary>
        public Dictionary<string, string> Routes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Use recognizer intent to invoke sub dialog
        /// </summary>
        /// <param name="outerDc"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.Recognizer == null)
            {
                throw new ArgumentNullException("Recognizer");
            }

            var dialogState = new DialogState();
            outerDc.ActiveDialog.State[PersistedDialogState] = dialogState;

            var innerDc = new DialogContext(_dialogs, outerDc.Context, dialogState);

            var result = await this.Recognizer.RecognizeAsync(outerDc.Context, cancellationToken);

            var topIntent = result.GetTopScoringIntent();

            // look up route
            if (Routes.TryGetValue(topIntent.intent, out string dialogId))
            {
                return await innerDc.BeginDialogAsync(dialogId, null, cancellationToken).ConfigureAwait(false);
            }

            // no match
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }
    }
}
