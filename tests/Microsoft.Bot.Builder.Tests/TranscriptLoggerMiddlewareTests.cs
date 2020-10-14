// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class TranscriptLoggerMiddlewareTests
    {
        [Fact]
        public async Task ShouldNotLogContinueConversation()
        {
            var transcriptStore = new MemoryTranscriptStore();
            var sut = new TranscriptLoggerMiddleware(transcriptStore);

            var conversationId = Guid.NewGuid().ToString();
            var adapter = new TestAdapter(TestAdapter.CreateConversation(conversationId))
                .Use(sut);

            await new TestFlow(adapter, async (context, cancellationToken) =>
                {
                    await context.SendActivityAsync("bar", cancellationToken: cancellationToken);
                })
                .Send("foo")
                .AssertReply(async activity =>
                {
                    Assert.Equal("bar", ((Activity)activity).Text);
                    var activities = await transcriptStore.GetTranscriptActivitiesAsync(activity.ChannelId, conversationId);
                    Assert.Equal(2, activities.Items.Length);
                })
                .Send(new Activity(ActivityTypes.Event) { Name = ActivityEventNames.ContinueConversation })
                .AssertReply(async activity =>
                {
                    // Ensure the event hasn't been added to the transcript.
                    var activities = await transcriptStore.GetTranscriptActivitiesAsync(activity.ChannelId, conversationId);
                    Assert.DoesNotContain(activities.Items, a => ((Activity)a).Type == ActivityTypes.Event && ((Activity)a).Name == ActivityEventNames.ContinueConversation);
                    Assert.Equal(3, activities.Items.Length);
                })
                .StartTestAsync();
        }
    }
}
