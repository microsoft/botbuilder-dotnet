// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// Debugger support for <see cref="ITurnContext"/>, <see cref="DialogContext"/>. 
    /// </summary>
    public static class DebugSupport
    {
        public static ISourceMap SourceMap { get; set; } = Debugging.SourceMap.Instance;

        /// <summary>
        /// Extension method to get IDialogDebugger from TurnContext.
        /// </summary>
        /// <param name="context">turnContext.</param>
        /// <returns>IDialogDebugger.</returns>
        public static IDialogDebugger GetDebugger(this ITurnContext context) =>
            context.TurnState.Get<IDialogDebugger>() ?? NullDialogDebugger.Instance;

        /// <summary>
        /// Extension method to get IDialogDebugger from DialogContext.
        /// </summary>
        /// <param name="context">dialogContext.</param>
        /// <returns>IDialogDebugger.</returns>
        public static IDialogDebugger GetDebugger(this DialogContext context) =>
            context.Context.GetDebugger();

        /// <summary>
        /// Call into active IDialogDebugger and let it know that we are at given point in the dialog.
        /// </summary>
        /// <param name="context">dialogContext.</param>
        /// <param name="dialog">dialog.</param>
        /// <param name="more">label.</param>
        /// <param name="cancellationToken">cancellation token for async operations.</param>
        /// <returns>async task.</returns>
        public static async Task DebuggerStepAsync(this DialogContext context, Dialog dialog, string more, CancellationToken cancellationToken)
        {
            await context.GetDebugger().StepAsync(context, dialog, more, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Call into active IDialogDebugger and let it know that we are at given point in the Recognizer.
        /// </summary>
        /// <param name="context">dialogContext.</param>
        /// <param name="recognizer">recognizer.</param>
        /// <param name="more">label.</param>
        /// <param name="cancellationToken">cancellation token for async operations.</param>
        /// <returns>async task.</returns>
        public static async Task DebuggerStepAsync(this DialogContext context, IRecognizer recognizer, string more, CancellationToken cancellationToken)
        {
            await context.GetDebugger().StepAsync(context, recognizer, more, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Call into active IDialogDebugger and let it know that we are at given point in the Recognizer.
        /// </summary>
        /// <param name="context">dialogContext.</param>
        /// <param name="recognizer">recognizer.</param>
        /// <param name="more">label.</param>
        /// <param name="cancellationToken">cancellation token for async operations.</param>
        /// <returns>async task.</returns>
        public static async Task DebuggerStepAsync(this DialogContext context, Recognizer recognizer, string more, CancellationToken cancellationToken)
        {
            await context.GetDebugger().StepAsync(context, recognizer, more, cancellationToken).ConfigureAwait(false);
        }
    }
}
