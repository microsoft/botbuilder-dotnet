using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    [Serializable]
    public class LGException : Exception, ISerializable
    {
        public LGException(string message, IList<Diagnostic> diagnostics)
            : base(message)
        {
            Diagnostics = diagnostics;
        }

        public IList<Diagnostic> Diagnostics { get; set; }
    }
}
