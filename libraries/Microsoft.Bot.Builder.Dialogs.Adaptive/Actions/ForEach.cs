// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions;
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

        private const string INDEX = "dialog.foreach.index";
        private const string VALUE = "dialog.foreach.value";

        [JsonConstructor]
        public Foreach([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Gets or sets property path expression to the collection of items.
        /// </summary>
        /// <value>
        /// Property path expression to the collection of items.
        /// </value>
        [JsonProperty("itemsProperty")]
        public string ItemsProperty { get; set; }

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            dc.GetState().SetValue(INDEX, -1);
            return this.NextItemAsync(dc, cancellationToken);
        }

        protected override Task<DialogTurnResult> OnBreakLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return dc.EndDialogAsync(cancellationToken: cancellationToken);
        }

        protected override Task<DialogTurnResult> OnContinueLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return this.NextItemAsync(dc, cancellationToken);
        }

        protected override Task<DialogTurnResult> OnEndOfActionsAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default)
        {
            return this.NextItemAsync(dc, cancellationToken);
        }

        protected virtual Task<DialogTurnResult> NextItemAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            // Get list information
            var itemsProperty = new ExpressionEngine().Parse(this.ItemsProperty);
            var (itemList, error) = itemsProperty.TryEvaluate(dc.GetState());
            var list = JArray.FromObject(itemList);
            var index = dc.GetState().GetIntValue(INDEX);

            // Next item
            if (++index < list.Count)
            {
                // Persist index and value
                dc.GetState().SetValue(VALUE, list[index]);
                dc.GetState().SetValue(INDEX, index);

                // Start loop
                return this.BeginActionAsync(dc, 0, cancellationToken);
            }
            else
            {
                // End of list has been reached
                return dc.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({this.ItemsProperty})";
        }
    }
}
