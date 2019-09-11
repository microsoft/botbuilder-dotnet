// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Command to end the current dialog, returning the resultProperty as the result of the dialog.
    /// </summary>
    public class EndDialog : DialogAction
    {
        private Expression value;

        [JsonConstructor]
        public EndDialog(string property = null, string value = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);

            if (!string.IsNullOrEmpty(property))
            {
                this.Property = property;
            }

            if (!string.IsNullOrEmpty(value))
            {
                this.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets a path to memory where the result of the value should be stored.
        /// </summary>
        [JsonProperty("property")]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets a value expression for the result to be returned to the caller
        /// </summary>
        [JsonProperty("value")]
        public string Value
        {
            get { return value?.ToString(); }
            set { this.value = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Value != null && this.Property != null)
            {
                var (result, error) = this.value.TryEvaluate(dc.State);
                dc.State.SetValue(this.Property, result);
                return await EndParentDialogAsync(dc, result, cancellationToken).ConfigureAwait(false);
            }

            return await EndParentDialogAsync(dc, result: null, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({this.Property ?? string.Empty})";
        }
    }
}
