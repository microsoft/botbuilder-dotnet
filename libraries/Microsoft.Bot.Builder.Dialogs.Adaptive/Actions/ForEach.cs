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
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.Foreach";

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

            dc.State.SetValue(Index.GetValue(dc.State), -1);
            return await this.NextItemAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<DialogTurnResult> OnBreakLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<DialogTurnResult> OnContinueLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return await this.NextItemAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<DialogTurnResult> OnEndOfActionsAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default)
        {
            return await this.NextItemAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<DialogTurnResult> NextItemAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            // Get list information
            var list = dc.State.GetValue<JArray>(this.ItemsProperty.GetValue(dc.State));
            var index = dc.State.GetIntValue(Index.GetValue(dc.State));

            // Next item
            if (++index < list.Count)
            {
                // Persist index and value
                dc.State.SetValue(Value.GetValue(dc.State), list[index]);
                dc.State.SetValue(Index.GetValue(dc.State), index);

                // Start loop
                return await this.BeginActionAsync(dc, 0, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // End of list has been reached
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({this.ItemsProperty?.ToString()})";
        }
    }
}
