// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// The memory transcript store stores transcripts in volatile memory in a Dictionary.
    /// </summary>
    /// <note>
    /// Because this uses an unbounded volitile dictionary this should only be used for unit tests or non-production environments
    /// </note>
    public class MemoryTranscriptStore : ITranscriptStore
    {
        private Dictionary<string, Dictionary<string, List<IActivity>>> _channels = new Dictionary<string, Dictionary<string, List<IActivity>>>();

        /// <summary>
        /// Log an activity to the transcript
        /// </summary>
        /// <param name="activity">activity to log</param>
        /// <returns></returns>
        public Task LogActivity(IActivity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity cannot be null for LogActivity()");

            lock (_channels)
            {
                Dictionary<string, List<IActivity>> channel;
                if (!_channels.TryGetValue(activity.ChannelId, out channel))
                {
                    channel = new Dictionary<string, List<IActivity>>();
                    _channels[activity.ChannelId] = channel;
                }

                List<IActivity> transcript;
                if (!channel.TryGetValue(activity.Conversation.Id, out transcript))
                {
                    transcript = new List<IActivity>();
                    channel[activity.Conversation.Id] = transcript;
                }

                transcript.Add(activity);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get activities from the memory transcript store
        /// </summary>
        /// <param name="channelId">channelId</param>
        /// <param name="conversationId">conversationId</param>
        /// <param name="continuationToken"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public Task<PagedResult<IActivity>> GetTranscriptActivities(string channelId, string conversationId, string continuationToken = null, DateTime startDate = default(DateTime))
        {
            if (channelId == null)
                throw new ArgumentNullException($"missing {nameof(channelId)}");

            if (conversationId == null)
                throw new ArgumentNullException($"missing {nameof(conversationId)}");

            var pagedResult = new PagedResult<IActivity>();
            lock (_channels)
            {
                Dictionary<string, List<IActivity>> channel;
                if (_channels.TryGetValue(channelId, out channel))
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

            return Task.FromResult(pagedResult);
        }

        /// <summary>
        /// Delete a conversation
        /// </summary>
        /// <param name="channelId">channelid for the conversation</param>
        /// <param name="conversationId">conversation id</param>
        /// <returns></returns>
        public Task DeleteTranscript(string channelId, string conversationId)
        {
            if (channelId == null)
                throw new ArgumentNullException($"{nameof(channelId)} should not be null");

            if (conversationId == null)
                throw new ArgumentNullException($"{nameof(conversationId)} should not be null");

            lock (_channels)
            {
                Dictionary<string, List<IActivity>> channel;
                if (_channels.TryGetValue(channelId, out channel))
                {
                    if (channel.ContainsKey(conversationId))
                    {
                        channel.Remove(conversationId);
                    }
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// List conversations in a channel.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        public Task<PagedResult<Transcript>> ListTranscripts(string channelId, string continuationToken = null)
        {
            if (channelId == null)
                throw new ArgumentNullException($"missing {nameof(channelId)}");

            var pagedResult = new PagedResult<Transcript>();
            lock (_channels)
            {
                Dictionary<string, List<IActivity>> channel;
                if (_channels.TryGetValue(channelId, out channel))
                {
                    if (continuationToken != null)
                    {
                        pagedResult.Items = channel.Select(c => new Transcript()
                        {
                            ChannelId = channelId,
                            Id = c.Key,
                            Created = c.Value.FirstOrDefault()?.Timestamp ?? default(DateTimeOffset),
                        })
                        .OrderBy(c => c.Created)
                        .SkipWhile(c => c.Id != continuationToken)
                        .Skip(1)
                        .Take(20)
                        .ToArray();

                        if (pagedResult.Items.Count() == 20)
                        {
                            pagedResult.ContinuationToken = pagedResult.Items.Last().Id;
                        }
                    }
                    else
                    {
                        pagedResult.Items = channel.Select(c => new Transcript
                        {
                            ChannelId = channelId,
                            Id = c.Key,
                            Created = c.Value.FirstOrDefault()?.Timestamp ?? default(DateTimeOffset),
                        })
                        .OrderBy(c => c.Created)
                        .Take(20)
                        .ToArray();
                        if (pagedResult.Items.Count() == 20)
                        {
                            pagedResult.ContinuationToken = pagedResult.Items.Last().Id;
                        }
                    }
                }
            }

            return Task.FromResult(pagedResult);
        }
    }
}
