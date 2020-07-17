// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
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
        public static async Task DebuggerStepAsync(this DialogContext context, OnCondition conditional, DialogEvent dialogEvent, CancellationToken cancellationToken)
        {
            await context.GetDebugger().StepAsync(context, conditional, more: dialogEvent?.Name ?? string.Empty, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
