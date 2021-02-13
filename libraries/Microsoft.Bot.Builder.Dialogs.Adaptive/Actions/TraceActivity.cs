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
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TraceActivity";

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceActivity"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
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

            object value = null;
            if (this.Value != null)
            {
                var (val, valError) = this.Value.TryGetValue(dc.State);
                if (valError != null)
                {
                    throw new InvalidOperationException($"Expression evaluation resulted in an error. Expression: \"{this.Value.ToString()}\". Error: {valError}");
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

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}({Name?.ToString()})";
        }
    }
}
