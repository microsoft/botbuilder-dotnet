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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Message")]
    public class EventFactoryTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HandoffInitiationNullTurnContext()
        {
            EventFactory.CreateHandoffInitiation(null, "some text");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HandoffStatusNullConversation()
        {
            EventFactory.CreateHandoffStatus(null, "accepted");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HandoffStatusNullStatus()
        {
            EventFactory.CreateHandoffStatus(new ConversationAccount(), null);
        }

        [TestMethod]
        public void TestCreateHandoffInitiation()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName));
            string fromID = "fred";
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

            Assert.IsNull(transcript.Activities[0].ChannelId);
            Assert.IsNull(transcript.Activities[0].ServiceUrl);
            Assert.IsNull(transcript.Activities[0].Conversation);

            var handoffEvent = EventFactory.CreateHandoffInitiation(context, new { Skill = "any" }, transcript);
            Assert.AreEqual(handoffEvent.Name, HandoffEventNames.InitiateHandoff);

            Assert.AreEqual(handoffEvent.From.Id, fromID);

            // Make sure ChannelId is fixed up in the transcript
            var transcriptFromEvent = (Transcript)(handoffEvent as Activity).Attachments[0].Content;
            Assert.IsNotNull(transcriptFromEvent.Activities[0].ChannelId);
            Assert.IsNotNull(transcriptFromEvent.Activities[0].ServiceUrl);
            Assert.IsNotNull(transcriptFromEvent.Activities[0].Conversation);
        }

        [TestMethod]
        public void TestCreateHandoffStatus()
        {
            var state = "failed";
            var message = "timed out";
            var handoffEvent = EventFactory.CreateHandoffStatus(new ConversationAccount(), state, message);
            Assert.AreEqual(handoffEvent.Name, HandoffEventNames.HandoffStatus);
            string status = JsonConvert.SerializeObject(handoffEvent.Value, Formatting.None);
            Assert.AreEqual(status, $"{{\"state\":\"{state}\",\"message\":\"{message}\"}}");
            Assert.IsNotNull((handoffEvent as Activity).Attachments);
            Assert.IsNotNull(handoffEvent.Id);
        }
    }
}
