// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Default persistence scopes supported for a frame.
    /// </summary>
    public class FrameScope
    {
        /// <summary>
        /// The slots within the frame are persisted across all of a users conversations.
        /// </summary>
        public const string User = "user";

        /// <summary>
        /// The slots within the frame are persisted for an individual conversation but
        /// are shared across all users within that conversation.
        /// </summary>
        public const string Conversation = "conversation";

        /// <summary>
        /// The slots within the frame are persisted for an individual conversation and
        /// are private to the current user.
        /// </summary>
        public const string ConversationMember = "conversationMember";
    }
}
