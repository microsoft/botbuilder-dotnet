// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class ComponentDialog : DialogContainer
    {
        public const string PersistedDialogState = "dialogs";

        private bool initialized = false;

        public ComponentDialog(string dialogId = null)
            : base(dialogId)
        {
        }

        public string InitialDialogId { get; set; }

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

            await EnsureInitialized(outerDc).ConfigureAwait(false);

            var innerDc = this.CreateChildContext(outerDc);
            var turnResult = await OnBeginDialogAsync(innerDc, options, cancellationToken).ConfigureAwait(false);

            // Check for end of inner dialog
            if (turnResult.Status != DialogTurnStatus.Waiting)
            {
                if (turnResult.Status == DialogTurnStatus.Cancelled)
                {
                    await EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
                    return new DialogTurnResult(DialogTurnStatus.Cancelled, turnResult.Result);
                }

                // Return result to calling dialog
                return await EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
            }

            // Just signal waiting
            return Dialog.EndOfTurn;
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Continue execution of inner dialog
            var innerDc = this.CreateChildContext(outerDc);
            var turnResult = await this.OnContinueDialogAsync(innerDc, cancellationToken).ConfigureAwait(false);

            // Check for end of inner dialog
            if (turnResult.Status != DialogTurnStatus.Waiting)
            {
                if (turnResult.Status == DialogTurnStatus.Cancelled)
                {
                    await EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
                    return new DialogTurnResult(DialogTurnStatus.Cancelled, turnResult.Result);
                }

                // Return to calling dialog
                return await this.EndComponentAsync(outerDc, turnResult.Result, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Signal end of turn
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

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext outerDc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

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
            var innerDc = this.CreateInnerDc(turnContext, instance, null, null);
            await innerDc.RepromptDialogAsync(cancellationToken).ConfigureAwait(false);

            // Notify component
            await OnRepromptDialogAsync(turnContext, instance, cancellationToken).ConfigureAwait(false);
        }

        public override async Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Forward cancel to inner dialogs
            if (reason == DialogReason.CancelCalled)
            {
                var innerDc = this.CreateInnerDc(turnContext, instance, null, null);
                await innerDc.CancelAllDialogsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            await OnEndDialogAsync(turnContext, instance, reason, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a dialog to the component dialog.
        /// </summary>
        /// <param name="dialog">The dialog to add.</param>
        /// <returns>The updated <see cref="ComponentDialog"/>.</returns>
        /// <remarks>Adding a new dialog will inherit the <see cref="IBotTelemetryClient"/> of the ComponentDialog.</remarks>
        public override Dialog AddDialog(IDialog dialog)
        {
            base.AddDialog(dialog);

            if (this.InitialDialogId == null)
            {
                this.InitialDialogId = dialog.Id;
            }

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

        public override DialogContext CreateChildContext(DialogContext dc)
        {
            var childDc = this.CreateInnerDc(dc.Context, dc.ActiveDialog, dc.State.User, dc.State.Conversation);
            childDc.Parent = dc;
            return childDc;
        }

        private DialogContext CreateInnerDc(ITurnContext context, DialogInstance instance, IDictionary<string, object> userState, IDictionary<string, object> conversationState)
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

            return new DialogContext(this._dialogs, context, state, conversationState, userState);
        }

        protected virtual Task OnInitialize(DialogContext dc)
        {
            if (this.InitialDialogId == null)
            {
                this.InitialDialogId = _dialogs.GetDialogs().FirstOrDefault()?.Id;
            }

            return Task.CompletedTask;
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

        protected override string OnComputeId()
        {
            return $"component[{this.BindingPath()}]";
        }
    }
}
