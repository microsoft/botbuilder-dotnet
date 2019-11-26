// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1402 // File may only contain a single type

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Tests
{
    public class LamdaDialog : Dialog
    {
        private Func<DialogContext, CancellationToken, Task> handler;

        public LamdaDialog(Func<DialogContext, CancellationToken, Task> handler)
        {
            this.handler = handler;
        }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            await this.handler(dc, cancellationToken);
            return await dc.EndDialogAsync();
        }
    }
}
