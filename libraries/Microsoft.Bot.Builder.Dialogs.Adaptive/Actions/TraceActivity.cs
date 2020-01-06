// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Send an Tace activity back to the transcript.
    /// </summary>
    public class TraceActivity : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.TraceActivity";

        private Expression disabled;

        [JsonConstructor]
        public TraceActivity([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        public string Disabled
        {
            get { return disabled?.ToString(); }
            set { disabled = value != null ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Gets or sets name of the trace activity.
        /// </summary>
        /// <value>
        /// Name of the trace activity.
        /// </value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets value type of the trace activity.
        /// </summary>
        /// <value>
        /// Value type of the trace activity.
        /// </value>
        [JsonProperty("valueType")]
        public string ValueType { get; set; }

        /// <summary>
        /// Gets or sets value expression to send as the value. 
        /// </summary>
        /// <value>
        /// Property binding to memory to send as the value. 
        /// </value>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a label to use when describing a trace activity.
        /// </summary>
        /// <value>The label to use. (default will use Name or parent dialog.id).</value>
        [JsonProperty]
        public string Label { get; set; }

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

            object value = null;
            if (!string.IsNullOrEmpty(this.Value))
            {
                value = dc.GetState().GetValue<object>(this.Value);
            }
            else
            {
                value = dc.GetState().GetMemorySnapshot();
            }

            var traceActivity = Activity.CreateTraceActivity(this.Name ?? "Trace", valueType: this.ValueType ?? "State", value: value, label: this.Label ?? this.Name ?? dc.Parent?.ActiveDialog?.Id);
            await dc.Context.SendActivityAsync(traceActivity, cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(traceActivity, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({Name})";
        }
    }
}
