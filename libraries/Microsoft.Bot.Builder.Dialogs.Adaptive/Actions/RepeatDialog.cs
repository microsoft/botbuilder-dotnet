// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action which repeats the active dialog (restarting it).
    /// </summary>
    public class RepeatDialog : BaseInvokeDialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.RepeatDialog";

        [JsonConstructor]
        public RepeatDialog(object options = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(null, options)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will allow loop of the repeated dialog.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression.
        /// </value>
        [JsonProperty("allowLoop")]
        public BoolExpression AllowLoop { get; set; }

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

            // use bindingOptions to bind to the bound options
            var boundOptions = BindOptions(dc, options);

            var targetDialogId = dc.Parent.ActiveDialog.Id;

            var repeatedIds = dc.State.GetValue<List<string>>(TurnPath.RepeatedIds, () => new List<string>());
            if (repeatedIds.Contains(targetDialogId))
            {
                if (this.AllowLoop == null || this.AllowLoop.GetValue(dc.State) == false)
                {
                    throw new ArgumentException($"Recursive loop detected, {targetDialogId} cannot be repeated twice in one turn.");
                }
            }
            else
            {
                repeatedIds.Add(targetDialogId);
            }

            dc.State.SetValue(TurnPath.RepeatedIds, repeatedIds);

            // set the activity processed state (default is true)
            dc.State.SetValue(TurnPath.ActivityProcessed, this.ActivityProcessed.GetValue(dc.State));

            var turnResult = await dc.Parent.ReplaceDialogAsync(dc.Parent.ActiveDialog.Id, boundOptions, cancellationToken).ConfigureAwait(false);
            turnResult.ParentEnded = true;
            return turnResult;
        }
    }
}
