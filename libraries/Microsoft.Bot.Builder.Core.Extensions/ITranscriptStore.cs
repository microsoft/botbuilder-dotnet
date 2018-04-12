using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.Extensions
{

    public class Conversation
    {
        /// <summary>
        /// ChannelId 
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Conversation Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Date conversation was started
        /// </summary>
        public DateTimeOffset Created { get; set; }
    }

    /// <summary>
    /// Page of results from an enumeration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Page of items
        /// </summary>
        public T[] Items { get; set; } = new T[0];

        /// <summary>
        /// Token used to page through multiple pages
        /// </summary>
        public string ContinuationToken { get; set; }
    }


    /// <summary>
    /// Transcript logger stores activities for conversations for recall
    /// </summary>
    public interface ITranscriptStore
    {
        /// <summary>
        /// Log an activity to the transcript
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        Task LogActivity(IActivity activity);

        /// <summary>
        /// Get activities for a conversation (Aka the transcript)
        /// </summary>
        /// <param name="channelId">Channel id</param>
        /// <param name="conversationId">Conversation id</param>
        /// <param name="continuationToken">continuatuation token to page through results</param>
        /// <param name="startDate">Earliest time to include.</param>
        /// <returns>Enumeration over the recorded activities.</returns>
        Task<PagedResult<IActivity>> GetConversationActivities(string channelId, string conversationId, string continuationToken=null, DateTime startDate = default(DateTime));

        /// <summary>
        /// List conversations in the channelId
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="continuationToken">continuation token to get next page of results</param>
        /// <returns></returns>
        Task<PagedResult<Conversation>> ListConversations(string channelId, string continuationToken=null);

        /// <summary>
        /// Delete a specific conversation and all of it's activities
        /// </summary>
        /// <param name="channelId">Channel where conversation took place.</param>
        /// <param name="conversationId">Id of conversation to delete.</param>
        /// <returns>Task.</returns>
        Task DeleteConversation(string channelId, string conversationId);
    }
}
