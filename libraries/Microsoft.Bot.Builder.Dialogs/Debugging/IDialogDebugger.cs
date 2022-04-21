// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// Defines the Dialog Debugger interface.
    /// </summary>
    public interface IDialogDebugger
    {
        /// <summary>
        /// Gets or sets a value indicating whether to trace steps.
        /// </summary>
        /// <value>
        /// true ot enable tracing steps.
        /// </value>
        public bool TraceSteps { get; set; }

        /// <summary>
        /// Task representing information in a given point of an item.
        /// </summary>
        /// <param name="context">The <see cref="DialogContext"/> object for this turn.</param>
        /// <param name="item">Object item in debugger.</param>
        /// <param name="more">Additional information.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task StepAsync(DialogContext context, object item, string more, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Defines the Debugger interface.
    /// </summary>
    public interface IDebugger
    {
        /// <summary>
        /// Task representing a debug output.
        /// </summary>
        /// <param name="text">Text to output.</param>
        /// <param name="item">Object item in debugger.</param>
        /// <param name="value">Value of the object to output.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task OutputAsync(string text, object item, object value, CancellationToken cancellationToken);
    }
}
