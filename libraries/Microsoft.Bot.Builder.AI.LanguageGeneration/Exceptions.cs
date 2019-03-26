using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class LGParsingException : Exception
    {
        public LGParsingException(string message)
            : base(message)
        {

        }

    }

    public class LGEvaluatingException : Exception
    {
        public LGEvaluatingException(string message)
            : base(message)
        {

        }

    }
}
