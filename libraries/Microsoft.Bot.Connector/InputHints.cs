using System;
using System.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Indicates whether the bot is accepting, expecting, or ignoring input
    /// </summary>
    public static class InputHints
    {
        /// <summary> 
        /// The sender is passively ready for input but is not waiting on a response.
        /// </summary> 
        public const string AcceptingInput = "acceptingInput";

        /// <summary>
        /// The sender is ignoring input. Bots may send this hint if they are actively processing a request and will ignore input
        /// from users until the request is complete.
        /// </summary> 
        public const string IgnoringInput = "ignoringInput";

        /// <summary>
        /// The sender is actively expecting a response from the user.
        /// </summary> 
        public const string ExpectingInput = "expectingInput";
    }
}
