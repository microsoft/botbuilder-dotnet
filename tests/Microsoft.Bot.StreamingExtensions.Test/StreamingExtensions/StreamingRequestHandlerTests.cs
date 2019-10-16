// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.StreamingExtensions;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.Bot.StreamingExtensions.UnitTests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.StreamingExtensions
{
#pragma warning disable IDE0017
    [TestClass]
    public class StreamingRequestHandlerTests
    {
        [TestMethod]
        public void StreamingRequestHandler_NullOnTurnError_Ok()
        {
            var s = new StreamingRequestHandler(null, new MockBot());
            Assert.IsNotNull(s);
        }

        [TestMethod]
        public void StreamingRequestHandler_NullMiddleware_Ok()
        {
            var s = new StreamingRequestHandler(null, new MockBot(), null);
            Assert.IsNotNull(s);
        }

        [TestMethod]
        public void StreamingRequestHandler_UserAgentSet()
        {
            var s = new StreamingRequestHandler(null, new MockBot(), null);
            Assert.IsNotNull(s.UserAgent);
        }

        [TestMethod]
        public void StreamingRequestHandler_UserAgent_Matches_Standard_Format()
        {
            var s = new StreamingRequestHandler(null, new MockBot(), null);

            var client = new HttpClient();
            var userAgentHeader = client.DefaultRequestHeaders.UserAgent;

            // The Schema version is 3.1, put into the Microsoft-BotFramework header
            var botFwkProductInfo = new ProductInfoHeaderValue("Microsoft-BotFramework", "3.1");
            if (!userAgentHeader.Contains(botFwkProductInfo))
            {
                userAgentHeader.Add(botFwkProductInfo);
            }

            // Info on Streaming Extensions Version
            var streamingExtensionsVersion = new ProductInfoHeaderValue("Streaming-Extensions", "1.0");
            if (!userAgentHeader.Contains(streamingExtensionsVersion))
            {
                userAgentHeader.Add(streamingExtensionsVersion);
            }

            // The Client SDK Version
            //  https://github.com/Microsoft/botbuilder-dotnet/blob/d342cd66d159a023ac435aec0fdf791f93118f5f/doc/UserAgents.md
            var botBuilderProductInfo = new ProductInfoHeaderValue("BotBuilder", ConnectorClient.GetClientVersion(new ConnectorClient(new System.Uri("http://localhost"))));
            if (!userAgentHeader.Contains(botBuilderProductInfo))
            {
                userAgentHeader.Add(botBuilderProductInfo);
            }

            // Additional Info.
            // https://github.com/Microsoft/botbuilder-dotnet/blob/d342cd66d159a023ac435aec0fdf791f93118f5f/doc/UserAgents.md
            var userAgent = $"({ConnectorClient.GetASPNetVersion()}; {ConnectorClient.GetOsVersion()}; {ConnectorClient.GetArchitecture()})";
            if (ProductInfoHeaderValue.TryParse(userAgent, out var additionalProductInfo))
            {
                if (!userAgentHeader.Contains(additionalProductInfo))
                {
                    userAgentHeader.Add(additionalProductInfo);
                }
            }

            Assert.AreEqual(s.UserAgent, userAgentHeader.ToString());
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_NoVerb_ReturnsBadRequest()
        {
            var s = new StreamingRequestHandler(onTurnError: null, bot: new MockBot(), transportServer: new MockStreamingTransportServer());

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Path = "/api/messages" }, null);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_NoPath_ReturnsBadRequest()
        {
            var s = new StreamingRequestHandler(onTurnError: null, bot: new MockBot(), transportServer: new MockStreamingTransportServer());

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Verb = "POST" }, null);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_BadVerb_ReturnsNotFound()
        {
            var s = new StreamingRequestHandler(onTurnError: null, bot: new MockBot(), transportServer: new MockStreamingTransportServer());

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Verb = "Delete", Path = "/api/messages" }, null);

            Assert.AreEqual((int)HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_BadPath_ReturnsNotFound()
        {
            var s = new StreamingRequestHandler(onTurnError: null, bot: new MockBot(), transportServer: new MockStreamingTransportServer());

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Verb = "Post", Path = "/api/messagesV3" }, null);

            Assert.AreEqual((int)HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_GetVersion()
        {
            var s = new StreamingRequestHandler(onTurnError: null, bot: new MockBot(), transportServer: new MockStreamingTransportServer());

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Verb = "GET", Path = "/api/version" }, null);

            Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, response.Streams.Count);
            var version = await response.Streams[0].Content.ReadAsAsync<VersionInfo>();
            Assert.IsNotNull(version);
            Assert.IsNotNull(version.UserAgent);
        }

        [TestMethod]
        [Ignore]
        public async Task StreamingRequestHandler_ProcessRequestAsync_PostMessages_NoActivity_BadRequest()
        {
            var s = new StreamingRequestHandler(onTurnError: null, bot: new MockBot(), transportServer: new MockStreamingTransportServer());

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Verb = "POST", Path = "/api/messages" }, null);

            Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_PostMessages_WithActivity_OK()
        {
            var s = new StreamingRequestHandler(onTurnError: null, bot: new MockBot(), transportServer: new MockStreamingTransportServer());

            var request = new ReceiveRequest() { Verb = "POST", Path = "/api/messages" };

            var stream = new MemoryStream();
            var x = new StreamWriter(stream);
            var writer = new JsonTextWriter(x);

            var serializer = new JsonSerializer();
            serializer.Serialize(writer, new Activity() { Type = "message", Text = "hi" });
            x.Flush();
            stream.Position = 0;

            request.Streams = new List<IContentStream>()
            {
                new MockContentStream(stream, "application/json"),
            };

            var response = await s.ProcessRequestAsync(request, null);

            Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_PostMessages_WithBotError_InternalServerError()
        {
            var bot = new MockBot() { ThrowDuringOnTurnAsync = true };
            var s = new StreamingRequestHandler(onTurnError: null, bot: bot, transportServer: new MockStreamingTransportServer());

            var request = new ReceiveRequest() { Verb = "POST", Path = "/api/messages" };

            var stream = new MemoryStream();
            var x = new StreamWriter(stream);
            var writer = new JsonTextWriter(x);

            var serializer = new JsonSerializer();
            serializer.Serialize(writer, new Activity() { Type = "message", Text = "hi" });
            x.Flush();
            stream.Position = 0;

            request.Streams = new List<IContentStream>()
            {
                new MockContentStream(stream, "application/json"),
            };

            var response = await s.ProcessRequestAsync(request, null);

            Assert.AreEqual((int)HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_PostMessages_WithOneBotError_Recovers()
        {
            var bot = new MockBot() { ThrowDuringOnTurnAsync = true };
            var s = new StreamingRequestHandler(onTurnError: null, bot: bot, transportServer: new MockStreamingTransportServer());

            var request = new ReceiveRequest() { Verb = "POST", Path = "/api/messages" };

            var stream = new MemoryStream();
            var x = new StreamWriter(stream);
            var writer = new JsonTextWriter(x);

            var serializer = new JsonSerializer();
            serializer.Serialize(writer, new Activity() { Type = "message", Text = "hi" });
            x.Flush();
            stream.Position = 0;

            request.Streams = new List<IContentStream>()
            {
                new MockContentStream(stream, "application/json"),
            };

            var response = await s.ProcessRequestAsync(request, null);

            Assert.AreEqual((int)HttpStatusCode.InternalServerError, response.StatusCode);

            stream = new MemoryStream();
            x = new StreamWriter(stream);
            writer = new JsonTextWriter(x);
            serializer.Serialize(writer, new Activity() { Type = "message", Text = "hi" });
            x.Flush();
            stream.Position = 0;

            bot.ThrowDuringOnTurnAsync = false;
            stream.Position = 0;
            request.Streams = new List<IContentStream>()
            {
                new MockContentStream(stream, "application/json"),
            };

            response = await s.ProcessRequestAsync(request, null);
            Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
        }
    }
#pragma warning restore IDE0017
}
