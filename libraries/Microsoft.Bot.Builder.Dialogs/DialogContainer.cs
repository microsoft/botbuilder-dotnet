// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs
{
    public abstract class DialogContainer : Dialog
    {
#pragma warning disable SA1401 // Fields should be private
        protected readonly DialogSet _dialogs = new DialogSet();
#pragma warning restore SA1401 // Fields should be private

        public DialogContainer(string dialogId = null)
            : base(dialogId)
        {
        }

        public abstract DialogContext CreateChildContext(DialogContext dc);

        public virtual Dialog FindDialog(string dialogId)
        {
            return this._dialogs.Find(dialogId);
        }
    }
}
