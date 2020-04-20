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
    /// Action which calls another dialog, when it is done it will go to the callers parent dialog.
    /// </summary>
    public class ReplaceDialog : BaseInvokeDialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ReplaceDialog";

        [JsonConstructor]
        public ReplaceDialog(string dialogIdToCall = null, object options = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var dialog = this.ResolveDialog(dc);

            // use bindingOptions to bind to the bound options
            var boundOptions = BindOptions(dc, options);

            // set the activity processed state (default is true)
            dc.State.SetValue(TurnPath.ActivityProcessed, this.ActivityProcessed.GetValue(dc.State));

            // replace dialog with bound options passed in as the options
            return await dc.ReplaceDialogAsync(dialog.Id, options: boundOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
