using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Planning
{
    public class BotState : DialogState
    {
        public string LastAccess { get; set; }

    }
}
