using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class DialogState
    {
        public DialogState()
        {
            DialogStack = new List<DialogInstance>();
        }

        public List<DialogInstance> DialogStack { get; }
    }
}
