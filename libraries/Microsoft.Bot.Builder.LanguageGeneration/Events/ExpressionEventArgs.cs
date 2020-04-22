
using System;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class ExpressionEventArgs : EventArgs
    {
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets LGContext, may include evaluation stack.
        /// </summary>
        /// <value>
        /// LGContext.
        /// </value>
        public object Context { get; set; }
    }
}
