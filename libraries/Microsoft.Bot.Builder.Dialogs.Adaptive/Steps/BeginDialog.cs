// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    /// <summary>
    /// Step which begins executing another dialog, when it is done, it will return to the caller
    /// </summary>
    public class BeginDialog : BaseInvokeDialog
    {
        public BeginDialog(string dialogIdToCall = null, string id = null, string property = null, object options = null)
            : base(dialogIdToCall, id, property, options)
        {
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            Options = Options.Merge(options ?? new object());
            var dialog = this.resolveDialog(dc);
            return await dc.BeginDialogAsync(dialog.Id, Options, cancellationToken).ConfigureAwait(false);
        }
    }
}
