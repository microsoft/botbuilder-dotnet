// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Event")]
    public class EventFactoryTests
    {
        public TestContext TestContext { get; set; }

        [Fact]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HandoffInitiationNullTurnContext()
        {
            EventFactory.CreateHandoffInitiation(null, "some text");
        }

        [Fact]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HandoffStatusNullConversation()
        {
            EventFactory.CreateHandoffStatus(null, "accepted");
        }

        [Fact]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HandoffStatusNullStatus()
        {
            EventFactory.CreateHandoffStatus(new ConversationAccount(), null);
        }

        [Fact]
        public void TestCreateHandoffInitiation()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            string fromID = "test";
            var activity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = string.Empty,
                Conversation = new ConversationAccount(),
                Recipient = new ChannelAccount(),
                From = new ChannelAccount(fromID),
                ChannelId = "testchannel",
                ServiceUrl = "http://myservice"
            };
            var context = new TurnContext(adapter, activity);

            var transcript = new Transcript(new Activity[] { MessageFactory.Text("hello") });

            Assert.Null(transcript.Activities[0].ChannelId);
            Assert.Null(transcript.Activities[0].ServiceUrl);
            Assert.Null(transcript.Activities[0].Conversation);

            var handoffEvent = EventFactory.CreateHandoffInitiation(context, new { Skill = "any" }, transcript);
            Assert.Equal(handoffEvent.Name, HandoffEventNames.InitiateHandoff);
            var skill = (handoffEvent.Value as JObject)?.Value<string>("Skill");
            Assert.Equal(skill, "any");
            Assert.Equal(handoffEvent.From.Id, fromID);
        }

        [Fact]
        public void TestCreateHandoffStatus()
        {
            var state = "failed";
            var message = "timed out";
            var handoffEvent = EventFactory.CreateHandoffStatus(new ConversationAccount(), state, message);
            Assert.Equal(handoffEvent.Name, HandoffEventNames.HandoffStatus);

            var stateFormEvent = (handoffEvent.Value as JObject)?.Value<string>("state");
            Assert.Equal(stateFormEvent, state);

            var messageFormEvent = (handoffEvent.Value as JObject)?.Value<string>("message");
            Assert.Equal(messageFormEvent, message);

            string status = JsonConvert.SerializeObject(handoffEvent.Value, Formatting.None);
            Assert.Equal(status, $"{{\"state\":\"{state}\",\"message\":\"{message}\"}}");
            Assert.NotNull((handoffEvent as Activity).Attachments);
            Assert.NotNull(handoffEvent.Id);
        }

        [Fact]
        public void TestCreateHandoffStatusNoMessage()
        {
            var state = "failed";
            var handoffEvent = EventFactory.CreateHandoffStatus(new ConversationAccount(), state);

            var stateFormEvent = (handoffEvent.Value as JObject)?.Value<string>("state");
            Assert.Equal(state, stateFormEvent);

            var messageFormEvent = (handoffEvent.Value as JObject)?.Value<string>("message");
            Assert.Equal(null, messageFormEvent);
        }
    }
}
