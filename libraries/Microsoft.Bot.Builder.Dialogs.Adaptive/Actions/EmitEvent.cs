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
    /// Action which emits an event declaratively.
    /// </summary>
    public class EmitEvent : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.EmitEvent";

        private Expression eventValue;
        private Expression disabled;

        [JsonConstructor]
        public EmitEvent(string eventName = null, string eventValue = null, bool bubble = false, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.EventName = eventName;
            this.EventValue = eventValue;
            this.BubbleEvent = bubble;
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
        /// Gets or sets the name of the event to emit.
        /// </summary>
        /// <value>
        /// The name of the event to emit.
        /// </value>
        [JsonProperty("eventName")]
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the memory property path to use to get the value to send as part of the event.
        /// </summary>
        /// <value>
        /// The memory property path to use to get the value to send as part of the event.
        /// </value>
        [JsonProperty("eventValue")]
        public string EventValue
        {
            get { return eventValue?.ToString(); }
            set { this.eventValue = (value != null) ? new ExpressionEngine().Parse(value) : null; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the event should bubble to parents or not.
        /// </summary>
        /// <value>
        /// A value indicating whether gets or sets whether the event should bubble to parents or not.
        /// </value>
        [JsonProperty("bubbleEvent")]
        public bool BubbleEvent { get; set; }

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

            var handled = false;
            if (eventValue != null)
            {
                var (value, valueError) = this.eventValue.TryEvaluate(dc.GetState());
                if (valueError == null)
                {
                    handled = await dc.EmitEventAsync(EventName, value, BubbleEvent, false, cancellationToken).ConfigureAwait(false);
                } 
                else 
                {
                    throw new Exception($"Expression evaluation resulted in an error. Expression: {eventValue.ToString()}. Error: {valueError}");
                }               
            }
            else
            {
                handled = await dc.EmitEventAsync(EventName, EventValue, BubbleEvent, false, cancellationToken).ConfigureAwait(false);
            }

            return await dc.EndDialogAsync(handled, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{EventName ?? string.Empty}]";
        }
    }
}
