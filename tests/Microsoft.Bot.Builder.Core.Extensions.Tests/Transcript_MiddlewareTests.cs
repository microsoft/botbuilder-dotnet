using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Core.Extensions.Tests
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

            await new TestFlow(adapter, async (context) =>
                {
                    conversationId = context.Activity.Conversation.Id;
                    var typingActivity = new Activity
                    {
                        Type = ActivityTypes.Typing,
                        RelatesTo = context.Activity.RelatesTo
                    };
                    await context.SendActivity(typingActivity);
                    await Task.Delay(500);
                    await context.SendActivity("echo:" + context.Activity.Text);
                })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTest();

            var pagedResult = await transcriptStore.GetTranscriptActivities("test", conversationId);
            Assert.AreEqual(6, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.IsNotNull(pagedResult.Items[1].AsTypingActivity());
            Assert.AreEqual("echo:foo", pagedResult.Items[2].AsMessageActivity().Text);
            Assert.AreEqual("bar", pagedResult.Items[3].AsMessageActivity().Text);
            Assert.IsNotNull(pagedResult.Items[4].AsTypingActivity());
            Assert.AreEqual("echo:bar", pagedResult.Items[5].AsMessageActivity().Text);
            foreach (var activity in pagedResult.Items)
            {
                Assert.IsTrue(!String.IsNullOrWhiteSpace(activity.Id));
                Assert.IsTrue(activity.Timestamp > default(DateTime));
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
            await new TestFlow(adapter, async (context) =>
            {
                conversationId = context.Activity.Conversation.Id;
                if (context.Activity.Text == "update")
                {
                    activityToUpdate.Text = "new response";
                    await context.UpdateActivity(activityToUpdate);
                }
                else
                {
                    var activity = context.Activity.CreateReply("response");
                    var response = await context.SendActivity(activity);
                    activity.Id = response.Id;

                    // clone the activity, so we can use it to do an update
                    activityToUpdate = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(activity));
                }
            })
                .Send("foo")
                .Send("update")
                    .AssertReply("new response")
                .StartTest();
            await Task.Delay(500);
            var pagedResult = await transcriptStore.GetTranscriptActivities("test", conversationId);
            Assert.AreEqual(4, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.AreEqual("response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.AreEqual("new response", pagedResult.Items[2].AsMessageUpdateActivity().Text);
            Assert.AreEqual("update", pagedResult.Items[3].AsMessageActivity().Text);
            Assert.AreEqual(pagedResult.Items[1].Id, pagedResult.Items[2].Id);
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
            await new TestFlow(adapter, async (context) =>
            {
                conversationId = context.Activity.Conversation.Id;
                if (context.Activity.Text == "deleteIt")
                {
                    await context.DeleteActivity(activityId);
                }
                else
                {
                    var activity = context.Activity.CreateReply("response");
                    var response = await context.SendActivity(activity);
                    activityId = response.Id;
                }
            })
                .Send("foo")
                    .AssertReply("response")
                .Send("deleteIt")
                .StartTest();
            await Task.Delay(500);
            var pagedResult = await transcriptStore.GetTranscriptActivities("test", conversationId);
            Assert.AreEqual(4, pagedResult.Items.Length);
            Assert.AreEqual("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.AreEqual("response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.AreEqual("deleteIt", pagedResult.Items[2].AsMessageActivity().Text);
            Assert.AreEqual(ActivityTypes.MessageDelete, pagedResult.Items[3].Type);
            Assert.AreEqual(pagedResult.Items[1].Id, pagedResult.Items[3].Id);
        }
    }
}