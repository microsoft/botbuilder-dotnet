// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// The memory transcript store stores transcripts in volatile memory in a Dictionary.
    /// </summary>
    /// <remarks>
    /// Because this uses an unbounded volatile dictionary this should only be used for unit tests or non-production environments.
    /// </remarks>
    public class MemoryTranscriptStore : ITranscriptStore
    {
        private Dictionary<string, Dictionary<string, List<IActivity>>> _channels = new Dictionary<string, Dictionary<string, List<IActivity>>>();

        /// <summary>
        /// Logs an activity to the transcript.
        /// </summary>
        /// <param name="activity">The activity to log.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public Task LogActivityAsync(IActivity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity), "activity cannot be null for LogActivity()");
            }

            lock (_channels)
            {
                if (!_channels.TryGetValue(activity.ChannelId, out var channel))
                {
                    channel = new Dictionary<string, List<IActivity>>();
                    _channels[activity.ChannelId] = channel;
                }

                if (!channel.TryGetValue(activity.Conversation.Id, out var transcript))
                {
                    transcript = new List<IActivity>();
                    channel[activity.Conversation.Id] = transcript;
                }

                switch (activity.Type)
                {
                    case ActivityTypes.MessageDelete:
                        // if message delete comes in, delete the message from the transcript
                        for (int i = 0; i < transcript.Count; i++)
                        {
                            var originalActivity = transcript[i] as IMessageActivity;
                            if (originalActivity.Id == activity.Id)
                            {
                                // tombstone the original message
                                transcript[i] = new Activity()
                                {
                                    Type = ActivityTypes.MessageDelete,
                                    Id = originalActivity.Id,
                                    From = new ChannelAccount(id: "deleted", role: originalActivity.From.Role),
                                    Recipient = new ChannelAccount(id: "deleted", role: originalActivity.Recipient.Role),
                                    Locale = originalActivity.Locale,
                                    LocalTimestamp = originalActivity.Timestamp,
                                    Timestamp = originalActivity.Timestamp,
                                    ChannelId = originalActivity.ChannelId,
                                    Conversation = originalActivity.Conversation,
                                    ServiceUrl = originalActivity.ServiceUrl,
                                    ReplyToId = originalActivity.ReplyToId,
                                };
                                break;
                            }
                        }

                        break;

                    case ActivityTypes.MessageUpdate:
                        for (int i = 0; i < transcript.Count; i++)
                        {
                            var originalActivity = transcript[i];
                            if (originalActivity.Id == activity.Id)
                            {
                                var updatedActivity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                                updatedActivity.Type = originalActivity.Type; // fixup original type (should be Message)
                                updatedActivity.LocalTimestamp = originalActivity.LocalTimestamp;
                                updatedActivity.Timestamp = originalActivity.Timestamp;
                                transcript[i] = updatedActivity;
                                break;
                            }
                        }

                        break;

                    default:
                        transcript.Add(activity);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets from the store activities that match a set of criteria.
        /// </summary>
        /// <param name="channelId">The ID of the channel the conversation is in.</param>
        /// <param name="conversationId">The ID of the conversation.</param>
        /// <param name="continuationToken">The continuation token from the previous page of results.</param>
        /// <param name="startDate">A cutoff date. Activities older than this date are not included.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains a page of matching activities.</remarks>
        public Task<PagedResult<IActivity>> GetTranscriptActivitiesAsync(string channelId, string conversationId, string continuationToken = null, DateTimeOffset startDate = default(DateTimeOffset))
        {
            if (channelId == null)
            {
                throw new ArgumentNullException(nameof(channelId));
            }

            if (conversationId == null)
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

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

                            if (pagedResult.Items.Length == 20)
                            {
                                pagedResult.ContinuationToken = pagedResult.Items.Last().Id;
                            }
                        }
                        else
                        {
                            pagedResult.Items = transcript
                                .OrderBy(a => a.Timestamp)
                                .Where(a => a.Timestamp >= startDate)
                                .Take(20)
                                .ToArray();

                            if (pagedResult.Items.Length == 20)
                            {
                                pagedResult.ContinuationToken = pagedResult.Items.Last().Id;
                            }
                        }
                    }
                }
            }

            return Task.FromResult(pagedResult);
        }

        /// <summary>
        /// Deletes conversation data from the store.
        /// </summary>
        /// <param name="channelId">The ID of the channel the conversation is in.</param>
        /// <param name="conversationId">The ID of the conversation to delete.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public Task DeleteTranscriptAsync(string channelId, string conversationId)
        {
            if (channelId == null)
            {
                throw new ArgumentNullException(nameof(channelId));
            }

            if (conversationId == null)
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            lock (_channels)
            {
                if (_channels.TryGetValue(channelId, out var channel))
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
        /// Gets a page of conversations for a channel from the store.
        /// </summary>
        /// <param name="channelId">The ID of the channel.</param>
        /// <param name="continuationToken">The continuation token from the previous page of results.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task is successful, the result contains a page of conversations.</remarks>
        public Task<PagedResult<TranscriptInfo>> ListTranscriptsAsync(string channelId, string continuationToken = null)
        {
            if (channelId == null)
            {
                throw new ArgumentNullException(nameof(channelId));
            }

            var pagedResult = new PagedResult<TranscriptInfo>();
            lock (_channels)
            {
                if (_channels.TryGetValue(channelId, out var channel))
                {
                    if (continuationToken != null)
                    {
                        pagedResult.Items = channel.Select(c => new TranscriptInfo()
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

                        if (pagedResult.Items.Length == 20)
                        {
                            pagedResult.ContinuationToken = pagedResult.Items.Last().Id;
                        }
                    }
                    else
                    {
                        pagedResult.Items = channel.Select(
                            c => new TranscriptInfo
                            {
                                ChannelId = channelId,
                                Id = c.Key,
                                Created = c.Value.FirstOrDefault()?.Timestamp ?? default(DateTimeOffset),
                            })
                            .OrderBy(c => c.Created)
                            .Take(20)
                            .ToArray();

                        if (pagedResult.Items.Length == 20)
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
