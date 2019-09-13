// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.TriggerHandlers
{
    /// <summary>
    /// Extension method for <see cref="DialogContext"/> provides <see cref="DebugSupport"/>.
    /// </summary>
    public static partial class Extensions
    {
        public static async Task DebuggerStepAsync(this DialogContext context, TriggerHandler triggerHandler, DialogEvent dialogEvent, CancellationToken cancellationToken)
        {
            await context.GetDebugger().StepAsync(context, triggerHandler, more: dialogEvent?.Name ?? string.Empty, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
