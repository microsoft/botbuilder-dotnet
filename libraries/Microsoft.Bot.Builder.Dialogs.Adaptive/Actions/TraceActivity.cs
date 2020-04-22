// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
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
        public const string Kind = "Microsoft.TraceActivity";

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
        public BoolExpression Disabled { get; set; } 

        /// <summary>
        /// Gets or sets name of the trace activity.
        /// </summary>
        /// <value>
        /// Name of the trace activity.
        /// </value>
        [JsonProperty("name")]
        public StringExpression Name { get; set; } 

        /// <summary>
        /// Gets or sets value type of the trace activity.
        /// </summary>
        /// <value>
        /// Value type of the trace activity.
        /// </value>
        [JsonProperty("valueType")]
        public StringExpression ValueType { get; set; } 

        /// <summary>
        /// Gets or sets value expression to send as the value. 
        /// </summary>
        /// <value>
        /// Property binding to memory to send as the value. 
        /// </value>
        [JsonProperty("value")]
        public ValueExpression Value { get; set; } 

        /// <summary>
        /// Gets or sets a label to use when describing a trace activity.
        /// </summary>
        /// <value>The label to use. (default will use Name or parent dialog.id).</value>
        [JsonProperty]
        public StringExpression Label { get; set; } 

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

            object value = null;
            if (this.Value != null)
            {
                var (val, valError) = this.Value.TryGetValue(dc.State);
                if (valError != null)
                {
                    throw new Exception(valError);
                }

                value = val;
            }
            else
            {
                value = dc.State.GetMemorySnapshot();
            }

            var name = this.Name?.GetValue(dc.State);
            var valueType = this.ValueType?.GetValue(dc.State);
            var label = this.Label?.GetValue(dc.State);

            var traceActivity = Activity.CreateTraceActivity(name ?? "Trace", valueType: valueType ?? "State", value: value, label: label ?? name ?? dc.Parent?.ActiveDialog?.Id);
            await dc.Context.SendActivityAsync(traceActivity, cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(traceActivity, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({Name?.ToString()})";
        }
    }
}
