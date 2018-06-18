using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    /// <summary>
    /// The memory transcript store stores transcripts in volatile memory in a Dictionary.
    /// </summary>
    /// <note>
    /// Because this uses an unbounded volitile dictionary this should only be used for unit tests or non-production environments
    /// </note>
    public class MemoryTranscriptStore : ITranscriptStore
    {
        Dictionary<string, Dictionary<string, List<IActivity>>> channels = new Dictionary<string, Dictionary<string, List<IActivity>>>();

        /// <summary>
        /// Log an activity to the transcript
        /// </summary>
        /// <param name="activity">activity to log</param>
        /// <returns></returns>
        public async Task LogActivity(IActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity cannot be null for LogActivity()");

            lock (this.channels)
            {
                Dictionary<string, List<IActivity>> channel;
                if (!channels.TryGetValue(activity.ChannelId, out channel))
                {
                    channel = new Dictionary<string, List<IActivity>>();
                    channels[activity.ChannelId] = channel;
                }
                List<IActivity> transcript;
                if (!channel.TryGetValue(activity.Conversation.Id, out transcript))
                {
                    transcript = new List<IActivity>();
                    channel[activity.Conversation.Id] = transcript;
                }
                transcript.Add(activity);
            }
        }

        /// <summary>
        /// Get activities from the memory transcript store
        /// </summary>
        /// <param name="channelId">channelId</param>
        /// <param name="conversationId">conversationId</param>
        /// <param name="continuationToken"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public async Task<PagedResult<IActivity>> GetTranscriptActivities(string channelId, string conversationId, string continuationToken = null, DateTime startDate = default(DateTime))
        {
            if (channelId == null)
                throw new ArgumentNullException($"missing {nameof(channelId)}");

            if (conversationId == null)
                throw new ArgumentNullException($"missing {nameof(conversationId)}");

            var pagedResult = new PagedResult<IActivity>();
            lock (this.channels)
            {
                Dictionary<string, List<IActivity>> channel;
                if (channels.TryGetValue(channelId, out channel))
                {
                    List<IActivity> transcript;
                    if (channel.TryGetValue(conversationId, out transcript))
                    {
                        if (continuationToken != null)
                        {
                            pagedResult.Items = transcript
                                .OrderBy(a => a.Timestamp)
                                .Where(a => a.Timestamp >= startDate)
                                .SkipWhile(a => a.Id != continuationToken)
                                .Skip(1)
                                .Take(20)
                                .ToArray();
                            if (pagedResult.Items.Count() == 20)
                                pagedResult.ContinuationToken = pagedResult.Items.Last().Id;
                        }
                        else
                        {
                            pagedResult.Items = transcript
                                .OrderBy(a => a.Timestamp)
                                .Where(a => a.Timestamp >= startDate)
                                .Take(20)
                                .ToArray();
                            if (pagedResult.Items.Count() == 20)
                                pagedResult.ContinuationToken = pagedResult.Items.Last().Id;
                        }
                    }
                }
            }
            return pagedResult;
        }

        /// <summary>
        /// Delete a conversation
        /// </summary>
        /// <param name="channelId">channelid for the conversation</param>
        /// <param name="conversationId">conversation id</param>
        /// <returns></returns>
        public async Task DeleteTranscript(string channelId, string conversationId)
        {
            if (channelId == null)
                throw new ArgumentNullException($"{nameof(channelId)} should not be null");

            if (conversationId == null)
                throw new ArgumentNullException($"{nameof(conversationId)} should not be null");

            lock (this.channels)
            {
                Dictionary<string, List<IActivity>> channel;
                if (channels.TryGetValue(channelId, out channel))
                {
                    if (channel.ContainsKey(conversationId))
                        channel.Remove(conversationId);
                }
            }
        }


        /// <summary>
        /// List conversations in a channel 
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        public async Task<PagedResult<Transcript>> ListTranscripts(string channelId, string continuationToken = null)
        {
            if (channelId == null)
                throw new ArgumentNullException($"missing {nameof(channelId)}");

            var pagedResult = new PagedResult<Transcript>();
            lock (this.channels)
            {
                Dictionary<string, List<IActivity>> channel;
                if (channels.TryGetValue(channelId, out channel))
                {
                    if (continuationToken != null)
                    {
                        pagedResult.Items = channel.Select(c => new Transcript()
                        {
                            ChannelId = channelId,
                            Id = c.Key,
                            Created = c.Value.FirstOrDefault()?.Timestamp ?? default(DateTimeOffset)
                        })
                        .OrderBy(c => c.Created)
                        .SkipWhile(c => c.Id != continuationToken)
                        .Skip(1)
                        .Take(20)
                        .ToArray();

                        if (pagedResult.Items.Count() == 20)
                            pagedResult.ContinuationToken = pagedResult.Items.Last().Id;
                    }
                    else
                    {
                        pagedResult.Items = channel.Select(c => new Transcript()
                        {
                            ChannelId = channelId,
                            Id = c.Key,
                            Created = c.Value.FirstOrDefault()?.Timestamp ?? default(DateTimeOffset)
                        })
                        .OrderBy(c => c.Created)
                        .Take(20)
                        .ToArray();
                        if (pagedResult.Items.Count() == 20)
                            pagedResult.ContinuationToken = pagedResult.Items.Last().Id;
                    }
                }
            }
            return pagedResult;
        }

    }
}
