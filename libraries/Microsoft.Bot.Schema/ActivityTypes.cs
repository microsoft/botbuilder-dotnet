// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Defines values for ActivityTypes.
    /// </summary>
    public static class ActivityTypes
    {
        /// <summary>
        /// The type value for contact relation update activities.
        /// </summary>
        public const string ContactRelationUpdate = "contactRelationUpdate";

        /// <summary>
        /// The type value for conversation update activities.
        /// </summary>
        public const string ConversationUpdate = "conversationUpdate";

        /// <summary>
        /// The type value for end of conversation activities.
        /// </summary>
        public const string EndOfConversation = "endOfConversation";
        
        /// <summary>
        /// The type value for event activities.
        /// </summary>
        public const string Event = "event";

        /// <summary>
        /// The type value for delay activities.
        /// </summary>
        /// <remarks>As an outgoing activity type, causes the adapter to pause for <see cref="Activity.Value"/> milliseconds.
        /// The activity's <see cref="Activity.Value"/> should be a <see cref="int"/>.</remarks>
        public const string Delay = "delay";

        /// <summary>
        /// The type value for delete user data activities.
        /// </summary>
        public const string DeleteUserData = "deleteUserData";

        /// <summary>
        /// The type value for handoff activities.
        /// </summary>
        public const string Handoff = "handoff";

        /// <summary>
        /// The type value for installation update activities.
        /// </summary>
        public const string InstallationUpdate = "installationUpdate";

        /// <summary>
        /// The type value for invoke activities.
        /// </summary>
        public const string Invoke = "invoke";

        /// <summary>
        /// The type value for message activities.
        /// </summary>
        public const string Message = "message";

        /// <summary>
        /// The type value for message delete activities.
        /// </summary>
        public const string MessageDelete = "messageDelete";

        /// <summary>
        /// The type value for message reaction activities.
        /// </summary>
        public const string MessageReaction = "messageReaction";

        /// <summary>
        /// The type value for message update activities.
        /// </summary>
        public const string MessageUpdate = "messageUpdate";
        
        /// <summary>
        /// The type value for suggestion activities.
        /// </summary>
        public const string Suggestion = "suggestion";

        /// <summary>
        /// The type value for trace activities.
        /// </summary>
        public const string Trace = "trace";

        /// <summary>
        /// The type value for typing activities.
        /// </summary>
        public const string Typing = "typing";

        /// <summary>
        /// The type value for command activities.
        /// </summary>
        public const string Command = "command";

        /// <summary>
        /// The type value for command result activities.
        /// </summary>
        public const string CommandResult = "commandResult";
    }
}
