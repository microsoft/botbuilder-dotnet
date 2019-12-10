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
    /// Sets a property with the result of evaluating a value expression.
    /// </summary>
    public class SetProperty : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.SetProperty";

        private Expression value;

        [JsonConstructor]
        public SetProperty([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets property path to put the value in.
        /// </summary>
        /// <value>
        /// Property path to put the value in.
        /// </value>
        [JsonProperty("property")]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the expression to get the value to put into property path.
        /// </summary>
        /// <value>
        /// The expression to get the value to put into property path.
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

            // SetProperty evaluates the "Value" expression and returns it as the result of the dialog
            var (value, valueError) = this.value.TryEvaluate(dc.GetState());
            if (valueError != null)
            {
                throw new Exception($"Expression evaluation resulted in an error. Expression: {this.Value.ToString()}. Error: {valueError}");
            }

            dc.GetState().SetValue(this.Property, value);

            dc.GetState().SetValue(DialogPath.Retries, 0);
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{this.Property ?? string.Empty}]";
        }
    }
}
