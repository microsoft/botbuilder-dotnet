// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Track a custom event using IBotTelemetryClient.
    /// </summary>
    public class TelemetryTrackEventAction : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TelemetryTrackEventAction";

        [JsonConstructor]
        public TelemetryTrackEventAction(string eventName, Dictionary<string, StringExpression> properties = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.EventName = eventName;
            this.Properties = properties;
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
        /// Gets or sets a name to use for the event.
        /// </summary>
        /// <value>The event name to use.</value>
        [JsonProperty]
        public StringExpression EventName { get; set; }

        /// <summary>
        /// Gets or sets the properties to attach to the tracked event.
        /// </summary>
        /// <value>
        /// A collection of properties to attach to the tracked event.
        /// </value>
        [JsonProperty]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public Dictionary<string, StringExpression> Properties { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

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

            if (this.EventName != null)
            {
                TelemetryClient.TrackEvent(
                    this.EventName.GetValue(dc.State),
                    this.Properties?.ToDictionary(kv => kv.Key, kv => kv.Value.GetValue(dc.State)));
            }

            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({EventName?.ToString()})";
        }
    }
}
