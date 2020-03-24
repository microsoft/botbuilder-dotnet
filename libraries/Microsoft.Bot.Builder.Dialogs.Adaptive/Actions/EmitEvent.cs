// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action which emits an event declaratively.
    /// </summary>
    public class EmitEvent : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.EmitEvent";

        [JsonConstructor]
        public EmitEvent(string eventName = null, object eventValue = null, bool bubble = false, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);

            if (eventName != null)
            {
                this.EventName = eventName;
            }

            if (eventValue != null)
            {
                this.EventValue = new ValueExpression(eventValue);
            }

            this.BubbleEvent = new BoolExpression(bubble);
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
        /// Gets or sets the name of the event to emit.
        /// </summary>
        /// <value>
        /// The name of the event to emit.
        /// </value>
        [JsonProperty("eventName")]
        public StringExpression EventName { get; set; }

        /// <summary>
        /// Gets or sets the memory property path to use to get the value to send as part of the event.
        /// </summary>
        /// <value>
        /// The memory property path to use to get the value to send as part of the event.
        /// </value>
        [JsonProperty("eventValue")]
        public ValueExpression EventValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the event should bubble to parents or not.
        /// </summary>
        /// <value>
        /// A value indicating whether gets or sets whether the event should bubble to parents or not.
        /// </value>
        [JsonProperty("bubbleEvent")]
        public BoolExpression BubbleEvent { get; set; }

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

            bool handled;
            var eventName = EventName?.GetValue(dc.State);
            var bubbleEvent = BubbleEvent.GetValue(dc.State);
            object value = null;
            
            if (EventValue != null)
            {
                value = this.EventValue.GetValue(dc.State);
            }

            if (dc.Parent != null)
            {
                handled = await dc.Parent.EmitEventAsync(eventName, value, bubbleEvent, false, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                handled = await dc.EmitEventAsync(eventName, value, bubbleEvent, false, cancellationToken).ConfigureAwait(false);
            }

            return await dc.EndDialogAsync(handled, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{EventName?.ToString() ?? string.Empty}]";
        }
    }
}
