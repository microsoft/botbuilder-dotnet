// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Base class for all dialogs.
    /// </summary>
    public abstract class Dialog
    {
        /// <summary>
        /// A <see cref="DialogTurnResult"/> that indicates that the current dialog is still
        /// active and waiting for input from the user next turn.
        /// </summary>
        public static readonly DialogTurnResult EndOfTurn = new DialogTurnResult(DialogTurnStatus.Waiting);
        private IBotTelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog"/> class.
        /// Called from constructors in derived classes to initialize the <see cref="Dialog"/> class.
        /// </summary>
        /// <param name="dialogId">The ID to assign to the new dialog.</param>
        public Dialog(string dialogId)
        {
            if (string.IsNullOrWhiteSpace(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            _telemetryClient = NullBotTelemetryClient.Instance;
            Id = dialogId;
        }

        /// <summary>
        /// Gets the ID assigned to this dialog.
        /// </summary>
        /// <value>The ID assigned to this dialog.</value>
        public string Id { get; }

        /// <summary>
        /// Gets or sets the <see cref="IBotTelemetryClient"/> to use for logging.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> to use for logging.</value>
        /// <seealso cref="DialogSet.TelemetryClient"/>
        public IBotTelemetryClient TelemetryClient
        {
            get
            {
                return _telemetryClient;
            }

            set
            {
                _telemetryClient = value;
            }
        }

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.</remarks>
        /// <seealso cref="DialogContext.BeginDialogAsync(string, object, CancellationToken)"/>
        public abstract Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Called when the dialog is _continued_, where it is the active dialog and the
        /// user replies with a new activity.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog. The result may also contain a
        /// return value.
        ///
        /// If this method is *not* overridden, the dialog automatically ends when the user replies.
        /// </remarks>
        /// <seealso cref="DialogContext.ContinueDialogAsync(CancellationToken)"/>
        public virtual async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // By default just end the current dialog.
            return await dc.EndDialogAsync(null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when a child dialog completed this turn, returning control to this dialog.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="reason">Reason why the dialog resumed.</param>
        /// <param name="result">Optional, value returned from the dialog that was called. The type
        /// of the value returned is dependent on the child dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether this dialog is still
        /// active after this dialog turn has been processed.
        ///
        /// Generally, the child dialog was started with a call to
        /// <see cref="BeginDialogAsync(DialogContext, object, CancellationToken)"/>. However, if the
        /// <see cref="DialogContext.ReplaceDialogAsync(string, object, CancellationToken)"/> method
        /// is called, the logical child dialog may be different than the original.
        ///
        /// If this method is *not* overridden, the dialog automatically ends when the user replies.
        /// </remarks>
        /// <seealso cref="DialogContext.EndDialogAsync(object, CancellationToken)"/>
        public virtual async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // By default just end the current dialog and return result to parent.
            return await dc.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when the dialog should re-prompt the user for input.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="instance">State information for this dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <seealso cref="DialogContext.RepromptDialogAsync(CancellationToken)"/>
        public virtual Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            // No-op by default
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when the dialog is ending.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="instance">State information associated with the instance of this dialog on the dialog stack.</param>
        /// <param name="reason">Reason why the dialog ended.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            // No-op by default
            return Task.CompletedTask;
        }
    }
}
