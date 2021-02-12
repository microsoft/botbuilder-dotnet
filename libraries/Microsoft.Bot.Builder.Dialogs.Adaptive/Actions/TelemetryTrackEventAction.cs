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
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TelemetryTrackEventAction";

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryTrackEventAction"/> class.
        /// </summary>
        /// <param name="eventName">Name to use for the event.</param>
        /// <param name="properties">Optional, properties to attach to the tracked event.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
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

            if (this.EventName != null)
            {
                TelemetryClient.TrackEvent(
                    this.EventName.GetValue(dc.State),
                    this.Properties?.ToDictionary(kv => kv.Key, kv => kv.Value.GetValue(dc.State)));
            }

            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}({EventName?.ToString()})";
        }
    }
}
