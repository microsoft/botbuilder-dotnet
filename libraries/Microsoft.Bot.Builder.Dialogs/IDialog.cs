// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public interface IDialog
    {
        /// <summary>
        /// Gets or sets unique id for the dialog.
        /// </summary>
        /// <value>
        /// Unique id for the dialog.
        /// </value>
        string Id { get; set; }

        /// <summary>
        /// Gets tags assigned to the dialog.
        /// </summary>
        /// <value>
        /// Tags assigned to the dialog.
        /// </value>
        List<string> Tags { get; }

        /// <summary>
        /// Gets dictionary of memory bindings which are evaluated in a call to `beginDialog()`.
        /// </summary>
        /// <remarks>Key = property expression to set in this dialog's memory context, Value = property expression of value you want to get from caller's memory context.</remarks>
        /// <example>{ "key": "value" } maps to set newDialogState.key = callerDialogState.value.</example>
        /// <value>
        /// Dictionary of memory bindings which are evaluated in a call to `beginDialog()`.
        /// </value>
        Dictionary<string, string> InputBindings { get; }

        /// <summary>
        /// Gets expression in the callers memory to store the result returned via `endDialog()` is called.
        /// </summary>
        /// <remarks>This the property which the result of EndDialog() for this dialog will be mapped to in the caller's dialog state.</remarks>
        /// <example>$foo will be set to EndDialog(result).</example>
        /// <value>
        /// Expression in the callers memory to store the result returned via `endDialog()` is called.
        /// </value>
        string OutputBinding { get; }

        /// <summary>
        /// Gets or sets telemetry client.
        /// </summary>
        /// <value>
        /// Telemetry client.
        /// </value>
        IBotTelemetryClient TelemetryClient { get; set; }

        /// <summary>
        /// Method called when a new dialog has been pushed onto the stack and is being activated.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="options">(Optional) arguments that were passed to the dialog during `begin()` call that started the instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Method called when an instance of the dialog is the "current" dialog and the
        /// user replies with a new activity. The dialog will generally continue to receive the users
        /// replies until it calls either `DialogSet.end()` or `DialogSet.begin()`.
        /// If this method is NOT implemented then the dialog will automatically be ended when the user replies.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Method called when an instance of the dialog is being returned to from another
        /// dialog that was started by the current instance using `DialogSet.begin()`.
        /// If this method is NOT implemented then the dialog will be automatically ended with a call
        /// to `DialogSet.endDialogWithResult()`. Any result passed from the called dialog will be passed
        /// to the current dialog's parent.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="reason">An enum indicating why the dialog resumed.</param>
        /// <param name="result">(Optional) value returned from the dialog that was called. The type of the value returned is dependant on the dialog that was called.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Trigger the dialog to prompt again.
        /// </summary>
        /// <param name="turnContext">Dialog turn context.</param>
        /// <param name="instance">Dialog instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RepromptDialogAsync(ITurnContext turnContext, DialogInstance instance, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// End the dialog.
        /// </summary>
        /// <param name="turnContext">Dialog turn context.</param>
        /// <param name="instance">Dialog instance.</param>
        /// <param name="reason">An enum indicating why the dialog ended.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task EndDialogAsync(ITurnContext turnContext, DialogInstance instance, DialogReason reason, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Called when an event has been raised, using `DialogContext.emitEvent()`, by either the current dialog or a dialog that the current dialog started.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of conversation.</param>
        /// <param name="e">The event being raised.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the event is handled by the current dialog and bubbling should stop.</returns>
        Task<bool> OnDialogEventAsync(DialogContext dc, DialogEvent e, CancellationToken cancellationToken);        
    }
}
