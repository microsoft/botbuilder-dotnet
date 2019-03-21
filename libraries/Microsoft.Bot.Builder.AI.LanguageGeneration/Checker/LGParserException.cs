using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration.Checker
{
    public class LGParserException : Exception
    {
        public LGParserException(string message) : base(message)
        {
        }
    }
}
