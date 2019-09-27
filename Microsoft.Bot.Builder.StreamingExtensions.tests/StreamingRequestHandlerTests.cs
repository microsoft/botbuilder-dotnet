using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.StreamingExtensions.Tests
{
    public class StreamingRequestHandlerTests
    {
        [Fact]
        public async Task Test1()
        {
            // Arrange

            // Act
            var handler = new StreamingRequestHandler(null, new DirectLineAdapter(), "fakePipe");
            var activity = new Schema.Activity()
            {
                Type = "message",
                Text = "received from bot",
                From = new Schema.ChannelAccount()
                {
                    Id = "bot",
                    Name = "bot",
                },
                Conversation = new Schema.ConversationAccount(null, null, "testconvoid", null, null, null, null),
            };

            var payload = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity, SerializationSettings.DefaultDeserializationSettings)));
            var fakeContentStreamId = Guid.NewGuid();
            var fakeContentStream = new FakeContentStream(fakeContentStreamId, "application/json", payload);
            var testRequest = new ReceiveRequest();
            testRequest.Path = $"/v3/conversations/{activity.Conversation?.Id}/activities/{activity.Id}";
            testRequest.Verb = "POST";
            testRequest.Streams.Add(fakeContentStream);
            var response = await handler.ProcessRequestAsync(testRequest);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ItGetsUserAgentInfo()
        {
            // Arrange

            // Act
            var handler = new StreamingRequestHandler(null, new DirectLineAdapter(), "fakePipe");
            var activity = new Schema.Activity()
            {
                Type = "message",
                Text = "received from bot",
                From = new Schema.ChannelAccount()
                {
                    Id = "bot",
                    Name = "bot",
                },
                Conversation = new Schema.ConversationAccount(null, null, "testconvoid", null, null, null, null),
            };

            var payload = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity, SerializationSettings.DefaultDeserializationSettings)));
            var fakeContentStreamId = Guid.NewGuid();
            var fakeContentStream = new FakeContentStream(fakeContentStreamId, "application/json", payload);
            var testRequest = new ReceiveRequest();
            testRequest.Path = "/api/version";
            testRequest.Verb = "GET";
            testRequest.Streams.Add(fakeContentStream);
            var response = await handler.ProcessRequestAsync(testRequest);

            // Assert
            Assert.NotNull(response);
        }
    }
}
