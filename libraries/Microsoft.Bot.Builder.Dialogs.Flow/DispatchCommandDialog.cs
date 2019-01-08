using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Flow
{
    /// <summary>
    /// DispatchDialog - Dispatches to sub Dialog based on intent out of a recognizer
    /// </summary>
    public class DispatchDialog : ComponentDialog
    {
        /// <summary>
        /// Recognizer to use to get intents/entities
        /// </summary>
        public IRecognizer Recognizer { get; set; }

        /// <summary>
        /// Route of Intent -> DialogId 
        /// </summary>
        public Dictionary<string, IDialogCommand> Routes { get; set; } = new Dictionary<string, IDialogCommand>();

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

            var result = await this.Recognizer.RecognizeAsync(outerDc.Context, cancellationToken);

            var topIntent = result.GetTopScoringIntent();

            // look up route
            if (Routes.TryGetValue(topIntent.intent, out IDialogCommand command))
            {
                return await command.Execute(outerDc, options, null, cancellationToken);
            }

            // no match
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }
    }
}
