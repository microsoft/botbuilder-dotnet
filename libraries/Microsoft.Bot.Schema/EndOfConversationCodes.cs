// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines values for EndOfConversationCodes.
    /// </summary>
    public static class EndOfConversationCodes
    {
        /// <summary>
        /// The code value for unknown end of conversations.
        /// </summary>
        public const string Unknown = "unknown";

        /// <summary>
        /// The code value for successful end of conversations.
        /// </summary>
        public const string CompletedSuccessfully = "completedSuccessfully";

        /// <summary>
        /// The code value for user cancelled end of conversations.
        /// </summary>
        public const string UserCancelled = "userCancelled";

        /// <summary>
        /// The code value for bot time out end of conversations.
        /// </summary>
        public const string BotTimedOut = "botTimedOut";

        /// <summary>
        /// The code value for bot-issued invalid message end of conversations.
        /// </summary>
        public const string BotIssuedInvalidMessage = "botIssuedInvalidMessage";

        /// <summary>
        /// The code value for channel failed end of conversations.
        /// </summary>
        public const string ChannelFailed = "channelFailed";
    }
}
