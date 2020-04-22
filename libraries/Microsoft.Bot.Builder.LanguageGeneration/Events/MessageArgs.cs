using System;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class MessageArgs : EventArgs
    {
        public string Type { get; } = EventTypes.Message;

        public string Source { get; set; }

        /// <summary>
        /// Gets or sets LGContext, may include evaluation stack.
        /// </summary>
        /// <value>
        /// LGContext.
        /// </value>
        public object Context { get; set; }

        public string Text { get; set; }
    }
}
