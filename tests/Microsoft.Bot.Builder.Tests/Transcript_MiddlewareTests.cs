// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class Transcript_MiddlewareTests
    {
        public static async Task LogActivitiesTest(ITranscriptStore transcriptStore)
        {
            var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
            TestAdapter adapter = new TestAdapter(conversation)
                .Use(new TranscriptLoggerMiddleware(transcriptStore));

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                var typingActivity = new Activity
                {
                    Type = ActivityTypes.Typing,
                    RelatesTo = context.Activity.RelatesTo
                };
                await context.SendActivityAsync(typingActivity);
                await Task.Delay(50);
                await context.SendActivityAsync("echo:" + context.Activity.Text);
            })
                .Send("foo")
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.Equal(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            await Task.Delay(100);

            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id);
            Assert.Equal(6, pagedResult.Items.Length);
            Assert.Equal("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.NotNull(pagedResult.Items[1].AsTypingActivity());
            Assert.Equal("echo:foo", pagedResult.Items[2].AsMessageActivity().Text);
            Assert.Equal("bar", pagedResult.Items[3].AsMessageActivity().Text);
            Assert.NotNull(pagedResult.Items[4].AsTypingActivity());
            Assert.Equal("echo:bar", pagedResult.Items[5].AsMessageActivity().Text);
            foreach (var activity in pagedResult.Items)
            {
                Assert.True(!string.IsNullOrWhiteSpace(activity.Id));
                Assert.True(activity.Timestamp > default(DateTimeOffset));
            }
        }

        public static async Task EnsureToLogActivitiesWithIdsTest(ITranscriptStore transcriptStore)
        {
            var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
            var adapter = new AllowNullIdTestAdapter(conversation)
                .Use(new TranscriptLoggerMiddleware(transcriptStore));

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                var activityWithId = new Activity
                {
                    Id = "TestActivityWithId",
                    Text = "I am an activity with an Id.",
                    Type = ActivityTypes.Message,
                    RelatesTo = context.Activity.RelatesTo
                };
                var activityWithNullId = new Activity
                {
                    Id = null,
                    Text = "My Id is null.",
                    Type = ActivityTypes.Message,
                    RelatesTo = context.Activity.RelatesTo
                };
                
                await context.SendActivityAsync(activityWithId);
                await context.SendActivityAsync(activityWithId);

                await context.SendActivityAsync(activityWithNullId);
            })
                 .Send("inbound message to TestFlow")
                    .AssertReply("I am an activity with an Id.")
                 .Send("2nd inbound message to TestFlow")
                   .AssertReply((activity) => Assert.Equal(activity.Id, "TestActivityWithId"))
                 .Send("3rd inbound message to TestFlow")
                   .AssertReply("My Id is null.")
                 .StartTestAsync();

            await Task.Delay(100);

            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id);
            Assert.Equal(12, pagedResult.Items.Length);
            Assert.Equal("inbound message to TestFlow", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.NotNull(pagedResult.Items[1].AsMessageActivity());
            Assert.Equal("I am an activity with an Id.", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.Equal("2nd inbound message to TestFlow", pagedResult.Items[4].AsMessageActivity().Text);
            Assert.Equal("TestActivityWithId", pagedResult.Items[5].Id);
            Assert.Equal("3rd inbound message to TestFlow", pagedResult.Items[8].AsMessageActivity().Text);
            Assert.Equal("My Id is null.", pagedResult.Items[11].AsMessageActivity().Text);
            Assert.Contains("g_", pagedResult.Items[11].AsMessageActivity().Id);
            foreach (var activity in pagedResult.Items)
            {
                Assert.True(activity.Timestamp > default(DateTimeOffset));
            }
        }

        public static async Task LogUpdateActivitiesTest(ITranscriptStore transcriptStore)
        {
            var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
            TestAdapter adapter = new TestAdapter(conversation)
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            Activity activityToUpdate = null;
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
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

            await Task.Delay(100);

            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id);
            Assert.Equal(3, pagedResult.Items.Length);
            Assert.Equal("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.Equal("new response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.Equal("update", pagedResult.Items[2].AsMessageActivity().Text);
        }

        public static async Task TestDateLogUpdateActivitiesTest(ITranscriptStore transcriptStore)
        {
            var dateTimeStartOffset1 = new DateTimeOffset(DateTime.Now);
            var dateTimeStartOffset2 = new DateTimeOffset(DateTime.UtcNow);

            var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
            TestAdapter adapter = new TestAdapter(conversation)
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            Activity activityToUpdate = null;
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
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

            await Task.Delay(100);

            // Perform some queries
            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id, null, dateTimeStartOffset1.DateTime);
            Assert.Equal(3, pagedResult.Items.Length);
            Assert.Equal("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.Equal("new response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.Equal("update", pagedResult.Items[2].AsMessageActivity().Text);

            // Perform some queries
            pagedResult = await transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id, null, DateTimeOffset.MinValue);
            Assert.Equal(3, pagedResult.Items.Length);
            Assert.Equal("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.Equal("new response", pagedResult.Items[1].AsMessageActivity().Text);
            Assert.Equal("update", pagedResult.Items[2].AsMessageActivity().Text);

            // Perform some queries
            pagedResult = await transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id, null, DateTimeOffset.MaxValue);
            Assert.Equal(0, pagedResult.Items.Length);
        }

        [Fact]
        public async Task MemoryTranscript_EnsureToLogActivitiesWithIdsTest()
        {
            var transcriptStore = new MemoryTranscriptStore();
            await EnsureToLogActivitiesWithIdsTest(transcriptStore);
        }

        [Fact]
        public async Task MemoryTranscript_LogActivities()
        {
            var transcriptStore = new MemoryTranscriptStore();
            await LogActivitiesTest(transcriptStore);
        }

        [Fact]
        public async Task MemoryTranscript_LogDeleteActivities()
        {
            var transcriptStore = new MemoryTranscriptStore();
            await LogDeleteActivitesTest(transcriptStore);
        }

        [Fact]
        public async Task MemoryTranscript_LogUpdateActivities()
        {
            var transcriptStore = new MemoryTranscriptStore();
            await LogUpdateActivitiesTest(transcriptStore);
        }

        [Fact]
        public async Task MemoryTranscript_TestDateLogUpdateActivities()
        {
            var transcriptStore = new MemoryTranscriptStore();
            await TestDateLogUpdateActivitiesTest(transcriptStore);
        }

        [Fact]
        public async Task FileTranscript_LogActivities()
        {
            var transcriptStore = GetFileTranscriptLogger();
            await LogActivitiesTest(transcriptStore);
        }

        [Fact]
        public async Task FileTranscript_LogDeleteActivities()
        {
            var transcriptStore = GetFileTranscriptLogger();
            await LogDeleteActivitesTest(transcriptStore);
        }

        [Fact]
        public async Task FileTranscript_LogUpdateActivities()
        {
            var transcriptStore = GetFileTranscriptLogger();
            await LogUpdateActivitiesTest(transcriptStore);
        }

        [Fact]
        public async Task FileTranscript_TestDateLogUpdateActivities()
        {
            var transcriptStore = GetFileTranscriptLogger();
            await TestDateLogUpdateActivitiesTest(transcriptStore);
        }

        private static async Task LogDeleteActivitesTest(ITranscriptStore transcriptStore)
        {
            var conversation = TestAdapter.CreateConversation(Guid.NewGuid().ToString("n"));
            TestAdapter adapter = new TestAdapter(conversation)
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            string activityId = null;
            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
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

            await Task.Delay(100);

            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync(conversation.ChannelId, conversation.Conversation.Id);
            Assert.Equal(3, pagedResult.Items.Length);
            Assert.Equal("foo", pagedResult.Items[0].AsMessageActivity().Text);
            Assert.NotNull(pagedResult.Items[1].AsMessageDeleteActivity());
            Assert.Equal(ActivityTypes.MessageDelete, pagedResult.Items[1].Type);
            Assert.Equal("deleteIt", pagedResult.Items[2].AsMessageActivity().Text);
        }

        private FileTranscriptLogger GetFileTranscriptLogger()
        {
            var path = Path.Combine(Path.GetTempPath(), "GetFileTranscriptLogger");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                foreach (var file in Directory.GetFiles(path, "*.transcript", new EnumerationOptions() { RecurseSubdirectories = true }))
                {
                    File.Delete(file);
                }
            }

            var transcriptStore = new FileTranscriptLogger(path);
            return transcriptStore;
        }
    }
}
