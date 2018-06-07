using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Prompts
{
    public class BasicPrompt<T> : Prompt<T> where T: PromptResult
    {
        protected BasicPrompt()
        {
        }
    }
}
