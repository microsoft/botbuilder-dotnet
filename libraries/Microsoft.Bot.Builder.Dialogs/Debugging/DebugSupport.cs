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
        private static ISourceMap staticSourceMap = Debugging.SourceMap.Instance;
        private static AsyncLocal<ISourceMap> asyncLocalSourceMap = new AsyncLocal<ISourceMap>();

        /// <summary>
        /// Gets or sets a value indicating whether to use async local values for 
        /// <see cref="SourceMap"/>. This enables multiple concurrent operations 
        /// to each use their own <see cref="ISourceMap"/>.
        /// </summary>
        /// <value>True if the <see cref="SourceMap"/> is local to the async 
        /// context, otherwise false.</value>
        public static bool UseAsyncLocal { get; set; }

        /// <summary>
        /// Gets or sets the source map instance.
        /// </summary>
        /// <value>The <see cref="SourceMap"/> instance.</value>
        public static ISourceMap SourceMap
        {
            get
            {
                if (UseAsyncLocal)
                {
                    if (asyncLocalSourceMap.Value == null)
                    {
                        asyncLocalSourceMap.Value = new SourceMap();
                    }

                    return asyncLocalSourceMap.Value;
                }
                else
                {
                    return staticSourceMap;
                }
            }

            set
            {
                if (UseAsyncLocal)
                {
                    asyncLocalSourceMap.Value = staticSourceMap;
                }
                else
                {
                    staticSourceMap = value;
                }
            }
        }

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
