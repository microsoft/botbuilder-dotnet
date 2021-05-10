// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Debugging;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Extension method for <see cref="DialogContext"/> provides <see cref="DebugSupport"/>.
    /// </summary>
#pragma warning disable CA1724 // Type names should not match namespaces (by design and we can't change this without breaking binary compat)
    public static partial class Extensions
#pragma warning restore CA1724 // Type names should not match namespaces
    {
        /// <summary>
        /// Call into active IDialogDebugger and let it know that we are at given point.
        /// </summary>
        /// <param name="context">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="conditional">Condition to trigger the event.</param>
        /// <param name="dialogEvent">The event being raised.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for this task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task DebuggerStepAsync(this DialogContext context, OnCondition conditional, DialogEvent dialogEvent, CancellationToken cancellationToken)
        {
            await context.GetDebugger().StepAsync(context, conditional, more: dialogEvent?.Name ?? string.Empty, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a value from a string expression, using a <see cref="TextTemplate"/>.
        /// </summary>
        /// <param name="stringExpression">The <see cref="StringExpression"/> to evaluate.</param>
        /// <param name="dc">The current <see cref="DialogContext"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> for this call.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<string> GetValueAsync(this StringExpression stringExpression, DialogContext dc, CancellationToken cancellationToken)
        {
            string text = await new TextTemplate(stringExpression.ExpressionText)
             .BindAsync(dc, cancellationToken: cancellationToken)
             .ConfigureAwait(false) ?? stringExpression.GetValue(dc.State);

            if (!string.IsNullOrEmpty(text) && text.StartsWith("=", StringComparison.OrdinalIgnoreCase))
            {
                text = AdaptiveExpressions.Expression.Parse(text).TryEvaluate<string>(dc.State).value;
            }

            return text;
        }
    }
}
