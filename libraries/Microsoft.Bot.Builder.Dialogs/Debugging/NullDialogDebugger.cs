// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// Default Dialog Debugger which simply ignores step calls for the IDialogDebuggerinterface.
    /// </summary>
    public class NullDialogDebugger : IDialogDebugger
    {
        public static readonly IDialogDebugger Instance = new NullDialogDebugger();

        private NullDialogDebugger()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether to trace steps.
        /// </summary>
        /// <value>
        /// true ot enable tracing steps.
        /// </value>
        public bool TraceSteps { get; set; } = true;

        Task IDialogDebugger.StepAsync(DialogContext context, object item, string more, CancellationToken cancellationToken)
        {
            if (TraceSteps)
            {
                var activity = context.Context.Activity;
                var turnText = activity.Text?.Trim() ?? string.Empty;
                if (turnText.Length == 0)
                {
                    turnText = activity.Type;
                }

                string name = string.Empty;
                if (item is Dialog dialog)
                {
                    name = dialog.Id;
                }
                else if (item is IItemIdentity identity)
                {
                    name = identity.GetIdentity();
                }
                else
                {
                    name = item?.GetType().Name ?? "null";
                }

                var threadText = $"'{Ellipsis(turnText, 18)}'";
                System.Diagnostics.Trace.TraceInformation($"{threadText} ==> {more?.PadRight(16) ?? string.Empty} ==> {name} ");
            }

            return Task.CompletedTask;
        }

        private static string Ellipsis(string text, int length)
        {
            if (text == null)
            {
                return string.Empty;
            }

            if (text.Length > length)
            {
                int pos = text.IndexOf(" ", length);
                if (pos >= 0)
                {
                    return text.Substring(0, pos) + "...";
                }
            }

            return text;
        }
    }
}
