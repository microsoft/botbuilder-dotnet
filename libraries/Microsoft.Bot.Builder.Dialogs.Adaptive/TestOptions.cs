using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// test options.
    /// </summary>
    public class TestOptions
    {
        /// <summary>
        /// Kind.
        /// </summary>
        public const string Kind = "conversation.TestOptions";

        /// <summary>
        /// Gets or sets random client.
        /// </summary>
        /// <value>
        /// Random client.
        /// </value>
        public Random Random { get; set; }
    }
}
