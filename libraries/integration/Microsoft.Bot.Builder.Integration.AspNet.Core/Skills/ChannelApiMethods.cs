// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Skills
{
    // TODO: change these to const strings (so we can change it without breaking compat)
    internal enum ChannelApiMethods
    {
        /// <summary>
        /// ReplyToActivity(conversationId, activity)
        /// </summary>
        ReplyToActivity,

        /// <summary>
        /// SendToConversation(activity)
        /// </summary>
        SendToConversation,

        /// <summary>
        /// UpdateActivity(activity)
        /// </summary>
        UpdateActivity,

        /// <summary>
        /// DeleteActivity(conversationId, activityId)
        /// </summary>
        DeleteActivity,

        /// <summary>
        /// SendConversationHistory(conversationId, history)
        /// </summary>
        SendConversationHistory,

        /// <summary>
        /// GetConversationMembers(conversationId)
        /// </summary>
        GetConversationMembers,

        /// <summary>
        /// GetConversationPageMembers(conversationId, (int)pageSize, continuationToken)
        /// </summary>
        GetConversationPagedMembers,

        /// <summary>
        /// DeleteConversationMember(conversationId, memberId)
        /// </summary>
        DeleteConversationMember,

        /// <summary>
        /// GetActivityMembers(conversationId, activityId)
        /// </summary>
        GetActivityMembers,

        /// <summary>
        /// UploadAttachment(conversationId, attachmentData)
        /// </summary>
        UploadAttachment,

        /// <summary>
        /// CreateConversation([FromBody] ConversationParameters parameters)
        /// Not supported for skills
        /// </summary>
        CreateConversation,

        /// <summary>
        /// GetConversations(string continuationToken = null)
        /// Not supported for skills
        /// </summary>
        GetConversations,
    }
}
