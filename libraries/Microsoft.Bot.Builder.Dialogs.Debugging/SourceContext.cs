using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    /// <summary>
    /// SourceContext used to build sourcemap of metadata about the call relationships between assets.
    /// </summary>
    public class SourceContext
    {
        public SourceContext()
        {
        }

        /// <summary>
        /// Gets the stack of SourceRange objects which represent the calling relationships between declarative assets.
        /// </summary>
        /// <value>
        /// The stack of SourceRange objects which represent the calling relationships between declarative assets.
        /// </value>
        public Stack<SourceRange> CallStack { get; private set; } = new Stack<SourceRange>();
    }
}
