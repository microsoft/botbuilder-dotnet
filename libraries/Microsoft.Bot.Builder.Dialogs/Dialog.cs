// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Base class for all dialogs.
    /// </summary>
    [DebuggerDisplay("{Id}")]
    public abstract class Dialog : IDialog
    {
        public static readonly DialogTurnResult EndOfTurn = new DialogTurnResult(DialogTurnStatus.Waiting);

        private IBotTelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog"/> class.
        /// Called from constructors in derived classes to initialize the <see cref="Dialog"/> class.
        /// </summary>
        /// <param name="dialogId">The ID to assign to this dialog.</param>
        public Dialog(string dialogId = null)
        {
            Id = dialogId;
            _telemetryClient = NullBotTelemetryClient.Instance;
        }

        private string id;

        /// <summary>
        /// Unique id for the dialog.
        /// </summary>
        public string Id
        {
            get
            {
                id = id ?? OnComputeId();
                return id;
            }

            set
            {
                id = value;
            }
        }

        /// <summary>
        /// Set of tags assigned to the dialog.
        /// </summary>
        public List<string> Tags { get; private set; } = new List<string>();

        /// <summary>
        /// Gets or sets expression for the memory slots to bind the dialogs options to on a call to `beginDialog()`.
        /// </summary>
        public Dictionary<string, string> InputBindings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets jSONPath expression for the memory slot to bind the dialogs result to when `endDialog()` is called.
        /// </summary>
        public string OutputBinding { get; set; }

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
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

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

        /// <summary>
        /// Called when an event has been raised, using `DialogContext.emitEvent()`, by either the current dialog or a dialog that the current dialog started.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="e">The event being raised.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the event is handled by the current dialog and bubbling should stop.</returns>
        public virtual async Task<bool> OnDialogEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            // Before bubble
            var handled = await this.OnPreBubbleEvent(dc, e, cancellationToken).ConfigureAwait(false);

            // Bubble as needed
            if (!handled && e.Bubble && dc.Parent != null)
            {
                handled = await dc.Parent.EmitEventAsync(e.Name, e.Value, true, false, cancellationToken).ConfigureAwait(false);
            }

            // Post bubble
            if (!handled)
            {
                handled = await this.OnPostBubbleEvent(dc, e, cancellationToken).ConfigureAwait(false);
            }

            return handled;
        }

        /// <summary>
        /// Called before an event is bubbled to its parent.
        /// </summary>
        /// <remarks>
        /// This is a good place to perform interception of an event as returning `true` will prevent
        /// any further bubbling of the event to the dialogs parents and will also prevent any child
        /// dialogs from performing their default processing.
        /// </remarks>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="e">The event being raised.</param>
        /// <returns> Whether the event is handled by the current dialog and further processing should stop.</returns>
        protected virtual Task<bool> OnPreBubbleEvent(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Called after an event was bubbled to all parents and wasn't handled.
        /// </summary>
        /// <remarks>
        /// This is a good place to perform default processing logic for an event. Returning `true` will
        /// prevent any processing of the event by child dialogs.
        /// </remarks>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="e">The event being raised.</param>
        /// <returns> Whether the event is handled by the current dialog and further processing should stop.</returns>
        protected virtual Task<bool> OnPostBubbleEvent(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        protected virtual string OnComputeId()
        {
            return $"{this.GetType().Name}[{this.BindingPath()}]";
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

        protected void RegisterSourceLocation(string path, int lineNumber)
        {
            if (!string.IsNullOrEmpty(path))
            {
                DebugSupport.SourceRegistry.Add(this, new Source.Range()
                {
                    Path = path,
                    Start = new Source.Point() { LineIndex = lineNumber, CharIndex = 0 },
                    After = new Source.Point() { LineIndex = lineNumber + 1, CharIndex = 0 },
                });
            }
        }
    }
}
