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
    /// Send an Tace activity back to the transcript
    /// </summary>
    public class TraceActivity : DialogAction
    {

        /// <summary>
        /// Name of the trace activity
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value type of the trace activity
        /// </summary>
        public string ValueType { get; set; }

        /// <summary>
        /// Property binding to memory to send as the value 
        /// </summary>
        public string Value { get; set; }

        [JsonConstructor]
        public TraceActivity([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

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
