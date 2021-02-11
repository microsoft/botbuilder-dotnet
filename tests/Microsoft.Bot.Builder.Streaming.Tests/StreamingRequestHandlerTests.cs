using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.UnitTests.Mocks;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Streaming.Tests
{
    public class StreamingRequestHandlerTests
    {
        [Fact]
        public void CanBeConstructedWithANamedPipe()
        {
            // Act
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), "fakePipe");

            // Assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void ThrowsIfSocketIsNull()
        {
            // Arrange
            Exception result = null;

            // Act
            try
            {
                var handler = new StreamingRequestHandler(bot: new MockBot(), activityProcessor: new BotFrameworkHttpAdapter(), socket: null);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsType<ArgumentNullException>(result);
        }

        [Fact]
        public void ThrowsIfPipeNameIsBlank()
        {
            // Arrange
            Exception result = null;

            // Act
            try
            {
                var handler = new StreamingRequestHandler(null, new BotFrameworkHttpAdapter(), string.Empty);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsType<ArgumentNullException>(result);
        }

        [Fact]
        public void CanBeConstructedWithAWebSocket()
        {
            // Act
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), new FauxSock());

            // Assert
            Assert.NotNull(handler);
        }

        [Fact]
        public async Task RequestHandlerRespondsWith500OnError()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), "fakePipe");
            var conversationId = "testconvoid";
            var membersAdded = new List<ChannelAccount>();
            var member = new ChannelAccount
            {
                Id = "123",
                Name = "bot",
            };
            membersAdded.Add(member);
            var activity = new Activity()
            {
                Type = "conversationUpdate",
                MembersAdded = membersAdded,
                Conversation = new ConversationAccount(null, null, conversationId, null, null, null, null),
            };

            var payload = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity, SerializationSettings.DefaultDeserializationSettings)));
            var fakeContentStreamId = Guid.NewGuid();
            var fakeContentStream = new FakeContentStream(fakeContentStreamId, "application/json", payload);
            var testRequest = new ReceiveRequest
            {
                Path = $"/v3/conversations/{activity.Conversation?.Id}/activities/{activity.Id}",
                Verb = "POST",
            };
            testRequest.Streams.Add(fakeContentStream);

            // Act
            var response = await handler.ProcessRequestAsync(testRequest);

            // Assert
            Assert.Equal(500, response.StatusCode);
        }

        [Fact]
        public async Task RequestHandlerRemembersConversations()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), "fakePipe");
            var conversationId = "testconvoid";
            var membersAdded = new List<ChannelAccount>();
            var member = new ChannelAccount
            {
                Id = "123",
                Name = "bot",
            };
            membersAdded.Add(member);
            var activity = new Activity()
            {
                Type = "conversationUpdate",
                MembersAdded = membersAdded,
                Conversation = new ConversationAccount(null, null, conversationId, null, null, null, null),
            };

            var payload = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity, SerializationSettings.DefaultDeserializationSettings)));
            var fakeContentStreamId = Guid.NewGuid();
            var fakeContentStream = new FakeContentStream(fakeContentStreamId, "application/json", payload);
            var testRequest = new ReceiveRequest
            {
                Path = $"/v3/conversations/{activity.Conversation?.Id}/activities/{activity.Id}",
                Verb = "POST",
            };
            testRequest.Streams.Add(fakeContentStream);

            // Act
            _ = await handler.ProcessRequestAsync(testRequest);

            // Assert
            Assert.True(handler.HasConversation(conversationId));
        }

        [Fact]
        public async Task RequestHandlerForgetsConversations()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), "fakePipe");
            var conversationId = "testconvoid";
            var membersAdded = new List<ChannelAccount>();
            var member = new ChannelAccount
            {
                Id = "123",
                Name = "bot",
            };
            membersAdded.Add(member);
            var activity = new Activity()
            {
                Type = "conversationUpdate",
                MembersAdded = membersAdded,
                Conversation = new ConversationAccount(null, null, conversationId, null, null, null, null),
            };

            var payload = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity, SerializationSettings.DefaultDeserializationSettings)));
            var fakeContentStreamId = Guid.NewGuid();
            var fakeContentStream = new FakeContentStream(fakeContentStreamId, "application/json", payload);
            var testRequest = new ReceiveRequest
            {
                Path = $"/v3/conversations/{activity.Conversation?.Id}/activities/{activity.Id}",
                Verb = "POST",
            };
            testRequest.Streams.Add(fakeContentStream);

            // Act
            _ = await handler.ProcessRequestAsync(testRequest);
            handler.ForgetConversation(conversationId);

            // Assert
            Assert.False(handler.HasConversation(conversationId));
        }

        [Fact]
        public async Task RequestHandlerAssignsAServiceUrl()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), "fakePipe");
            var conversationId = "testconvoid";
            var serviceUrl = "urn:FakeName:fakeProtocol://fakePath";
            var membersAdded = new List<ChannelAccount>();
            var member = new ChannelAccount
            {
                Id = "123",
                Name = "bot",
            };
            membersAdded.Add(member);
            var activity = new Activity()
            {
                ServiceUrl = serviceUrl,
                Type = "conversationUpdate",
                MembersAdded = membersAdded,
                Conversation = new ConversationAccount(null, null, conversationId, null, null, null, null),
            };

            var payload = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity, SerializationSettings.DefaultDeserializationSettings)));
            var fakeContentStreamId = Guid.NewGuid();
            var fakeContentStream = new FakeContentStream(fakeContentStreamId, "application/json", payload);
            var testRequest = new ReceiveRequest
            {
                Path = $"/v3/conversations/{activity.Conversation?.Id}/activities/{activity.Id}",
                Verb = "POST",
            };
            testRequest.Streams.Add(fakeContentStream);

            // Act
            _ = await handler.ProcessRequestAsync(testRequest);

            // Assert
            Assert.Equal(serviceUrl, handler.ServiceUrl);
        }

        [Fact]
        public async Task ItGetsUserAgentInfo()
        {
            // Arrange

            // Act
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), "fakePipe");
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

        private class MessageBot : IBot
        {
            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default) => await turnContext.SendActivityAsync(MessageFactory.Text("do.not.go.gentle.into.that.good.night"));
        }

        private class FakeContentStream : IContentStream
        {
            public FakeContentStream(Guid id, string contentType, Stream stream)
            {
                Id = id;
                ContentType = contentType;
                Stream = stream;
                Length = int.Parse(stream.Length.ToString());
            }

            public Guid Id { get; set; }

            public string ContentType { get; set; }

            public int? Length { get; set; }

            public Stream Stream { get; set; }
        }

        private class FauxSock : WebSocket
        {
            public override WebSocketCloseStatus? CloseStatus => throw new NotImplementedException();

            public override string CloseStatusDescription => throw new NotImplementedException();

            public override WebSocketState State => throw new NotImplementedException();

            public override string SubProtocol => throw new NotImplementedException();

            public override void Abort()
            {
                throw new NotImplementedException();
            }

            public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }

            public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
