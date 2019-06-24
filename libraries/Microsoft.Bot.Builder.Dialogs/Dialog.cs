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
        public static readonly DialogTurnResult EndOfTurn = new DialogTurnResult(DialogTurnStatus.Waiting);
        private IBotTelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog"/> class.
        /// Called from constructors in derived classes to initialize the <see cref="Dialog"/> class.
        /// </summary>
        /// <param name="dialogId">The ID to assign to this dialog.</param>
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
        /// Gets or sets the telemetry client for logging events.
        /// </summary>
        /// <value>The Telemetry Client logger.</value>
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
        /// Method called when a new dialog has been pushed onto the stack and is being activated.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="options">(Optional) additional argument(s) to pass to the dialog being started.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.</remarks>
        public abstract Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Method called when an instance of the dialog is the "current" dialog and the
        /// user replies with a new activity. The dialog will generally continue to receive the user's
        /// replies until it calls either `EndDialogAsync()` or `BeginDialogAsync()`.
        /// If this method is NOT implemented then the dialog will automatically be ended when the user replies.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.</remarks>
        public virtual async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // By default just end the current dialog.
            return await dc.EndDialogAsync(null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Method called when an instance of the dialog is being returned to from another
        /// dialog that was started by the current instance using `BeginDialogAsync()`.
        /// If this method is NOT implemented then the dialog will be automatically ended with a call
        /// to `EndDialogAsync()`. Any result passed from the called dialog will be passed
        /// to the current dialog's parent.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="reason">Reason why the dialog resumed.</param>
        /// <param name="result">(Optional) value returned from the dialog that was called. The type of the value returned is dependent on the dialog that was called.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.</remarks>
        public virtual async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // By default just end the current dialog and return result to parent.
            return await dc.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Method called when the dialog has been requested to re-prompt the user for input.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation with the user.</param>
        /// <param name="instance">The instance of the dialog on the stack.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            // No-op by default
            return Task.CompletedTask;
        }

        public virtual Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            // No-op by default
            return Task.CompletedTask;
        }
    }
}
