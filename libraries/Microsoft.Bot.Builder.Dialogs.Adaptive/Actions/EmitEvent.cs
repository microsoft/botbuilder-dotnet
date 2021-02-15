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
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.EmitEvent";

        /// <summary>
        /// Initializes a new instance of the <see cref="EmitEvent"/> class.
        /// </summary>
        /// <param name="eventName">Name of the event to emit.</param>
        /// <param name="eventValue">Memory property path to use to get the value to send as part of the event.</param>
        /// <param name="bubble">Value indicating whether the event should bubble to parents or not.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
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

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{EventName?.ToString() ?? string.Empty}]";
        }
    }
}
