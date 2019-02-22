// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class ComponentDialog : ComponentDialogBase
    {
        public ComponentDialog(string dialogId = null)
            : base(dialogId)
        {
        }

        public string InitialDialogId { get; set; }

        protected override Task OnInitialize(DialogContext dc)
        {
            if (this.InitialDialogId == null)
            {
                this.InitialDialogId = _dialogs.GetDialogs().FirstOrDefault()?.Id;
            }

            return base.OnInitialize(dc);
        }

        protected override Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            return innerDc.BeginDialogAsync(InitialDialogId, options, cancellationToken);
        }
    }
}
