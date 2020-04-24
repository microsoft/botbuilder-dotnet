// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A <see cref="Dialog"/> that is composed of other dialogs.
    /// </summary>
    /// <remarks>A component dialog has an inner <see cref="DialogSet"/> and <see cref="DialogContext"/>,
    /// which provides an inner dialog stack that is hidden from the parent dialog.</remarks>
    public class ComponentDialog : DialogContainer
    {
        public const string PersistedDialogState = "dialogs";

        private bool initialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentDialog"/> class.
        /// </summary>
        /// <param name="dialogId">The ID to assign to the new dialog within the parent dialog set.</param>
        public ComponentDialog(string dialogId = null)
            : base(dialogId)
        {
        }

        public string InitialDialogId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IBotTelemetryClient"/> to use for logging.
        /// When setting this property, all of the contained dialogs' <see cref="Dialog.TelemetryClient"/>
        /// properties are also set.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> to use when logging.</value>
        /// <seealso cref="DialogSet.TelemetryClient"/>
        public override IBotTelemetryClient TelemetryClient
        {
            get
            {
                return base.TelemetryClient;
            }

            set
            {
                base.TelemetryClient = value ?? NullBotTelemetryClient.Instance;
                Dialogs.TelemetryClient = base.TelemetryClient;
            }
        }

        /// <summary>
        /// Called when the dialog is started and pushed onto the parent's dialog stack.
        /// </summary>
        /// <param name="outerDc">The parent <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.</remarks>
        /// <seealso cref="OnBeginDialogAsync(DialogContext, object, CancellationToken)"/>
        /// <seealso cref="DialogContext.BeginDialogAsync(string, object, CancellationToken)"/>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (outerDc == null)
            {
                throw new ArgumentNullException(nameof(outerDc));
            }

            await EnsureInitializedAsync(outerDc).ConfigureAwait(false);

            await this.CheckForVersionChangeAsync(outerDc).ConfigureAwait(false);

            var innerDc = this.CreateChildContext(outerDc);
            var turnResult = await OnBeginDialogAsync(innerDc, options, cancellationToken).ConfigureAwait(false);

            // Check for end of inner dialog
            if (turnResult.Status != DialogTurnStatus.Waiting)
            {
                // Return result to calling dialog
                return await EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
            }

            TelemetryClient.TrackDialogView(Id);

            // Just signal waiting
            return Dialog.EndOfTurn;
        }

        /// <summary>
        /// Called when the dialog is _continued_, where it is the active dialog and the
        /// user replies with a new activity.
        /// </summary>
        /// <param name="outerDc">The parent <see cref="DialogContext"/> for the current turn of conversation.</param>
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
        /// <seealso cref="OnContinueDialogAsync(DialogContext, CancellationToken)"/>
        /// <seealso cref="DialogContext.ContinueDialogAsync(CancellationToken)"/>
        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            await EnsureInitializedAsync(outerDc).ConfigureAwait(false);

            await this.CheckForVersionChangeAsync(outerDc).ConfigureAwait(false);

            // Continue execution of inner dialog
            var innerDc = this.CreateChildContext(outerDc);
            var turnResult = await this.OnContinueDialogAsync(innerDc, cancellationToken).ConfigureAwait(false);

            // Check for end of inner dialog
            if (turnResult.Status != DialogTurnStatus.Waiting)
            {
                // Return to calling dialog
                return await this.EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
            }

            // Just signal waiting
            return Dialog.EndOfTurn;
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
        /// <seealso cref="RepromptDialogAsync(ITurnContext, DialogInstance, CancellationToken)"/>
        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            await EnsureInitializedAsync(outerDc).ConfigureAwait(false);

            await this.CheckForVersionChangeAsync(outerDc).ConfigureAwait(false);

            // Containers are typically leaf nodes on the stack but the dev is free to push other dialogs
            // on top of the stack which will result in the container receiving an unexpected call to
            // dialogResume() when the pushed on dialog ends.
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
        /// <seealso cref="OnRepromptDialogAsync(ITurnContext, DialogInstance, CancellationToken)"/>
        /// <seealso cref="DialogContext.RepromptDialogAsync(CancellationToken)"/>
        public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Delegate to inner dialog.
            var innerDc = this.CreateInnerDc(turnContext, instance);
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
        /// <remarks>When this method is called from the parent dialog's context, the component dialog
        /// cancels all of the dialogs on its inner dialog stack before ending.</remarks>
        /// <seealso cref="OnEndDialogAsync(ITurnContext, DialogInstance, DialogReason, CancellationToken)"/>
        /// <seealso cref="DialogContext.EndDialogAsync(object, CancellationToken)"/>
        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Forward cancel to inner dialogs
            if (reason == DialogReason.CancelCalled)
            {
                var innerDc = this.CreateInnerDc(turnContext, instance);
                await innerDc.CancelAllDialogsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
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
            this.Dialogs.Add(dialog);

            if (this.InitialDialogId == null)
            {
                this.InitialDialogId = dialog.Id;
            }

            return this;
        }

        public override DialogContext CreateChildContext(DialogContext dc)
        {
            var childDc = this.CreateInnerDc(dc.Context, dc.ActiveDialog);
            childDc.Parent = dc;
            return childDc;
        }

        protected async Task EnsureInitializedAsync(DialogContext outerDc)
        {
            if (!this.initialized)
            {
                this.initialized = true;
                await OnInitializeAsync(outerDc).ConfigureAwait(false);
            }
        }

        protected virtual Task OnInitializeAsync(DialogContext dc)
        {
            if (this.InitialDialogId == null)
            {
                this.InitialDialogId = Dialogs.GetDialogs().FirstOrDefault()?.Id;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when the dialog is started and pushed onto the parent's dialog stack.
        /// </summary>
        /// <param name="innerDc">The inner <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog.
        ///
        /// By default, this calls the
        /// <see cref="Dialog.BeginDialogAsync(DialogContext, object, CancellationToken)"/> method
        /// of the component dialog's initial dialog, as defined by <see cref="InitialDialogId"/>.
        ///
        /// Override this method in a derived class to implement interrupt logic.</remarks>
        /// <seealso cref="BeginDialogAsync(DialogContext, object, CancellationToken)"/>
        protected virtual Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return innerDc.BeginDialogAsync(InitialDialogId, options, cancellationToken);
        }

        /// <summary>
        /// Called when the dialog is _continued_, where it is the active dialog and the
        /// user replies with a new activity.
        /// </summary>
        /// <param name="innerDc">The inner <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>If the task is successful, the result indicates whether the dialog is still
        /// active after the turn has been processed by the dialog. The result may also contain a
        /// return value.
        ///
        /// By default, this calls the currently active inner dialog's
        /// <see cref="Dialog.ContinueDialogAsync(DialogContext, CancellationToken)"/> method.
        ///
        /// Override this method in a derived class to implement interrupt logic.</remarks>
        /// <seealso cref=" ContinueDialogAsync(DialogContext, CancellationToken)"/>
        protected virtual Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return innerDc.ContinueDialogAsync(cancellationToken);
        }

        /// <summary>
        /// Called when the dialog is ending.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="instance">State information associated with the inner dialog stack of this
        /// component dialog.</param>
        /// <param name="reason">Reason why the dialog ended.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>Override this method in a derived class to implement any additional logic that
        /// should happen at the component level, after all inner dialogs have been canceled.</remarks>
        /// <seealso cref="EndDialogAsync(ITurnContext, DialogInstance, DialogReason, CancellationToken)"/>
        protected virtual Task OnEndDialogAsync(ITurnContext context, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when the dialog should re-prompt the user for input.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="instance">State information associated with the inner dialog stack of this
        /// component dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>Override this method in a derived class to implement any additional logic that
        /// should happen at the component level, after the re-prompt operation completes for the inner
        /// dialog.</remarks>
        /// <seealso cref="RepromptDialogAsync(ITurnContext, DialogInstance, CancellationToken)"/>
        protected virtual Task OnRepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Ends the component dialog in its parent's context.
        /// </summary>
        /// <param name="outerDc">The parent <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="result">Optional, value to return from the dialog component to the parent context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result indicates that the dialog ended after the
        /// turn was processed by the dialog.
        ///
        /// In general, the parent context is the dialog or bot turn handler that started the dialog.
        /// If the parent is a dialog, the stack calls the parent's
        /// <see cref="Dialog.ResumeDialogAsync(DialogContext, DialogReason, object, CancellationToken)"/>
        /// method to return a result to the parent dialog. If the parent dialog does not implement
        /// `ResumeDialogAsync`, then the parent will end, too, and the result is passed to the next
        /// parent context, if one exists.
        ///
        /// The returned <see cref="DialogTurnResult"/> contains the return value in its
        /// <see cref="DialogTurnResult.Result"/> property.</remarks>
        /// <seealso cref="BeginDialogAsync(DialogContext, object, CancellationToken)"/>
        /// <seealso cref="ContinueDialogAsync(DialogContext, CancellationToken)"/>
        protected virtual Task<DialogTurnResult> EndComponentAsync(DialogContext outerDc, object result, CancellationToken cancellationToken)
        {
            return outerDc.EndDialogAsync(result, cancellationToken);
        }

        private DialogContext CreateInnerDc(ITurnContext context, DialogInstance instance)
        {
            DialogState state;

            if (instance.State.ContainsKey(PersistedDialogState))
            {
                state = instance.State[PersistedDialogState] as DialogState;
            }
            else
            {
                state = new DialogState();
                instance.State[PersistedDialogState] = state;
            }

            if (state.DialogStack == null)
            {
                state.DialogStack = new List<DialogInstance>();
            }

            return new DialogContext(this.Dialogs, context, state);
        }
    }
}
