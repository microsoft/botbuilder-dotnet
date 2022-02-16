// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Streaming;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Connector.Streaming.Payloads;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Connector.Streaming.Tests.Application
{
    public class StreamingRequestHandlerTests
    {
        public static IEnumerable<object[]> GetSuccessfulConstructorTestData(int scenario)
        {
            var testData = new List<object[]>
            {
                new object[] { Guid.NewGuid().ToString(), null },
                new object[] { Guid.NewGuid().ToString(), "audience" },
                new object[] { new FauxSock(), null },
                new object[] { new FauxSock(), "audience" }
            };

            return new List<object[]> { testData[scenario] };
        }

        [Theory]
        [MemberData(nameof(GetSuccessfulConstructorTestData), parameters: 0)]
        [MemberData(nameof(GetSuccessfulConstructorTestData), parameters: 1)]
        public void CanBeConstructedWithANamedPipe(string namedPipe, string audience)
        {
            // Arrange

            // Act
            var handler = new StreamingRequestHandler(new MockBot(), new FakeCloudAdapter(), new NamedPipeStreamingConnection(namedPipe, null), audience);

            // Assert
            Assert.NotNull(handler);
            Assert.Equal(audience, handler.Audience);
        }

        [Fact]
        public void ThrowsIfSocketIsNull()
        {
            // Arrange
            Exception result = null;

            // Act
            try
            {
                var handler = new StreamingRequestHandler(new MockBot(), new FakeCloudAdapter(), new WebSocketStreamingConnection(socket: null, null));
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
                var handler = new StreamingRequestHandler(new MockBot(), new FakeCloudAdapter(), new NamedPipeStreamingConnection(pipeName: string.Empty, null));
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsType<ArgumentNullException>(result);
        }

        [Theory]
        [MemberData(nameof(GetSuccessfulConstructorTestData), parameters: 2)]
        [MemberData(nameof(GetSuccessfulConstructorTestData), parameters: 3)]
        public void CanBeConstructedWithAWebSocket(FauxSock socket, string audience)
        {
            // Arrange 

            // Act
            var handler = new StreamingRequestHandler(new MockBot(), new FakeCloudAdapter(), new WebSocketStreamingConnection(socket, null), audience);

            // Assert
            Assert.NotNull(handler);
            Assert.Equal(audience, handler.Audience);
        }

        [Fact]
        public async void RequestHandlerRespondsWith500OnError()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new FakeCloudAdapter(), new NamedPipeStreamingConnection(Guid.NewGuid().ToString(), null));
            var conversationId = Guid.NewGuid().ToString();
            var member = new ChannelAccount
            {
                Id = "123",
                Name = "bot",
            };
            var activity = new Activity
            {
                Type = "conversationUpdate",
                Conversation = new ConversationAccount(null, null, conversationId),
            };
            activity.MembersAdded.Add(member);

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
        public async void DoesNotThrowExceptionIfReceiveRequestIsNull()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new FakeCloudAdapter(), new NamedPipeStreamingConnection(Guid.NewGuid().ToString(), null));
            ReceiveRequest testRequest = null;

            // Act
            var response = await handler.ProcessRequestAsync(testRequest);

            // Assert
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public async void DoesNotThrowExceptionIfReceiveRequestHasNoActivity()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new FakeCloudAdapter(), new NamedPipeStreamingConnection(Guid.NewGuid().ToString(), null));

            var payload = new MemoryStream();
            var fakeContentStreamId = Guid.NewGuid();
            var fakeContentStream = new FakeContentStream(fakeContentStreamId, "application/json", payload);
            var testRequest = new ReceiveRequest
            {
                Verb = "POST",
            };
            testRequest.Streams.Add(fakeContentStream);

            // Act
            var response = await handler.ProcessRequestAsync(testRequest);

            // Assert
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public async void RequestHandlerRemembersConversations()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new FakeCloudAdapter(), new NamedPipeStreamingConnection(Guid.NewGuid().ToString(), null));
            var conversationId = Guid.NewGuid().ToString();
            var member = new ChannelAccount
            {
                Id = "123",
                Name = "bot",
            };

            var activity = new Activity
            {
                Type = "conversationUpdate",
                Conversation = new ConversationAccount(null, null, conversationId),
            };
            activity.MembersAdded.Add(member);

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
        public async void RequestHandlerForgetsConversations()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new FakeCloudAdapter(), new NamedPipeStreamingConnection(Guid.NewGuid().ToString(), null));
            var conversationId = Guid.NewGuid().ToString();
            var member = new ChannelAccount
            {
                Id = "123",
                Name = "bot",
            };
            var activity = new Activity
            {
                Type = "conversationUpdate",

                Conversation = new ConversationAccount(null, null, conversationId),
            };
            activity.MembersAdded.Add(member);

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
        public async void RequestHandlerAssignsAServiceUrl()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new FakeCloudAdapter(), new NamedPipeStreamingConnection(Guid.NewGuid().ToString(), null));
            var conversationId = Guid.NewGuid().ToString();
            const string serviceUrl = "urn:FakeName:fakeProtocol://fakePath";
            var member = new ChannelAccount
            {
                Id = "123",
                Name = "bot",
            };
            var activity = new Activity
            {
                ServiceUrl = serviceUrl,
                Type = "conversationUpdate",
                Conversation = new ConversationAccount(null, null, conversationId),
            };
            activity.MembersAdded.Add(member);

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
        public async void ItGetsUserAgentInfo()
        {
            // Arrange
            var expectation = new Regex("{\"userAgent\":\"Microsoft-BotFramework\\/[0-9.]+\\s.*BotBuilder\\/[0-9.]+\\s+\\(.*\\)\".*}");

            // Act
            var handler = new StreamingRequestHandler(new MockBot(), new FakeCloudAdapter(), new NamedPipeStreamingConnection(Guid.NewGuid().ToString(), null));
            var activity = new Activity
            {
                Type = "message",
                Text = "received from bot",
                From = new ChannelAccount
                {
                    Id = "bot",
                    Name = "bot",
                },
                Conversation = new ConversationAccount(null, null, Guid.NewGuid().ToString()),
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
            Assert.Matches(expectation, response.Streams[0].Content.ReadAsStringAsync().Result);
        }

        public class FauxSock : WebSocket
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

        private class FakeCloudAdapter : CloudAdapterBase, IStreamingActivityProcessor
        {
            public FakeCloudAdapter()
                : base(BotFrameworkAuthenticationFactory.Create())
            {
            }

            public Task<InvokeResponse> ProcessStreamingActivityAsync(Activity activity, BotCallbackHandler botCallbackHandler, CancellationToken cancellationToken = default)
            {
                var authResult = new AuthenticateRequestResult();
                return ProcessActivityAsync(authResult, activity, botCallbackHandler, cancellationToken);
            }
        }

        private class MockBot : IBot
        {
            public bool ThrowDuringOnTurnAsync { get; set; } = false;

            public List<Activity> Activities { get; set; } = new List<Activity>();

            public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                if (ThrowDuringOnTurnAsync)
                {
                    throw new InvalidOperationException();
                }

                Activities.Add(turnContext.Activity);
                return Task.CompletedTask;
            }
        }
    }
}
