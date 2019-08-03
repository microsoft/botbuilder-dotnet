// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Events
{
    public static partial class Extensions
    {
        public static async Task DebuggerStepAsync(this DialogContext context, IOnEvent rule, DialogEvent dialogEvent,  CancellationToken cancellationToken)
        {
            var more = dialogEvent.Name;
            await context.GetDebugger().StepAsync(context, rule, more, cancellationToken).ConfigureAwait(false);
        }
    }
}
