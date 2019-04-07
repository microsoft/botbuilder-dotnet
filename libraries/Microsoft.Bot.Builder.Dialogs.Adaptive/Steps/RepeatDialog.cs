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
    public class RepeatDialog : DialogCommand
    {
        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            object originalOptions;
            bool originalOptionsFound = dc.State.Dialog.TryGetValue("options", out originalOptions);

            options = options == null ? originalOptions : options.Merge(originalOptions ?? new object());
            return await RepeatParentDialogAsync(dc, options, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"RepeatDialog({BindingPath()})";
        }
    }
}
