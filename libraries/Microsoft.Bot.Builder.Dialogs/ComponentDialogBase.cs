// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public abstract class ComponentDialogBase : Dialog
    {
        protected const string PersistedDialogState = "dialogs";

        protected DialogSet _dialogs;

        private bool initialized = false;

        public ComponentDialogBase(string dialogId=null)
            : base(dialogId)
        {
            _dialogs = new DialogSet();
        }

        /// <summary>
        /// Gets or sets or set the <see cref="IBotTelemetryClient"/> to use.
        /// When setting this property, all the contained dialogs TelemetryClient properties are also set.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> to use when logging.</value>
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

        protected virtual Task OnInitialize(DialogContext dc)
        {
            return Task.CompletedTask;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (outerDc == null)
            {
                throw new ArgumentNullException(nameof(outerDc));
            }

            // Start the inner dialog.
            var dialogState = new DialogState();
            outerDc.DialogState[PersistedDialogState] = dialogState;

            await EnsureInitialized(outerDc).ConfigureAwait(false);

            var innerDc = new DialogContext(_dialogs, outerDc, dialogState, outerDc.State.Conversation, outerDc.State.User);
            var turnResult = await OnBeginDialogAsync(innerDc, options, cancellationToken).ConfigureAwait(false);

            // Check for end of inner dialog
            if (turnResult.Status != DialogTurnStatus.Waiting)
            {
                // Return result to calling dialog
                return await EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Just signal waiting
                return Dialog.EndOfTurn;
            }
        }

        protected async Task EnsureInitialized(DialogContext outerDc)
        {
            if (!this.initialized)
            {
                this.initialized = true;
                await OnInitialize(outerDc).ConfigureAwait(false);
            }
        }

        public override async Task<DialogConsultation> ConsultDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (outerDc == null)
            {
                throw new ArgumentNullException(nameof(outerDc));
            }

            await EnsureInitialized(outerDc).ConfigureAwait(false);

            // Consult inner dialog.
            var dialogState = (DialogState)outerDc.DialogState[PersistedDialogState];
            var innerDc = new DialogContext(_dialogs, outerDc, dialogState, outerDc.State.Conversation, outerDc.State.User);
            var innerConsultation = await OnConsultDialog(innerDc, cancellationToken).ConfigureAwait(false);

            // Call OnContinueDialog() with inner consultation
            // - The default implementation of OnContinueDialog() will simply invoke the inner processor that was returned. 
            // - This lets legacy components that have added custom interruption logic to continue to operate as designed.
            return new DialogConsultation()
            {
                Desire = innerConsultation != null ? innerConsultation.Desire : DialogConsultationDesire.CanProcess,
                Processor = (dc) => OnContinueDialogAsync(innerDc, innerConsultation),
            };
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await EnsureInitialized(outerDc).ConfigureAwait(false);

            // Containers are typically leaf nodes on the stack but the dev is free to push other dialogs
            // on top of the stack which will result in the container receiving an unexpected call to
            // dialogResume() when the pushed on dialog ends.
            // To avoid the container prematurely ending we need to implement this method and simply
            // ask our inner dialog stack to re-prompt.
            await RepromptDialogAsync(outerDc.Context, outerDc.ActiveDialog, cancellationToken).ConfigureAwait(false);
            return Dialog.EndOfTurn;
        }

        public override async Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Delegate to inner dialog.
            var dialogState = (DialogState)((Dictionary<string, object>)instance.State)[PersistedDialogState];
            var innerDc = new DialogContext(_dialogs, turnContext, dialogState, new Dictionary<string, object>(), new Dictionary<string, object>());
            await innerDc.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);

            // Notify component
            await OnRepromptDialogAsync(turnContext, instance, cancellationToken).ConfigureAwait(false);
        }

        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Forward cancel to inner dialogs
            if (reason == DialogReason.CancelCalled)
            {
                var dialogState = (DialogState)((Dictionary<string, object>)instance.State)[PersistedDialogState];
                var innerDc = new DialogContext(_dialogs, turnContext, dialogState, new Dictionary<string, object>(), new Dictionary<string, object>());
                await innerDc.CancelAllDialogsAsync(cancellationToken).ConfigureAwait(false);
            }

            await OnEndDialogAsync(turnContext, instance, reason, cancellationToken).ConfigureAwait(false);
        }

        public ComponentDialogBase AddDialog(Dialog dialog)
        {
            return AddDialog(dialog as IDialog);
        }

        /// <summary>
        /// Adds a dialog to the component dialog.
        /// </summary>
        /// <param name="dialog">The dialog to add.</param>
        /// <returns>The updated <see cref="ComponentDialog"/>.</returns>
        /// <remarks>Adding a new dialog will inherit the <see cref="IBotTelemetryClient"/> of the ComponentDialog.</remarks>
        public ComponentDialogBase AddDialog(IDialog dialog)
        {
            _dialogs.Add(dialog);
            return this;
        }

        /// <summary>
        /// Finds a dialog by ID (ONLY in this ComponentDialog, use DialogContext.FindDialog to get scoped dialogs)
        /// </summary>
        /// <param name="dialogId">The ID of the dialog to find.</param>
        /// <returns>The dialog; or <c>null</c> if there is not a match for the ID.</returns>
        public IDialog FindDialog(string dialogId)
        {
            return _dialogs.Find(dialogId);
        }

        protected abstract Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken));

        protected virtual Task<DialogConsultation> OnConsultDialog(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return innerDc.ConsultDialogAsync(cancellationToken);
        }

        protected virtual Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, DialogConsultation consultation = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return consultation != null ? consultation.Processor(innerDc) : innerDc.ContinueDialogAsync(cancellationToken);
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
            return outerDc.EndDialogAsync(result);
        }

        protected override string OnComputeId()
        {
            return $"component[{this.BindingPath()}]";
        }
    }
}
