// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class TranscriptBaseTests
    {
        public TranscriptBaseTests()
        {
        }

        public ITranscriptStore Store { get; set; }

        public async Task BadArgs()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.LogActivityAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.GetTranscriptActivitiesAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.GetTranscriptActivitiesAsync("asdfds", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.ListTranscriptsAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.DeleteTranscriptAsync(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => Store.DeleteTranscriptAsync("test", null));
        }

        public async Task LogActivity()
        {
            string conversationId = "_LogActivity";
            var activities = CreateActivities(conversationId, DateTime.UtcNow);
            var activity = activities.First();
            await Store.LogActivityAsync(activity);

            var results = await Store.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.Single(results.Items);

            Assert.Equal(JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(results.Items[0]));
        }

        public async Task LogMultipleActivities()
        {
            string conversationId = "LogMultipleActivities";
            DateTime start = DateTime.UtcNow;
            var activities = CreateActivities(conversationId, start);

            foreach (var activity in activities)
            {
                await Store.LogActivityAsync(activity);
            }

            // modify first record
            var updateActivity = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activities[0]));
            updateActivity.Text = "updated";
            updateActivity.Type = ActivityTypes.MessageUpdate;
            await Store.LogActivityAsync(updateActivity);
            activities[0].Text = "updated";

            // modify delete second record
            var deleteActivity = new Activity()
            {
                Type = ActivityTypes.MessageDelete,
                Timestamp = DateTime.Now,
                Id = activities[1].Id,
                ChannelId = activities[1].ChannelId,
                From = activities[1].From,
                Conversation = activities[1].Conversation,
                Recipient = activities[1].Recipient,
                ServiceUrl = activities[1].ServiceUrl,
            };
            await Store.LogActivityAsync(deleteActivity);

            // tombstone the deleted record
            activities[1] = new Activity()
            {
                Type = ActivityTypes.MessageDelete,
                Id = activities[1].Id,
                From = new ChannelAccount(id: "deleted", role: activities[1].From.Role),
                Recipient = new ChannelAccount(id: "deleted", role: activities[1].Recipient.Role),
                Locale = activities[1].Locale,
                LocalTimestamp = activities[1].Timestamp,
                Timestamp = activities[1].Timestamp,
                ChannelId = activities[1].ChannelId,
                Conversation = activities[1].Conversation,
                ServiceUrl = activities[1].ServiceUrl,
                ReplyToId = activities[1].ReplyToId,
            };

            // make sure other channels and conversations don't return results
            var pagedResult = await Store.GetTranscriptActivitiesAsync("bogus", conversationId);
            Assert.Null(pagedResult.ContinuationToken);
            Assert.Empty(pagedResult.Items);

            // make sure other channels and conversations don't return results
            pagedResult = await Store.GetTranscriptActivitiesAsync("test", "bogus");
            Assert.Null(pagedResult.ContinuationToken);
            Assert.Empty(pagedResult.Items);

            pagedResult = await Store.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.Null(pagedResult.ContinuationToken);
            Assert.Equal(activities.Count, pagedResult.Items.Length);

            int indexActivity = 0;
            foreach (var result in pagedResult.Items.OrderBy(result => result.Timestamp))
            {
                Assert.Equal(JsonConvert.SerializeObject(activities[indexActivity++]), JsonConvert.SerializeObject(result));
            }

            pagedResult = await Store.GetTranscriptActivitiesAsync("test", conversationId, startDate: start + TimeSpan.FromMinutes(5));
            Assert.Equal(activities.Count / 2, pagedResult.Items.Length);

            indexActivity = 5;
            foreach (var result in pagedResult.Items.OrderBy(result => result.Timestamp))
            {
                Assert.Equal(JsonConvert.SerializeObject(activities[indexActivity++]), JsonConvert.SerializeObject(result));
            }
        }

        public async Task DeleteTranscript()
        {
            string conversationId = "_DeleteConversation";
            DateTime start = DateTime.UtcNow;
            var activities = CreateActivities(conversationId, start);

            foreach (var activity in activities)
            {
                await Store.LogActivityAsync(activity);
            }

            string conversationId2 = "_DeleteConversation2";
            start = DateTime.UtcNow;
            var activities2 = CreateActivities(conversationId2, start);

            foreach (var activity in activities2)
            {
                await Store.LogActivityAsync(activity);
            }

            var pagedResult = await Store.GetTranscriptActivitiesAsync("test", conversationId);
            var pagedResult2 = await Store.GetTranscriptActivitiesAsync("test", conversationId2);

            Assert.Equal(activities.Count, pagedResult.Items.Length);
            Assert.Equal(activities.Count, pagedResult2.Items.Length);

            await Store.DeleteTranscriptAsync("test", conversationId);

            pagedResult = await Store.GetTranscriptActivitiesAsync("test", conversationId);
            pagedResult2 = await Store.GetTranscriptActivitiesAsync("test", conversationId2);

            Assert.Empty(pagedResult.Items);
            Assert.Equal(activities.Count, pagedResult2.Items.Length);
        }

        public async Task GetTranscriptActivities()
        {
            string conversationId = "_GetConversationActivitiesPaging";
            DateTime start = DateTime.UtcNow;
            var activities = CreateActivities(conversationId, start, 50);

            // log in parallel batches of 10
            int pos = 0;
            foreach (var group in activities.GroupBy(a => pos++ / 10))
            {
                await Task.WhenAll(group.Select(a => Store.LogActivityAsync(a)));
            }

            HashSet<string> seen = new HashSet<string>();
            PagedResult<IActivity> pagedResult = null;
            var pageSize = 0;
            do
            {
                pagedResult = await Store.GetTranscriptActivitiesAsync("test", conversationId, pagedResult?.ContinuationToken);
                Assert.NotNull(pagedResult);
                Assert.NotNull(pagedResult.Items);

                // NOTE: Assumes page size is consistent
                if (pageSize == 0)
                {
                    pageSize = pagedResult.Items.Count();
                }
                else if (pageSize == pagedResult.Items.Count())
                {
                    Assert.True(!string.IsNullOrEmpty(pagedResult.ContinuationToken));
                }

                foreach (var item in pagedResult.Items)
                {
                    Assert.DoesNotContain(item.Id, seen);
                    seen.Add(item.Id);
                }
            }
            while (pagedResult.ContinuationToken != null);

            Assert.Equal(activities.Count(), seen.Count);

            foreach (var activity in activities)
            {
                Assert.Contains(activity.Id, seen);
            }
        }

        public async Task GetTranscriptActivitiesStartDate()
        {
            string conversationId = "_GetConversationActivitiesStartDate";
            DateTime start = DateTime.UtcNow;
            var activities = CreateActivities(conversationId, start, 50);

            // log in parallel batches of 10
            int pos = 0;
            foreach (var group in activities.GroupBy(a => pos++ / 10))
            {
                await Task.WhenAll(group.Select(a => Store.LogActivityAsync(a)));
            }

            HashSet<string> seen = new HashSet<string>();
            DateTime startDate = start + TimeSpan.FromMinutes(50);
            PagedResult<IActivity> pagedResult = null;
            var pageSize = 0;
            do
            {
                pagedResult = await Store.GetTranscriptActivitiesAsync("test", conversationId, pagedResult?.ContinuationToken, startDate);
                Assert.NotNull(pagedResult);
                Assert.NotNull(pagedResult.Items);

                // NOTE: Assumes page size is consistent
                if (pageSize == 0)
                {
                    pageSize = pagedResult.Items.Count();
                }
                else if (pageSize == pagedResult.Items.Count())
                {
                    Assert.True(!string.IsNullOrEmpty(pagedResult.ContinuationToken));
                }

                foreach (var item in pagedResult.Items)
                {
                    Assert.DoesNotContain(item.Id, seen);
                    seen.Add(item.Id);
                }
            }
            while (pagedResult.ContinuationToken != null);

            Assert.Equal(activities.Count() / 2, seen.Count);

            foreach (var activity in activities.Where(a => a.Timestamp >= startDate))
            {
                Assert.Contains(activity.Id, seen);
            }

            foreach (var activity in activities.Where(a => a.Timestamp < startDate))
            {
                Assert.DoesNotContain(activity.Id, seen);
            }
        }

        public async Task ListTranscripts()
        {
            List<string> conversationIds = new List<string>();
            DateTime start = DateTime.UtcNow;
            for (int i = 0; i < 100; i++)
            {
                conversationIds.Add($"_ListConversations{i}");
            }

            List<Activity> activities = new List<Activity>();
            foreach (var conversationId in conversationIds)
            {
                activities.AddRange(CreateActivities(conversationId, start, 1));
            }

            // log in parallel batches of 10
            int pos = 0;
            foreach (var group in activities.GroupBy(a => pos++ / 10))
            {
                await Task.WhenAll(group.Select(a => Store.LogActivityAsync(a)));
            }

            HashSet<string> seen = new HashSet<string>();
            PagedResult<TranscriptInfo> pagedResult = null;
            var pageSize = 0;
            do
            {
                pagedResult = await Store.ListTranscriptsAsync("test", pagedResult?.ContinuationToken);
                Assert.NotNull(pagedResult);
                Assert.NotNull(pagedResult.Items);

                // NOTE: Assumes page size is consistent
                if (pageSize == 0)
                {
                    pageSize = pagedResult.Items.Count();
                }
                else if (pageSize == pagedResult.Items.Count())
                {
                    Assert.True(!string.IsNullOrEmpty(pagedResult.ContinuationToken));
                }

                foreach (var item in pagedResult.Items)
                {
                    Assert.DoesNotContain(item.Id, seen);
                    if (item.Id.StartsWith("_ListConversations"))
                    {
                        seen.Add(item.Id);
                    }
                }
            }
            while (pagedResult.ContinuationToken != null);

            Assert.Equal(conversationIds.Count(), seen.Count);

            foreach (var conversationId in conversationIds)
            {
                Assert.Contains(conversationId, seen);
            }
        }

        private List<Activity> CreateActivities(string conversationId, DateTime ts, int count = 5)
        {
            List<Activity> activities = new List<Activity>();
            for (int i = 1; i <= count; i++)
            {
                var activity = new Activity()
                {
                    Type = ActivityTypes.Message,
                    Timestamp = ts,
                    Id = Guid.NewGuid().ToString(),
                    Text = i.ToString(),
                    ChannelId = "test",
                    From = new ChannelAccount($"User" + i),
                    Conversation = new ConversationAccount(id: conversationId),
                    Recipient = new ChannelAccount("Bot1", "2"),
                    ServiceUrl = "http://foo.com/api/messages",
                };
                activities.Add(activity);
                ts += TimeSpan.FromMinutes(1);

                activity = new Activity()
                {
                    Type = ActivityTypes.Message,
                    Timestamp = ts,
                    Id = Guid.NewGuid().ToString(),
                    Text = i.ToString(),
                    ChannelId = "test",
                    From = new ChannelAccount("Bot1", "2"),
                    Conversation = new ConversationAccount(id: conversationId),
                    Recipient = new ChannelAccount($"User" + i),
                    ServiceUrl = "http://foo.com/api/messages",
                };
                activities.Add(activity);
                ts += TimeSpan.FromMinutes(1);
            }

            return activities;
        }
    }
}
