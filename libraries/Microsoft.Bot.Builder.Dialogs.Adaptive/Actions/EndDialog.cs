// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Command to end the current dialog, returning the resultProperty as the result of the dialog.
    /// </summary>
    public class EndDialog : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.EndDialog";

        private Expression value;
        private Expression disabled;

        [JsonConstructor]
        public EndDialog(string value = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);

            if (!string.IsNullOrEmpty(value))
            {
                this.Value = value;
            }
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
        public string Disabled
        {
            get { return disabled?.ToString(); }
            set { disabled = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Gets or sets a value expression for the result to be returned to the caller.
        /// </summary>
        /// <value>
        /// A value expression for the result to be returned to the caller.
        /// </value>
        [JsonProperty("value")]
        public string Value
        {
            get { return value?.ToString(); }
            set { this.value = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.disabled != null && (bool?)this.disabled.TryEvaluate(dc.GetState()).value == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (this.Value != null)
            {
                var (result, error) = this.value.TryEvaluate(dc.GetState());
                return await EndParentDialogAsync(dc, result, cancellationToken).ConfigureAwait(false);
            }

            return await EndParentDialogAsync(dc, result: null, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected async Task<DialogTurnResult> EndParentDialogAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            if (dc.Parent != null)
            {
                var turnResult = await dc.Parent.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
                turnResult.ParentEnded = true;
                return turnResult;
            }
            else
            {
                return await dc.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({this.Value ?? string.Empty})";
        }
    }
}
