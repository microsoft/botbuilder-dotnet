// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs
{
    public abstract class DialogContainer : Dialog
    {       
        protected DialogContainer(string dialogId = null)
            : base(dialogId)
        {
        }

        [JsonIgnore]
        public DialogSet Dialogs { get; set; } = new DialogSet();

        public abstract DialogContext CreateChildContext(DialogContext dc);

        public virtual Dialog FindDialog(string dialogId)
        {
            return this.Dialogs.Find(dialogId);
        }
    }
}
