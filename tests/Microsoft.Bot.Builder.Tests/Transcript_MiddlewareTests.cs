// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class Transcript_MiddlewareTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task Transcript_LogActivities()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            string conversationId = null;

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    conversationId = context.Activity.Conversation.Id;
                    var typingActivity = new Activity
                    {
                        Type = ActivityTypes.Typing,
                        RelatesTo = context.Activity.RelatesTo
                    };
                    await context.SendActivityAsync(typingActivity);
                    await Task.Delay(500);
                    await context.SendActivityAsync("echo:" + context.Activity.Text);
                })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.AreEqual(6, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.IsNotNull(pagedResult.Items[1].AsTypingActivity());
            Assert.AreEqual("echo:foo", pagedResult.Items[2].AsMessageActivity().Text);
            Assert.AreEqual("bar", pagedResult.Items[3].AsMessageActivity().Text);
            Assert.IsNotNull(pagedResult.Items[4].AsTypingActivity());
            Assert.AreEqual("echo:bar", pagedResult.Items[5].AsMessageActivity().Text);
            foreach (var activity in pagedResult.Items)
            {
                Assert.IsTrue(!string.IsNullOrWhiteSpace(activity.Id));
                Assert.IsTrue(activity.Timestamp > default(DateTimeOffset));
            }
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task Transcript_LogUpdateActivities()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            string conversationId = null;
            Activity activityToUpdate = null;
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                conversationId = context.Activity.Conversation.Id;
                if (context.Activity.Text == "update")
                {
                    activityToUpdate.Text = "new response";
                    await context.UpdateActivityAsync(activityToUpdate);
                }
                else
                {
                    var activity = context.Activity.CreateReply("response");
                    var response = await context.SendActivityAsync(activity);
                    activity.Id = response.Id;

                    // clone the activity, so we can use it to do an update
                    activityToUpdate = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                }
            })
                .Send("foo")
                .Send("update")
                    .AssertReply("new response")
                .StartTestAsync();
            await Task.Delay(500);
            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.AreEqual(4, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.AreEqual("response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.AreEqual("new response", pagedResult.Items[2].AsMessageUpdateActivity().Text);
            Assert.AreEqual("update", pagedResult.Items[3].AsMessageActivity().Text);
            Assert.AreEqual(pagedResult.Items[1].Id, pagedResult.Items[2].Id);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task Transcript_TestDateLogUpdateActivities()
        {
            var dateTimeStartOffset1 = new DateTimeOffset(DateTime.Now);
            var dateTimeStartOffset2 = new DateTimeOffset(DateTime.UtcNow);
            

            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            string conversationId = null;
            Activity activityToUpdate = null;
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                conversationId = context.Activity.Conversation.Id;
                if (context.Activity.Text == "update")
                {
                    activityToUpdate.Text = "new response";
                    await context.UpdateActivityAsync(activityToUpdate);
                }
                else
                {
                    var activity = context.Activity.CreateReply("response");

                    var response = await context.SendActivityAsync(activity);
                    activity.Id = response.Id;

                    // clone the activity, so we can use it to do an update
                    activityToUpdate = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                }
            })
                .Send("foo")
                .Send("update")
                    .AssertReply("new response")
                .StartTestAsync();
            await Task.Delay(500);
            
            // Perform some queries
            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId, null, dateTimeStartOffset1.DateTime);
            Assert.AreEqual(4, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.AreEqual("response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.AreEqual("new response", pagedResult.Items[2].AsMessageUpdateActivity().Text);
            Assert.AreEqual("update", pagedResult.Items[3].AsMessageActivity().Text);
            Assert.AreEqual(pagedResult.Items[1].Id, pagedResult.Items[2].Id);
            // Perform some queries
            pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId, null, DateTimeOffset.MinValue);
            Assert.AreEqual(4, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.AreEqual("response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.AreEqual("new response", pagedResult.Items[2].AsMessageUpdateActivity().Text);
            Assert.AreEqual("update", pagedResult.Items[3].AsMessageActivity().Text);
            Assert.AreEqual(pagedResult.Items[1].Id, pagedResult.Items[2].Id);
            // Perform some queries
            pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId, null, DateTimeOffset.MaxValue);
            Assert.AreEqual(0, pagedResult.Items.Length);

        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task Transcript_LogDeleteActivities()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            string conversationId = null;
            string activityId = null;
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                conversationId = context.Activity.Conversation.Id;
                if (context.Activity.Text == "deleteIt")
                {
                    await context.DeleteActivityAsync(activityId);
                }
                else
                {
                    var activity = context.Activity.CreateReply("response");
                    var response = await context.SendActivityAsync(activity);
                    activityId = response.Id;
                }
            })
                .Send("foo")
                    .AssertReply("response")
                .Send("deleteIt")
                .StartTestAsync();
            await Task.Delay(500);
            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.AreEqual(4, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.AreEqual("response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.AreEqual("deleteIt", pagedResult.Items[2].AsMessageActivity().Text);
            Assert.AreEqual(ActivityTypes.MessageDelete, pagedResult.Items[3].Type);
            Assert.AreEqual(pagedResult.Items[1].Id, pagedResult.Items[3].Id);
        }
    }
}
