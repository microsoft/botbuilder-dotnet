using System;
using System.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Types of Activities
    /// </summary>
    public static class ActivityTypes
    {
        /// <summary>
        /// Message from a user -> bot or bot -> User
        /// </summary>
        public const string Message = "message";

        /// <summary>
        /// Bot added removed to contact list
        /// </summary>
        public const string ContactRelationUpdate = "contactRelationUpdate";

        /// <summary>
        /// This notification is sent when the conversation's properties change, for example the topic name, or when user joins or leaves the group.
        /// </summary>
        public const string ConversationUpdate = "conversationUpdate";

        /// <summary>
        /// a user is typing
        /// </summary>
        public const string Typing = "typing";

        /// <summary>
        /// Bounce a message off of the server without replying or changing it's state
        /// </summary>
        public const string Ping = "ping";

        /// <summary>
        /// End a conversation
        /// </summary>
        public const string EndOfConversation = "endOfConversation";

        /// <summary>
        /// NOTE: Trigger activity has been renamed to Event activity
        /// </summary>
        [Obsolete]
        public const string Trigger = "trigger";

        /// <summary>
        /// Asynchronous external event
        /// </summary>
        public const string Event = "event";

        /// <summary>
        /// Synchronous request to invoke a command
        /// </summary>
        public const string Invoke = "invoke";

        /// <summary>
        /// Delete user data
        /// </summary>
        public const string DeleteUserData = "deleteUserData";

        /// <summary>
        /// Bot added or removed from channel
        /// </summary>
        public const string InstallationUpdate = "installationUpdate";

        /// <summary>
        /// Reaction added or removed from activity
        /// </summary>
        public const string MessageReaction = "messageReaction";
    }
}
