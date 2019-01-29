using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Composition
{
    /// <summary>
    /// IntentDialog - Dispatches to Dialog based on intent out of a recognizer
    /// </summary>
    public class IntentDialog : ComponentDialog, IRecognizerDialog<IDialog>
    {
        /// <summary>
        /// Recognizer to use to get intents/entities
        /// </summary>
        public IRecognizer Recognizer { get; set; }

        /// <summary>
        /// Route of Intent -> DialogId 
        /// </summary>
        public IDictionary<string, IDialog> Routes { get; set; } = new Dictionary<string, IDialog>();


        // autoregister dialogs in the routes
        protected override Task OnInitialize(DialogContext outerDc)
        {
            foreach(var route in Routes)
            {
                var dialog = route.Value;
                if (String.IsNullOrEmpty(dialog.Id))
                {
                    dialog.Id = $"{this.Id}.{route.Key}";
                }

                if (this.FindDialog(dialog.Id) == null && outerDc.FindDialog(dialog.Id) == null)
                {
                    this.AddDialog(dialog);
                }
            }
            return base.OnInitialize(outerDc);
        }

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

            var result = await this.Recognizer.RecognizeAsync(outerDc.Context, cancellationToken);

            var topIntent = result.GetTopScoringIntent();

            // look up route
            if (Routes.TryGetValue(topIntent.intent, out IDialog dialog))
            {
                var innerDc = new DialogContext(_dialogs, outerDc, dialogState);
                return await innerDc.BeginDialogAsync(dialog.Id, null, cancellationToken).ConfigureAwait(false);
            }

            // no match
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }
    }
}
