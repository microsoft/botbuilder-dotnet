// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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
            try
            {
                await Store.LogActivityAsync(null);
                Assert.Fail("LogActivity Should have thrown on null ");
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail("LogActivity Should have thrown ArgumentNull exception on null ");
            }

            try
            {
                await Store.GetTranscriptActivitiesAsync(null, null);
                Assert.Fail("GetConversationActivities Should have thrown on null");
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail("DeleteConversation Should have thrown ArgumentNull ");
            }

            try
            {
                await Store.GetTranscriptActivitiesAsync("asdfds", null);
                Assert.Fail("GetConversationActivities Should have thrown on null");
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail("DeleteConversation Should have thrown ArgumentNull ");
            }

            try
            {
                await Store.ListTranscriptsAsync(null);
                Assert.Fail("ListConversations Should have thrown on null");
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail("ListConversations Should have thrown ArgumentNull ");
            }

            try
            {
                await Store.DeleteTranscriptAsync(null, null);
                Assert.Fail("DeleteConversation Should have thrown on null channelId");
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail("DeleteConversation Should have thrown ArgumentNull on channelId");
            }

            try
            {
                await Store.DeleteTranscriptAsync("test", null);
                Assert.Fail("DeleteConversation Should have thrown on null conversationId");
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail("DeleteConversation Should have thrown ArgumentNull on conversationId");
            }
        }

        public async Task LogActivity()
        {
            string conversationId = "_LogActivity";
            var activities = CreateActivities(conversationId, DateTime.UtcNow);
            var activity = activities.First();
            await Store.LogActivityAsync(activity);

            var results = await Store.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.AreEqual(1, results.Items.Length);

            Assert.AreEqual(JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(results.Items[0]));
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
            Assert.IsNull(pagedResult.ContinuationToken);
            Assert.AreEqual(0, pagedResult.Items.Length);

            // make sure other channels and conversations don't return results
            pagedResult = await Store.GetTranscriptActivitiesAsync("test", "bogus");
            Assert.IsNull(pagedResult.ContinuationToken);
            Assert.AreEqual(0, pagedResult.Items.Length);

            pagedResult = await Store.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.IsNull(pagedResult.ContinuationToken);
            Assert.AreEqual(activities.Count, pagedResult.Items.Length);

            int indexActivity = 0;
            foreach (var result in pagedResult.Items.OrderBy(result => result.Timestamp))
            {
                Assert.AreEqual(JsonConvert.SerializeObject(activities[indexActivity++]), JsonConvert.SerializeObject(result));
            }

            pagedResult = await Store.GetTranscriptActivitiesAsync("test", conversationId, startDate: start + TimeSpan.FromMinutes(5));
            Assert.AreEqual(activities.Count / 2, pagedResult.Items.Length);

            indexActivity = 5;
            foreach (var result in pagedResult.Items.OrderBy(result => result.Timestamp))
            {
                Assert.AreEqual(JsonConvert.SerializeObject(activities[indexActivity++]), JsonConvert.SerializeObject(result));
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

            Assert.AreEqual(activities.Count, pagedResult.Items.Length);
            Assert.AreEqual(activities.Count, pagedResult2.Items.Length);

            await Store.DeleteTranscriptAsync("test", conversationId);

            pagedResult = await Store.GetTranscriptActivitiesAsync("test", conversationId);
            pagedResult2 = await Store.GetTranscriptActivitiesAsync("test", conversationId2);

            Assert.AreEqual(0, pagedResult.Items.Length);
            Assert.AreEqual(activities.Count, pagedResult2.Items.Length);
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
                Assert.IsNotNull(pagedResult);
                Assert.IsNotNull(pagedResult.Items);

                // NOTE: Assumes page size is consistent
                if (pageSize == 0)
                {
                    pageSize = pagedResult.Items.Count();
                }
                else if (pageSize == pagedResult.Items.Count())
                {
                    Assert.IsTrue(!string.IsNullOrEmpty(pagedResult.ContinuationToken));
                }

                foreach (var item in pagedResult.Items)
                {
                    Assert.IsFalse(seen.Contains(item.Id));
                    seen.Add(item.Id);
                }
            }
            while (pagedResult.ContinuationToken != null);

            Assert.AreEqual(activities.Count(), seen.Count);

            foreach (var activity in activities)
            {
                Assert.IsTrue(seen.Contains(activity.Id));
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
                Assert.IsNotNull(pagedResult);
                Assert.IsNotNull(pagedResult.Items);

                // NOTE: Assumes page size is consistent
                if (pageSize == 0)
                {
                    pageSize = pagedResult.Items.Count();
                }
                else if (pageSize == pagedResult.Items.Count())
                {
                    Assert.IsTrue(!string.IsNullOrEmpty(pagedResult.ContinuationToken));
                }

                foreach (var item in pagedResult.Items)
                {
                    Assert.IsFalse(seen.Contains(item.Id));
                    seen.Add(item.Id);
                }
            }
            while (pagedResult.ContinuationToken != null);

            Assert.AreEqual(activities.Count() / 2, seen.Count);

            foreach (var activity in activities.Where(a => a.Timestamp >= startDate))
            {
                Assert.IsTrue(seen.Contains(activity.Id));
            }

            foreach (var activity in activities.Where(a => a.Timestamp < startDate))
            {
                Assert.IsFalse(seen.Contains(activity.Id));
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
                Assert.IsNotNull(pagedResult);
                Assert.IsNotNull(pagedResult.Items);

                // NOTE: Assumes page size is consistent
                if (pageSize == 0)
                {
                    pageSize = pagedResult.Items.Count();
                }
                else if (pageSize == pagedResult.Items.Count())
                {
                    Assert.IsTrue(!string.IsNullOrEmpty(pagedResult.ContinuationToken));
                }

                foreach (var item in pagedResult.Items)
                {
                    Assert.IsFalse(seen.Contains(item.Id));
                    if (item.Id.StartsWith("_ListConversations"))
                    {
                        seen.Add(item.Id);
                    }
                }
            }
            while (pagedResult.ContinuationToken != null);

            Assert.AreEqual(conversationIds.Count(), seen.Count);

            foreach (var conversationId in conversationIds)
            {
                Assert.IsTrue(seen.Contains(conversationId));
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
