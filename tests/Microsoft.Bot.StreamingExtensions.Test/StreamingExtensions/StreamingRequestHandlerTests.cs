using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Bot.StreamingExtensions.UnitTests.Mocks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Microsoft.Bot.Builder;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.StreamingExtensions
{
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
        public void StreamingRequestHandler_CanSetServer()
        {
            var s = new StreamingRequestHandler(null, new MockBot());
            s.Server = new MockStreamingTransportServer();
            Assert.IsNotNull(s.Server);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_NoVerb_ReturnsBadRequest()
        {
            var s = new StreamingRequestHandler(null, new MockBot());
            s.Server = new MockStreamingTransportServer();

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Path = "/api/messages" });

            Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_NoPath_ReturnsBadRequest()
        {
            var s = new StreamingRequestHandler(null, new MockBot());
            s.Server = new MockStreamingTransportServer();

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Verb = "POST" });

            Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_BadVerb_ReturnsNotFound()
        {
            var s = new StreamingRequestHandler(null, new MockBot());
            s.Server = new MockStreamingTransportServer();

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Verb = "Delete", Path = "/api/messages" });

            Assert.AreEqual((int)HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_BadPath_ReturnsNotFound()
        {
            var s = new StreamingRequestHandler(null, new MockBot());
            s.Server = new MockStreamingTransportServer();

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Verb = "Post", Path = "/api/messagesV3" });

            Assert.AreEqual((int)HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_GetVersion()
        {
            var s = new StreamingRequestHandler(null, new MockBot());
            s.Server = new MockStreamingTransportServer();

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Verb = "GET", Path = "/api/version" });

            Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, response.Streams.Count);
            var version = await response.Streams[0].Content.ReadAsAsync<VersionInfo>();
            Assert.IsNotNull(version);
            Assert.IsNotNull(version.UserAgent);
        }



        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_PostMessages_NoActivity_BadRequest()
        {
            var s = new StreamingRequestHandler(null, new MockBot());
            s.Server = new MockStreamingTransportServer();

            var response = await s.ProcessRequestAsync(new ReceiveRequest() { Verb = "POST", Path = "/api/messages" });

            Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_PostMessages_WithActivity_OK()
        {
            var s = new StreamingRequestHandler(null, new MockBot());
            s.Server = new MockStreamingTransportServer();

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
                new MockContentStream(stream, "application/json")
            };

            var response = await s.ProcessRequestAsync(request);

            Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_PostMessages_WithBotError_InternalServerError()
        {
            var s = new StreamingRequestHandler(null, new MockBot() { ThrowDuringOnTurnAsync = true });
            s.Server = new MockStreamingTransportServer();

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
                new MockContentStream(stream, "application/json")
            };

            var response = await s.ProcessRequestAsync(request);

            Assert.AreEqual((int)HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public async Task StreamingRequestHandler_ProcessRequestAsync_PostMessages_WithOneBotError_Recovers()
        {
            var bot = new MockBot() { ThrowDuringOnTurnAsync = true };
            var s = new StreamingRequestHandler(null, bot);
            s.Server = new MockStreamingTransportServer();

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
                new MockContentStream(stream, "application/json")
            };

            var response = await s.ProcessRequestAsync(request);

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
                new MockContentStream(stream, "application/json")
            };

            response = await s.ProcessRequestAsync(request);
            Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
        }
    }
}
