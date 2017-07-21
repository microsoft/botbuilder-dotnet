using System;
using System.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Codes indicating why a conversation has ended
    /// </summary>
    public class EndOfConversationCodes
    {
        /// <summary>
        /// The conversation was ended for unknown reasons
        /// </summary>
        public const string Unknown = "unknown";

        /// <summary>
        /// The conversation completed successfully
        /// </summary>
        public const string CompletedSuccessfully = "completedSuccessfully";

        /// <summary>
        /// The user cancelled the conversation
        /// </summary>
        public const string UserCancelled = "userCancelled";

        /// <summary>
        /// The conversation was ended because requests sent to the bot timed out
        /// </summary>
        public const string BotTimedOut = "botTimedOut";

        /// <summary>
        /// The conversation was ended because the bot sent an invalid message
        /// </summary>
        public const string BotIssuedInvalidMessage = "botIssuedInvalidMessage";

        /// <summary>
        /// The conversation ended because the channel experienced an internal failure
        /// </summary>
        public const string ChannelFailed = "channelFailed";
    }
}
