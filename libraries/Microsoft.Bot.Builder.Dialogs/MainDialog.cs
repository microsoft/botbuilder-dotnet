// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// MainDialog handles the basic structure of running the root of a dialog stack.
    /// </summary>
    public abstract class MainDialog : ComponentDialog
    {
        private DialogSet _mainDialogSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainDialog"/> class.
        /// </summary>
        /// <param name="dialogState">State property used to persist the bots dialog stack.</param>
        /// <param name="dialogId">(Optional) id to assign to the main dialog on the stack. Defaults to a value of 'main'.</param>
        public MainDialog(IStatePropertyAccessor<DialogState> dialogState, string dialogId = nameof(MainDialog))
            : base(dialogId)
        {
            DialogStateProperty = dialogState ?? throw new ArgumentNullException(nameof(dialogState));

            _mainDialogSet = new DialogSet(DialogStateProperty);
            _mainDialogSet.Add(this);
        }

        /// <summary>
        /// Gets or sets the DialogState property Accessor.
        /// </summary>
        /// <value>
        /// A property accessor for the current DialogState.
        /// </value>
        protected IStatePropertyAccessor<DialogState> DialogStateProperty { get; set; }

        /// <summary>
        /// Run the current dialog stack.
        /// </summary>
        /// <param name="turnContext">Turn context containing the activity that was received.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of running the current main dialog.</returns>
        public async Task<DialogTurnResult> RunAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            // Create a dialog context and try to continue running the current dialog
            var dialogContext = await _mainDialogSet.CreateContextAsync(turnContext, cancellationToken).ConfigureAwait(false);

            // continue any running dialog
            var result = await dialogContext.ContinueDialogAsync(cancellationToken).ConfigureAwait(false);

            // if there isn't one running, then start main dialog as the root
            if (result.Status == DialogTurnStatus.Empty)
            {
                result = await dialogContext.BeginDialogAsync(this.Id, null, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        protected abstract Task<DialogTurnResult> OnRunTurnAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// The implementation just calls onContinueDialog()
        /// We're overriding the components built in logic that wants to start the initial dialog.
        /// We want the bots onRunTurn() implementation to always decide which dialog (if any) 
        /// gets started.
        /// </summary>
        /// <param name="innerDC">The DialogContext scoped to this ComponnentDialog instance.</param>
        /// <param name="options">(Optional) arguments that were passed to the dialog during `begin()` call that started the instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of running the OnRunAsync call.</returns>
        protected override Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDC, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnContinueDialogAsync(innerDC, cancellationToken);
        }

        /// <summary>
        /// The implementation calls onRunTurn()
        /// We're overriding the components built in logic that calls innerDC.continueDialog().
        /// We want the bots onRunTurn() implementation to decide what should happen for a turn.
        /// </summary>
        /// <param name="innerDC">The DialogContext scoped to this ComponnentDialog instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of running the OnRunAsync call.</returns>
        protected override Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDC, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnRunTurnAsync(innerDC, cancellationToken);
        }
    }
}
