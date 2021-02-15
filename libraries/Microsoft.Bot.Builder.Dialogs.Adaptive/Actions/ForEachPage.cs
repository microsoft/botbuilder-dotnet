// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public class ForeachPage : ActionScope
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ForeachPage";

        /// <summary>
        /// Initializes a new instance of the <see cref="ForeachPage"/> class.
        /// </summary>
        /// <param name="sourceFilePath">Optional, full path of the source file that contains the caller.</param>
        /// <param name="sourceLineNumber">optional, line number in the source file at which the method is called.</param>
        [JsonConstructor]
        public ForeachPage([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
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
        /// Gets or sets the item properties list.
        /// </summary>
        /// <value>Expression used to compute the item property list that should be enumerated.</value>
        [JsonProperty("itemsProperty")]
        public StringExpression ItemsProperty { get; set; }

        /// <summary>
        /// Gets or sets the pages list.
        /// </summary>
        /// <value>Expression used to compute the pages list that should be enumerated.</value>.
        [JsonProperty("page")]
        public StringExpression Page { get; set; } = "dialog.foreach.page";

        /// <summary>
        /// Gets or sets the page indexes list.
        /// </summary>
        /// <value>Expression used to compute the page indexes list that should be enumerated.</value>.
        [JsonProperty("pageIndex")]
        public StringExpression PageIndex { get; set; } = "dialog.foreach.pageindex";

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        /// <value><see cref="IntExpression"/> with the size of the page.</value>.
        [JsonProperty("pageSize")]
        public IntExpression PageSize { get; set; } = 10;

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

            dc.State.SetValue(PageIndex.GetValue(dc.State), 0);
            return await NextPageAsync(dc, cancellationToken).ConfigureAwait(false);
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
            return await NextPageAsync(dc, cancellationToken).ConfigureAwait(false);
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
        /// <remarks>Default is to simply end the dialog and propagate to parent to handle.</remarks>
        protected override async Task<DialogTurnResult> OnContinueLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return await this.NextPageAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}({this.ItemsProperty?.ToString()})";
        }

        private async Task<DialogTurnResult> NextPageAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            int pageIndex = dc.State.GetIntValue(PageIndex.GetValue(dc.State), 0);
            int pageSize = this.PageSize.GetValue(dc.State);
            int itemOffset = pageSize * pageIndex;

            var itemsProperty = this.ItemsProperty.GetValue(dc.State);
            if (dc.State.TryGetValue<object>(itemsProperty, out object items)) 
            {
                var page = this.GetPage(items, itemOffset, pageSize);

                if (page.Any())
                {
                    dc.State.SetValue(Page.GetValue(dc.State), page);
                    dc.State.SetValue(PageIndex.GetValue(dc.State), ++pageIndex);
                    return await this.BeginActionAsync(dc, 0, cancellationToken).ConfigureAwait(false);
                }
            }

            // End of list has been reached
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private List<object> GetPage(object list, int index, int pageSize)
        {
            List<object> page = new List<object>();
            int end = index + pageSize;
            if (list != null && list.GetType() == typeof(JArray))
            {
                for (int i = index; i < end && i < JArray.FromObject(list).Count; i++)
                {
                    page.Add(JArray.FromObject(list)[i]);
                }
            }
            else if (list != null && list is JObject)
            {
                for (int i = index; i < end; i++)
                {
                    if (((JObject)list).SelectToken(i.ToString(CultureInfo.InvariantCulture)).HasValues)
                    {
                        page.Add(((JObject)list).SelectToken(i.ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }

            return page;
        }
    }
}
