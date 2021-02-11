// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Tests
{
    public class InspectionTests
    {
        [Fact]
        public async Task ScenarioWithInspectionMiddlwarePassthrough()
        {
            var inspectionState = new InspectionState(new MemoryStorage());
            var inspectionMiddleware = new InspectionMiddleware(inspectionState);

            var adapter = new TestAdapter();
            adapter.Use(inspectionMiddleware);

            var inboundActivity = MessageFactory.Text("hello");

            await adapter.ProcessActivityAsync(inboundActivity, async (turnContext, cancellationToken) =>
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("hi"));
            });

            var outboundActivity = adapter.ActiveQueue.Dequeue();

            Assert.Equal("hi", outboundActivity.Text);
        }

        [Fact]
        public async Task ScenarioWithInspectionMiddlwareOpenAttach()
        {
            // Arrange

            // any bot state should be returned as trace messages per turn
            var storage = new MemoryStorage();
            var inspectionState = new InspectionState(storage);
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            // set up the middleware with an http client that will just record the traffic - we are expecting the trace activities here
            var recordingHttpClient = new RecordingHttpMessageHandler();
            var inspectionMiddleware = new TestInspectionMiddleware(
                inspectionState,
                userState,
                conversationState,
                new HttpClient(recordingHttpClient));

            // Act

            // (1) send the /INSPECT open command from the emulator to the middleware
            var openActivity = MessageFactory.Text("/INSPECT open");

            var inspectionAdapter = new TestAdapter(Channels.Test, true);
            await inspectionAdapter.ProcessActivityAsync(openActivity, async (turnContext, cancellationToken) =>
            {
                await inspectionMiddleware.ProcessCommandAsync(turnContext);
            });

            var inspectionOpenResultActivity = inspectionAdapter.ActiveQueue.Dequeue();

            // (2) send the resulting /INSPECT attach command from the channel to the middleware
            var applicationAdapter = new TestAdapter(Channels.Test, true);
            applicationAdapter.Use(inspectionMiddleware);

            var attachCommand = inspectionOpenResultActivity.Value.ToString();

            await applicationAdapter.ProcessActivityAsync(MessageFactory.Text(attachCommand), async (turnContext, cancellationToken) =>
            {
                // nothing happens - just attach the inspector
                await Task.CompletedTask;
            });

            var attachResponse = applicationAdapter.ActiveQueue.Dequeue();

            // (3) send an application messaage from the channel, it should get the reply and then so should the emulator http endpioint
            await applicationAdapter.ProcessActivityAsync(MessageFactory.Text("hi"), async (turnContext, cancellationToken) =>
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"echo: {turnContext.Activity.Text}"));

                (await userState.CreateProperty<Scratch>("x").GetAsync(turnContext, () => new Scratch())).Property = "hello";
                (await conversationState.CreateProperty<Scratch>("y").GetAsync(turnContext, () => new Scratch())).Property = "world";
                await userState.SaveChangesAsync(turnContext);
                await conversationState.SaveChangesAsync(turnContext);
            });

            // Assert
            var outboundActivity = applicationAdapter.ActiveQueue.Dequeue();
            Assert.Equal("echo: hi", outboundActivity.Text);
            Assert.Equal(3, recordingHttpClient.Requests.Count);

            var inboundTrace = JObject.Parse(recordingHttpClient.Requests[0]);

            Assert.Equal("trace", inboundTrace["type"].ToString());
            Assert.Equal("ReceivedActivity", inboundTrace["name"].ToString());
            Assert.Equal("message", inboundTrace["value"]["type"].ToString());
            Assert.Equal("hi", inboundTrace["value"]["text"].ToString());

            var outboundTrace = JObject.Parse(recordingHttpClient.Requests[1]);

            Assert.Equal("trace", outboundTrace["type"].ToString());
            Assert.Equal("SentActivity", outboundTrace["name"].ToString());
            Assert.Equal("message", outboundTrace["value"]["type"].ToString());
            Assert.Equal("echo: hi", outboundTrace["value"]["text"].ToString());

            var stateTrace = JObject.Parse(recordingHttpClient.Requests[2]);

            Assert.Equal("trace", stateTrace["type"].ToString());
            Assert.Equal("BotState", stateTrace["name"].ToString());
            Assert.Equal("hello", stateTrace["value"]["userState"]["x"]["Property"].ToString());
            Assert.Equal("world", stateTrace["value"]["conversationState"]["y"]["Property"].ToString());
        }

        [Fact]
        public async Task ScenarioWithInspectionMiddlwareOpenAttachWithMention()
        {
            // Arrange

            // any bot state should be returned as trace messages per turn
            var storage = new MemoryStorage();
            var inspectionState = new InspectionState(storage);
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            // set up the middleware with an http client that will just record the traffic - we are expecting the trace activities here
            var recordingHttpClient = new RecordingHttpMessageHandler();
            var inspectionMiddleware = new TestInspectionMiddleware(
                inspectionState,
                userState,
                conversationState,
                new HttpClient(recordingHttpClient));

            // Act

            // (1) send the /INSPECT open command from the emulator to the middleware
            var openActivity = MessageFactory.Text("/INSPECT open");

            var inspectionAdapter = new TestAdapter(Channels.Test, true);
            await inspectionAdapter.ProcessActivityAsync(openActivity, async (turnContext, cancellationToken) =>
            {
                await inspectionMiddleware.ProcessCommandAsync(turnContext);
            });

            var inspectionOpenResultActivity = inspectionAdapter.ActiveQueue.Dequeue();

            // (2) send the resulting /INSPECT attach command from the channel to the middleware
            var applicationAdapter = new TestAdapter(Channels.Test, true);
            applicationAdapter.Use(inspectionMiddleware);

            // some channels - for example Microsoft Teams - adds an @ mention to the text - this should be taken into account when evaluating the INSPECT
            var recipientId = "bot";
            var attachCommand = $"<at>{recipientId}</at> {inspectionOpenResultActivity.Value}\n";
            var properties = new JObject
            {
                { "text", $"<at>{recipientId}</at>" },
                { "mentioned", new JObject { { "id", "bot" } } },
            };
            var attachActivity = MessageFactory.Text(attachCommand);
            attachActivity.Entities.Add(new Schema.Entity { Type = "mention", Properties = properties });

            await applicationAdapter.ProcessActivityAsync(attachActivity, async (turnContext, cancellationToken) =>
            {
                // nothing happens - just attach the inspector
                await Task.CompletedTask;
            });

            var attachResponse = applicationAdapter.ActiveQueue.Dequeue();

            // (3) send an application messaage from the channel, it should get the reply and then so should the emulator http endpioint
            await applicationAdapter.ProcessActivityAsync(MessageFactory.Text("hi"), async (turnContext, cancellationToken) =>
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"echo: {turnContext.Activity.Text}"));

                (await userState.CreateProperty<Scratch>("x").GetAsync(turnContext, () => new Scratch())).Property = "hello";
                (await conversationState.CreateProperty<Scratch>("y").GetAsync(turnContext, () => new Scratch())).Property = "world";
                await userState.SaveChangesAsync(turnContext);
                await conversationState.SaveChangesAsync(turnContext);
            });

            // Assert
            var outboundActivity = applicationAdapter.ActiveQueue.Dequeue();
            Assert.Equal("echo: hi", outboundActivity.Text);
            Assert.Equal(3, recordingHttpClient.Requests.Count);

            var inboundTrace = JObject.Parse(recordingHttpClient.Requests[0]);

            Assert.Equal("trace", inboundTrace["type"].ToString());
            Assert.Equal("ReceivedActivity", inboundTrace["name"].ToString());
            Assert.Equal("message", inboundTrace["value"]["type"].ToString());
            Assert.Equal("hi", inboundTrace["value"]["text"].ToString());

            var outboundTrace = JObject.Parse(recordingHttpClient.Requests[1]);

            Assert.Equal("trace", outboundTrace["type"].ToString());
            Assert.Equal("SentActivity", outboundTrace["name"].ToString());
            Assert.Equal("message", outboundTrace["value"]["type"].ToString());
            Assert.Equal("echo: hi", outboundTrace["value"]["text"].ToString());

            var stateTrace = JObject.Parse(recordingHttpClient.Requests[2]);

            Assert.Equal("trace", stateTrace["type"].ToString());
            Assert.Equal("BotState", stateTrace["name"].ToString());
            Assert.Equal("hello", stateTrace["value"]["userState"]["x"]["Property"].ToString());
            Assert.Equal("world", stateTrace["value"]["conversationState"]["y"]["Property"].ToString());
        }

        [Fact]
        public async Task ScenarioWithInspectionMiddlwareOpenAttachAndTracePassThrough()
        {
            // Arrange

            // any bot state should be returned as trace messages per turn
            var storage = new MemoryStorage();
            var inspectionState = new InspectionState(storage);

            // set up the middleware with an http client that will just record the traffic - we are expecting the trace activities here
            var recordingHttpClient = new RecordingHttpMessageHandler();
            var inspectionMiddleware = new TestInspectionMiddleware(
                inspectionState,
                null,
                null,
                new HttpClient(recordingHttpClient));

            // Act

            // (1) send the /INSPECT open command from the emulator to the middleware
            var openActivity = MessageFactory.Text("/INSPECT open");

            var inspectionAdapter = new TestAdapter(Channels.Test, true);
            await inspectionAdapter.ProcessActivityAsync(openActivity, async (turnContext, cancellationToken) =>
            {
                await inspectionMiddleware.ProcessCommandAsync(turnContext);
            });

            var inspectionOpenResultActivity = inspectionAdapter.ActiveQueue.Dequeue();

            // (2) send the resulting /INSPECT attach command from the channel to the middleware
            var applicationAdapter = new TestAdapter(Channels.Test, true);
            applicationAdapter.Use(inspectionMiddleware);

            var attachCommand = inspectionOpenResultActivity.Value.ToString();

            await applicationAdapter.ProcessActivityAsync(MessageFactory.Text(attachCommand), async (turnContext, cancellationToken) =>
            {
                // nothing happens - just attach the inspector
                await Task.CompletedTask;
            });

            var attachResponse = applicationAdapter.ActiveQueue.Dequeue();

            // (3) send an application messaage from the channel, it should get the reply and then so should the emulator http endpioint
            await applicationAdapter.ProcessActivityAsync(MessageFactory.Text("hi"), async (turnContext, cancellationToken) =>
            {
                var activity = (Activity)Activity.CreateTraceActivity("CustomTrace");
                await turnContext.SendActivityAsync(activity);
            });

            // Assert
            var outboundActivity = applicationAdapter.ActiveQueue.Dequeue();
            Assert.Equal("CustomTrace", outboundActivity.Name);
            Assert.Equal(2, recordingHttpClient.Requests.Count);

            var inboundTrace = JObject.Parse(recordingHttpClient.Requests[0]);

            Assert.Equal("trace", inboundTrace["type"].ToString());
            Assert.Equal("ReceivedActivity", inboundTrace["name"].ToString());
            Assert.Equal("message", inboundTrace["value"]["type"].ToString());
            Assert.Equal("hi", inboundTrace["value"]["text"].ToString());

            var outboundTrace = JObject.Parse(recordingHttpClient.Requests[1]);

            Assert.Equal("trace", outboundTrace["type"].ToString());
            Assert.Equal("CustomTrace", outboundTrace["name"].ToString());
        }

        private class Scratch
        {
            public string Property { get; set; }
        }

        private class RecordingHttpMessageHandler : HttpMessageHandler
        {
            public List<string> Requests { get; } = new List<string>();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var requestContent = request.Content != null ? await request.Content.ReadAsStringAsync() : "(null)";

                Requests.Add(requestContent);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(new JObject { { "id", "SendActivityId" } }.ToString());
                return response;
            }
        }

        private class TestInspectionMiddleware : InspectionMiddleware
        {
            private HttpClient _httpClient;

            public TestInspectionMiddleware(InspectionState inspectionState, UserState userState, ConversationState conversationState, HttpClient httpClient)
                : base(inspectionState, userState, conversationState)
            {
                _httpClient = httpClient;
            }

            protected override HttpClient GetHttpClient()
            {
                return _httpClient;
            }
        }
    }
}
