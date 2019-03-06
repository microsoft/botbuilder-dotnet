// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Base class for all dialogs.
    /// </summary>
    public abstract class Dialog : IDialog
    {
        public static readonly DialogTurnResult EndOfTurn = new DialogTurnResult(DialogTurnStatus.Waiting);
        private IBotTelemetryClient _telemetryClient;

        public Dialog(string dialogId = null)
        {
            Id = dialogId ?? OnComputeId();
            _telemetryClient = NullBotTelemetryClient.Instance;
        }

        /// <summary>
        /// Unique id for the dialog.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Set of tags assigned to the dialog.
        /// </summary>
        public List<string> Tags { get; private set; } = new List<string>();

        /// <summary>
        /// JSONPath expression for the memory slots to bind the dialogs options to on a call to `beginDialog()`.
        /// </summary>
        public Dictionary<string, string> InputBindings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// JSONPath expression for the memory slot to bind the dialogs result to when `endDialog()` is called.
        /// </summary>
        public string OutputBinding { get; set; }

        public virtual string Property
        {
            get { return OutputBinding; }
            set { OutputBinding = value; }
        }

        /// <summary>
        /// Gets or sets the telemetry client for logging events.
        /// </summary>
        /// <value>The Telemetry Client logger.</value>
        public virtual IBotTelemetryClient TelemetryClient
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
        /// <param name="options">(Optional) arguments that were passed to the dialog during `begin()` call that started the instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Method called when an instance of the dialog is the "current" dialog and the
        /// user replies with a new activity. The dialog will generally continue to receive the users
        /// replies until it calls either `DialogSet.end()` or `DialogSet.begin()`.
        /// If this method is NOT implemented then the dialog will automatically be ended when the user replies.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            // By default just end the current dialog.
            return await dc.EndDialogAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Method called when an instance of the dialog is being returned to from another
        /// dialog that was started by the current instance using `DialogSet.begin()`.
        /// If this method is NOT implemented then the dialog will be automatically ended with a call
        /// to `DialogSet.endDialogWithResult()`. Any result passed from the called dialog will be passed
        /// to the current dialogs parent.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="reason">Reason why the dialog resumed.</param>
        /// <param name="result">(Optional) value returned from the dialog that was called. The type of the value returned is dependant on the dialog that was called.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // By default just end the current dialog and return result to parent.
            return await dc.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
        }

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

        /// <summary>
        /// Should be overridden by dialogs that support multi-turn conversations. A function for 
        /// processing the utterance is returned along with a code indicating the dialogs desire to 
        /// process the utterance.This can be one of the following values. 
        /// - CanProcess - The dialog is capable of processing the utterance but parent dialogs 
        /// should feel free to intercept the utterance if they'd like.
        /// - ShouldProcess - The dialog (or one of its children) wants to process the utterance
        /// so parents should not intercept it.
        /// The default implementation calls the legacy ContinueDialogAsync for 
        /// compatibility reasons.That method simply calls DialogContext.EndDialog().
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public virtual async Task<DialogConsultation> ConsultDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new DialogConsultation()
            {
                Desire = DialogConsultationDesires.CanProcess,
                Processor = (dialogContext) => this.ContinueDialogAsync(dialogContext),
            };
        }

        /// <summary>
        /// Called when an event has been raised, using `DialogContext.emitEvent()`, by either the current dialog or a dialog that the current dialog started.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="e">The event being raised.</param>
        /// <returns>True if the event is handled by the current dialog and bubbling should stop.</returns>
        public virtual async Task<bool> OnDialogEventAsync(DialogContext dc, DialogEvent e)
        {
            return false;
        }

        protected virtual string OnComputeId()
        {
            return $"dialog[{this.BindingPath()}]";
        }

        protected virtual string BindingPath()
        {
            const string valueKey = "value";

            if (InputBindings.ContainsKey(valueKey))
            {
                return InputBindings[valueKey];
            }
            else if (!string.IsNullOrEmpty(OutputBinding))
            {
                return OutputBinding;
            }

            return string.Empty;
        }
    }
}
