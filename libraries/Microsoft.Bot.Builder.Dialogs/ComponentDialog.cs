// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A <see cref="Dialog"/> that is composed of other dialogs.
    /// </summary>
    /// <remarks>A component dialog has an inner <see cref="DialogSet"/> and <see cref="DialogContext"/>,
    /// which provides an inner dialog stack that is hidden from the parent dialog.</remarks>
    public class ComponentDialog : Dialog
    {
        private const string PersistedDialogState = "dialogs";

        private DialogSet _dialogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentDialog"/> class.
        /// </summary>
        /// <param name="dialogId">The ID to assign to the new dialog within the parent dialog set.</param>
        public ComponentDialog(string dialogId)
            : base(dialogId)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            _dialogs = new DialogSet();
        }

        /// <summary>
        /// Gets or sets the <see cref="IBotTelemetryClient"/> to use for logging.
        /// When setting this property, all of the contained dialogs' <see cref="Dialog.TelemetryClient"/>
        /// properties are also set.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> to use when logging.</value>
        /// <seealso cref="DialogSet.TelemetryClient"/>
        public new IBotTelemetryClient TelemetryClient
        {
            get
            {
                return base.TelemetryClient;
            }

            set
            {
                base.TelemetryClient = value ?? NullBotTelemetryClient.Instance;
                _dialogs.TelemetryClient = base.TelemetryClient;
            }
        }

        /// <summary>
        /// Gets or sets the ID of the inner <see cref="Dialog"/> to start when the
        /// <see cref="ComponentDialog"/> is started.
        /// </summary>
        /// <value>The ID of the inner <see cref="Dialog"/> to start when the <see cref="ComponentDialog"/>
        /// is started.</value>
        /// <seealso cref="BeginDialogAsync(DialogContext, object, CancellationToken)"/>
        protected string InitialDialogId { get; set; }

        /// <summary>
        /// Called when the dialog is started and pushed onto the parent's dialog stack.
        /// </summary>
        /// <param name="outerDc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.</remarks>
        /// <seealso cref="DialogContext.BeginDialogAsync(string, object, CancellationToken)"/>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (outerDc == null)
            {
                throw new ArgumentNullException(nameof(outerDc));
            }

            // Start the inner dialog.
            var dialogState = new DialogState();
            outerDc.ActiveDialog.State[PersistedDialogState] = dialogState;
            var innerDc = new DialogContext(_dialogs, outerDc.Context, dialogState);
            innerDc.Parent = outerDc;
            var turnResult = await OnBeginDialogAsync(innerDc, options, cancellationToken).ConfigureAwait(false);

            // Check for end of inner dialog
            if (turnResult.Status != DialogTurnStatus.Waiting)
            {
                // Return result to calling dialog
                return await EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
            }

            // Just signal waiting
            return EndOfTurn;
        }

        /// <summary>
        /// Called when the dialog is _continued_, where it is the active dialog and the
        /// user replies with a new activity.
        /// </summary>
        /// <param name="outerDc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog. The result may also contain a
        /// return value.
        ///
        /// If this method is *not* overridden, the component dialog calls the
        /// <see cref="DialogContext.ContinueDialogAsync(CancellationToken)"/> method on its inner
        /// dialog context. If the inner dialog stack is empty, the component dialog ends, and if
        /// a <see cref="DialogTurnResult.Result"/> is available, the component dialog uses that as
        /// its return value.
        /// </remarks>
        /// <seealso cref="DialogContext.ContinueDialogAsync(CancellationToken)"/>
        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (outerDc == null)
            {
                throw new ArgumentNullException(nameof(outerDc));
            }

            // Continue execution of inner dialog.
            var dialogState = (DialogState)outerDc.ActiveDialog.State[PersistedDialogState];
            var innerDc = new DialogContext(_dialogs, outerDc.Context, dialogState);
            innerDc.Parent = outerDc;
            var turnResult = await OnContinueDialogAsync(innerDc, cancellationToken).ConfigureAwait(false);

            if (turnResult.Status != DialogTurnStatus.Waiting)
            {
                // Return result to calling dialog
                return await EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
            }

            return EndOfTurn;
        }

        /// <summary>
        /// Called when a child dialog on the parent's dialog stack completed this turn, returning
        /// control to this dialog component.
        /// </summary>
        /// <param name="outerDc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
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
        /// <see cref="BeginDialogAsync(DialogContext, object, CancellationToken)"/> in the parent's
        /// context. However, if the
        /// <see cref="DialogContext.ReplaceDialogAsync(string, object, CancellationToken)"/> method
        /// is called, the logical child dialog may be different than the original.
        ///
        /// If this method is *not* overridden, the dialog automatically calls its
        /// <see cref="RepromptDialogAsync(ITurnContext, DialogInstance, CancellationToken)"/> when
        /// the user replies.
        /// </remarks>
        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Containers are typically leaf nodes on the stack but the developer is free to push other dialogs
            // on top of the stack which will result in the container receiving an unexpected call to
            // ResumeDialogAsync() when the pushed on dialog ends.
            // To avoid the container prematurely ending we need to implement this method and simply
            // ask our inner dialog stack to re-prompt.
            await RepromptDialogAsync(outerDc.Context, outerDc.ActiveDialog, cancellationToken).ConfigureAwait(false);
            return Dialog.EndOfTurn;
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
        public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Delegate to inner dialog.
            var dialogState = (DialogState)instance.State[PersistedDialogState];
            var innerDc = new DialogContext(_dialogs, turnContext, dialogState);
            await innerDc.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);

            // Notify component
            await OnRepromptDialogAsync(turnContext, instance, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when the dialog is ending.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="instance">State information associated with the instance of this component
        /// dialog on its parent's dialog stack.</param>
        /// <param name="reason">Reason why the dialog ended.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Forward cancel to inner dialogs
            if (reason == DialogReason.CancelCalled)
            {
                var dialogState = (DialogState)instance.State[PersistedDialogState];
                var innerDc = new DialogContext(_dialogs, turnContext, dialogState);
                await innerDc.CancelAllDialogsAsync(cancellationToken).ConfigureAwait(false);
            }

            await OnEndDialogAsync(turnContext, instance, reason, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new <see cref="Dialog"/> to the component dialog and returns the updated component.
        /// </summary>
        /// <param name="dialog">The dialog to add.</param>
        /// <returns>The <see cref="ComponentDialog"/> after the operation is complete.</returns>
        /// <remarks>The added dialog's <see cref="Dialog.TelemetryClient"/> is set to the
        /// <see cref="TelemetryClient"/> of the component dialog.</remarks>
        public ComponentDialog AddDialog(Dialog dialog)
        {
            _dialogs.Add(dialog);
            if (string.IsNullOrEmpty(InitialDialogId))
            {
                InitialDialogId = dialog.Id;
            }

            return this;
        }

        /// <summary>
        /// Searches the inner <see cref="DialogSet"/> of the component dialog for a
        /// <see cref="Dialog"/> by its ID.
        /// </summary>
        /// <param name="dialogId">The ID of the dialog to find.</param>
        /// <returns>The dialog; or <c>null</c> if there is not a match for the ID.</returns>
        public Dialog FindDialog(string dialogId)
        {
            return _dialogs.Find(dialogId);
        }

        protected virtual Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return innerDc.BeginDialogAsync(InitialDialogId, options, cancellationToken);
        }

        protected virtual Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return innerDc.ContinueDialogAsync(cancellationToken);
        }

        protected virtual Task OnEndDialogAsync(ITurnContext context, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnRepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        protected virtual Task<DialogTurnResult> EndComponentAsync(DialogContext outerDc, object result, CancellationToken cancellationToken)
        {
            return outerDc.EndDialogAsync(result, cancellationToken);
        }
    }
}
