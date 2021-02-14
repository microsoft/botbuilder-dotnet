// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Executes a set of actions once for each item in an in-memory list or collection.
    /// </summary>
    public class Foreach : ActionScope
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.Foreach";

        /// <summary>
        /// Initializes a new instance of the <see cref="Foreach"/> class.
        /// </summary>
        /// <param name="sourceFilePath">Optional, full path of the source file that contains the caller.</param>
        /// <param name="sourceLineNumber">optional, line number in the source file at which the method is called.</param>
        [JsonConstructor]
        public Foreach([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
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
        /// Gets or sets property path expression to the collection of items.
        /// </summary>
        /// <value>
        /// Property path expression to the collection of items.
        /// </value>
        [JsonProperty("itemsProperty")]
        public StringExpression ItemsProperty { get; set; }

        /// <summary>
        /// Gets or sets property path expression to item index.
        /// </summary>
        /// <value>
        /// Property path expression to the item index.
        /// </value>
        [JsonProperty("index")]
        public StringExpression Index { get; set; } = "dialog.foreach.index";

        /// <summary>
        /// Gets or sets property path expression to item value.
        /// </summary>
        /// <value>
        /// Property path expression to the item value.
        /// </value>
        [JsonProperty("value")]
        public StringExpression Value { get; set; } = "dialog.foreach.value";

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

            dc.State.SetValue(Index.GetValue(dc.State), -1);
            return await this.NextItemAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when a returning control to this dialog with an <see cref="ActionScopeResult"/>
        /// with the property ActionCommand set to <c>BreakLoop</c>.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="actionScopeResult">Contains the actions scope result.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task<DialogTurnResult> OnBreakLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when a returning control to this dialog with an <see cref="ActionScopeResult"/>
        /// with the property ActionCommand set to <c>ContinueLoop</c>.
        /// </summary>
        /// <param name="dc">The dialog context for the current turn of the conversation.</param>
        /// <param name="actionScopeResult">Contains the actions scope result.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task<DialogTurnResult> OnContinueLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return await this.NextItemAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Called when the dialog's action ends.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="result">Optional, value returned from the dialog that was called. The type
        /// of the value returned is dependent on the child dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task<DialogTurnResult> OnEndOfActionsAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default)
        {
            return await this.NextItemAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Calls the next item in the stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual async Task<DialogTurnResult> NextItemAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            // Get list information
            var list = dc.State.GetValue<JArray>(this.ItemsProperty.GetValue(dc.State));
            var index = dc.State.GetIntValue(Index.GetValue(dc.State));

            // Next item
            if (list != null && ++index < list.Count)
            {
                // Persist index and value
                dc.State.SetValue(Value.GetValue(dc.State), list[index]);
                dc.State.SetValue(Index.GetValue(dc.State), index);

                // Start loop
                return await this.BeginActionAsync(dc, 0, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // End of list has been reached, or the list is null
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}({this.ItemsProperty?.ToString()})";
        }
    }
}
