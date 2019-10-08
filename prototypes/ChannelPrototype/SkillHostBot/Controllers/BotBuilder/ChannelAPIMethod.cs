using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillHost.Controllers
{
    public enum ChannelAPIMethod
    {
        /// <summary>
        /// ReplyToActivity(converationId, activity)
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
    }
}
