// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Rules
{
    public static partial class Extensions
    {
        public static async Task DebuggerStepAsync(this DialogContext context, IRule rule, DialogEvent dialogEvent, CancellationToken cancellationToken, [CallerMemberName]string memberName = null)
        {
            var more = $"{memberName}-{dialogEvent.Name}";
            await context.GetDebugger().StepAsync(context, rule, more, cancellationToken).ConfigureAwait(false);
        }
    }
}
