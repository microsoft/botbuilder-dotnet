// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

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
            var dialogSet = new DialogSet(accessor);
            dialogSet.TelemetryClient = dialog.TelemetryClient;
            dialogSet.Add(dialog);

            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken).ConfigureAwait(false);
            var results = await dialogContext.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);
            if (results.Status == DialogTurnStatus.Empty)
            {
                await dialogContext.BeginDialogAsync(dialog.Id, null, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
