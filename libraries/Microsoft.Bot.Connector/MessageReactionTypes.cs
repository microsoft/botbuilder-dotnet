using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Message reaction types
    /// </summary>
    /// <remarks>
    /// Message reactions are an open set. This class contains some common values.
    /// </remarks>
    public static class MessageReactionTypes
    {
        /// <summary>
        /// Like
        /// </summary>
        public const string Like = "like";

        /// <summary>
        /// +1
        /// </summary>
        public const string PlusOne = "plusOne";
    }
}
