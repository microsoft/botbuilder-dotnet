using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Flow;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Contract
{
    public class DialogInfo
    {
        public string Id { get; set; }
        public IDialog Dialog { get; set; }
        public List<IDialogCommand> Commands { get; set; }
    }
}
