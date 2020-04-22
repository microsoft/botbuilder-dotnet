using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.LanguageGeneration.Events
{
    public class RegisteSourceMapArgs : EventArgs
    {
        public string Type { get; } = EventTypes.RegisteSourceMap;

        /// <summary>
        /// Gets or sets LGContext, may include evaluation stack.
        /// </summary>
        /// <value>
        /// LGContext.
        /// </value>
        public SourceRange SourceRange { get; set; }
    }
}
