// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Base class for all dialogs.
    /// </summary>
    [DebuggerDisplay("{Id}")]
    public abstract class Dialog
    {
        /// <summary>
        /// A <see cref="DialogTurnResult"/> that indicates that the current dialog is still
        /// active and waiting for input from the user next turn.
        /// </summary>
        public static readonly DialogTurnResult EndOfTurn = new DialogTurnResult(DialogTurnStatus.Waiting);

        private IBotTelemetryClient _telemetryClient;

        [JsonProperty("id")]
        private string id;

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

        /// <summary>
        /// Gets or sets id for the dialog.
        /// </summary>
        /// <value>
        /// Id for the dialog.
        /// </value>
        [JsonIgnore]
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
        /// Gets or sets the <see cref="IBotTelemetryClient"/> to use for logging.
        /// </summary>
        /// <value>The <see cref="IBotTelemetryClient"/> to use for logging.</value>
        /// <seealso cref="DialogSet.TelemetryClient"/>
        [JsonIgnore]
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

        [JsonIgnore]
        public virtual SourceRange Source => DebugSupport.SourceMap.TryGetValue(this, out var range) ? range : null;

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
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

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

        /// <summary>
        /// Gets a unique string which represents the version of this dialog.  If the version changes between turns the dialog system will emit a DialogChanged event.
        /// </summary>
        /// <returns>Unique string which should only change when dialog has changed in a way that should restart the dialog.</returns>
        public virtual string GetVersion()
        {
            return this.Id;
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
            var handled = await this.OnPreBubbleEventAsync(dc, e, cancellationToken).ConfigureAwait(false);

            // Bubble as needed
            if (!handled && e.Bubble && dc.Parent != null)
            {
                handled = await dc.Parent.EmitEventAsync(e.Name, e.Value, true, false, cancellationToken).ConfigureAwait(false);
            }

            // Post bubble
            if (!handled)
            {
                handled = await this.OnPostBubbleEventAsync(dc, e, cancellationToken).ConfigureAwait(false);
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns> Whether the event is handled by the current dialog and further processing should stop.</returns>
        protected virtual Task<bool> OnPreBubbleEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns> Whether the event is handled by the current dialog and further processing should stop.</returns>
        protected virtual Task<bool> OnPostBubbleEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        protected virtual string OnComputeId()
        {
            return this.GetType().Name;
        }

        protected void RegisterSourceLocation(string path, int lineNumber)
        {
            if (!string.IsNullOrEmpty(path))
            {
                DebugSupport.SourceMap.Add(this, new SourceRange()
                {
                    Path = path,
                    StartPoint = new SourcePoint() { LineIndex = lineNumber, CharIndex = 0 },
                    EndPoint = new SourcePoint() { LineIndex = lineNumber + 1, CharIndex = 0 },
                });
            }
        }
    }
}
