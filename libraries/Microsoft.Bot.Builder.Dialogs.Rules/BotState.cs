using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Rules
{
    public class BotState : DialogState
    {
        public string LastAccess { get; set; }

    }
}
