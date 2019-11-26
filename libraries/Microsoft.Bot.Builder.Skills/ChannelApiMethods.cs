// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    internal static class ChannelApiMethods
    {
        /// <summary>
        /// ReplyToActivity(conversationId, activity).
        /// </summary>
        public const string ReplyToActivity = "ReplyToActivity";

        /// <summary>
        /// SendToConversation(activity).
        /// </summary>
        public const string SendToConversation = "SendToConversation";

        /// <summary>
        /// UpdateActivity(activity).
        /// </summary>
        public const string UpdateActivity = "UpdateActivity";

        /// <summary>
        /// DeleteActivity(conversationId, activityId).
        /// </summary>
        public const string DeleteActivity = "DeleteActivity";

        /// <summary>
        /// SendConversationHistory(conversationId, history).
        /// </summary>
        public const string SendConversationHistory = "SendConversationHistory";

        /// <summary>
        /// GetConversationMembers(conversationId).
        /// </summary>
        public const string GetConversationMembers = "GetConversationMembers";

        /// <summary>
        /// GetConversationPageMembers(conversationId, (int)pageSize, continuationToken).
        /// </summary>
        public const string GetConversationPagedMembers = "GetConversationPagedMembers";

        /// <summary>
        /// DeleteConversationMember(conversationId, memberId).
        /// </summary>
        public const string DeleteConversationMember = "DeleteConversationMember";

        /// <summary>
        /// GetActivityMembers(conversationId, activityId).
        /// </summary>
        public const string GetActivityMembers = "GetActivityMembers";

        /// <summary>
        /// UploadAttachment(conversationId, attachmentData).
        /// </summary>
        public const string UploadAttachment = "UploadAttachment";

        /// <summary>
        /// CreateConversation([FromBody] ConversationParameters parameters).
        /// </summary>
        public const string CreateConversation = "CreateConversation";

        /// <summary>
        /// GetConversations(string continuationToken = null).
        /// </summary>
        public const string GetConversations = "GetConversations";
    }
}
