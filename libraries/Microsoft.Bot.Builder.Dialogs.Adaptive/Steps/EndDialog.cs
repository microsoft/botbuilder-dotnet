// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Steps
{
    /// <summary>
    /// Command to end the current dialog, returning the resultProperty as the result of the dialog.
    /// </summary>
    public class EndDialog : DialogCommand
    {
        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = Property != null ? dc.State.GetValue<string>(Property) : null;
            return await EndParentDialogAsync(dc, result, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"end({this.Property ?? string.Empty})";
        }
    }
}
