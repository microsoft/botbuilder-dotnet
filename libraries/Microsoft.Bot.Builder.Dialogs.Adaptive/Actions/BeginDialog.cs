// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action which begins executing another dialog, when it is done, it will return to the caller.
    /// </summary>
    public class BeginDialog : BaseInvokeDialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.BeginDialog";

        /// <summary>
        /// Initializes a new instance of the <see cref="BeginDialog"/> class.
        /// </summary>
        /// <param name="dialogIdToCall">Optional, id of the dialog to call.</param>
        /// <param name="options">Optional, object with additional options.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public BeginDialog(string dialogIdToCall = null, object options = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(dialogIdToCall, options)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; } 

        /// <summary>
        /// Gets or sets the property path to store the dialog result in.
        /// </summary>
        /// <value>
        /// The property path to store the dialog result in.
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }
            
            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var dialog = this.ResolveDialog(dc);

            // use bindingOptions to bind to the bound options
            var boundOptions = BindOptions(dc, options);

            // set the activity processed state (default is true)
            dc.State.SetValue(TurnPath.ActivityProcessed, this.ActivityProcessed.GetValue(dc.State));

            // start dialog with bound options passed in as the options
            return await dc.BeginDialogAsync(dialog.Id, options: boundOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when a child dialog completed its turn, returning control to this dialog.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="reason">Reason why the dialog resumed.</param>
        /// <param name="result">Optional, value returned from the dialog that was called. The type
        /// of the value returned is dependent on the child dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
            }

            // By default just end the current dialog and return result to parent.
            return await dc.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
        }
    }
}
