// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Send an Tace activity back to the transcript.
    /// </summary>
    public class TraceActivity : DialogAction
    {
        [JsonConstructor]
        public TraceActivity([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets name of the trace activity.
        /// </summary>
        /// <value>
        /// Name of the trace activity.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets value type of the trace activity.
        /// </summary>
        /// <value>
        /// Value type of the trace activity.
        /// </value>
        public string ValueType { get; set; }

        /// <summary>
        /// Gets or sets property binding to memory to send as the value. 
        /// </summary>
        /// <value>
        /// Property binding to memory to send as the value. 
        /// </value>
        public string Value { get; set; }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            object value = null;
            if (!string.IsNullOrEmpty(this.Value))
            {
                value = JObject.FromObject(dc.State.GetValue<object>(this.Value));
            }
            else
            {
                value = JObject.FromObject(dc.State);
            }

            var traceActivity = Activity.CreateTraceActivity(this.Name, valueType: this.ValueType, value: value);
            await dc.Context.SendActivityAsync(traceActivity, cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(traceActivity, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"TraceActivity({Name})";
        }
    }
}
