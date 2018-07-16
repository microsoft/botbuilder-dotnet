// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogContainer : IDialogContinue
    {
        protected DialogSet Dialogs { get; set; }

        protected string DialogId { get; set; }

        public DialogContainer(string dialogId, DialogSet dialogs = null)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                throw new ArgumentNullException(nameof(dialogId));
            }

            Dialogs = dialogs ?? new DialogSet();
            DialogId = dialogId;
        }

        public async Task DialogBeginAsync(DialogContext dc, IDictionary<string, object> dialogArgs = null)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Start the controls entry point dialog.
            IDictionary<string, object> result = null;
            var cdc = new DialogContext(this.Dialogs, dc.Context, dc.ActiveDialog.State, (r) => { result = r; });
            await cdc.BeginAsync(DialogId, dialogArgs);

            // End if the controls dialog ends.
            if (cdc.ActiveDialog == null)
            {
                await dc.EndAsync(result);
            }
        }

        public async Task DialogContinueAsync(DialogContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            // Continue controls dialog stack.
            IDictionary<string, object> result = null;
            var cdc = new DialogContext(this.Dialogs, dc.Context, dc.ActiveDialog.State, (r) => { result = r; });
            await cdc.ContinueAsync();

            // End if the controls dialog ends.
            if (cdc.ActiveDialog == null)
            {
                await dc.EndAsync(result);
            }
        }
    }
}
