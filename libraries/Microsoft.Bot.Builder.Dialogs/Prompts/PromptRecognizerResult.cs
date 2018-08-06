using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs
{
    public class PromptRecognizerResult<T>
    {
        public PromptRecognizerResult()
        {
            Succeeded = false;
        }

        public bool Succeeded { get; set; }

        public T Value { get; set; }
    }
}
