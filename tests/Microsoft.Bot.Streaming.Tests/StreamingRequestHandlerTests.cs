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
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Bot.Streaming.UnitTests.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Serialization;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Streaming.Tests
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
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), namedPipe, audience);

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
                var handler = new StreamingRequestHandler(new MockBot(), activityProcessor: new BotFrameworkHttpAdapter(), socket: null);
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
                var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), string.Empty);
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
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), socket, audience);

            // Assert
            Assert.NotNull(handler);
            Assert.Equal(audience, handler.Audience);
        }

        [Fact]
        public async Task RequestHandlerRespondsWith500OnError()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), Guid.NewGuid().ToString());
            var conversationId = Guid.NewGuid().ToString();
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
        public async Task DoesNotThrowExceptionIfReceiveRequestIsNull()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), Guid.NewGuid().ToString());
            ReceiveRequest testRequest = null;

            // Act
            var response = await handler.ProcessRequestAsync(testRequest);

            // Assert
            Assert.Equal(400, response.StatusCode);
        }

        [Fact]
        public async Task DoesNotThrowExceptionIfReceiveRequestHasNoActivity()
        {
            // Arrange
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), Guid.NewGuid().ToString());
            
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
        public async Task RequestHandlerRemembersConversations()
        {
            // Arrange
            var adapter = new BotFrameworkHttpAdapter();
            var handler = new StreamingRequestHandler(new MockBot(), adapter, Guid.NewGuid().ToString());
            var conversationId = Guid.NewGuid().ToString();
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
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), Guid.NewGuid().ToString());
            var conversationId = Guid.NewGuid().ToString();
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
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), Guid.NewGuid().ToString());
            var conversationId = Guid.NewGuid().ToString();
            const string serviceUrl = "urn:FakeName:fakeProtocol://fakePath";
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
            var expectation = new Regex("{\"userAgent\":\"Microsoft-BotFramework\\/[0-9.]+\\s.*BotBuilder\\/[0-9.]+\\s+\\(.*\\)\".*}");

            // Act
            var handler = new StreamingRequestHandler(new MockBot(), new BotFrameworkHttpAdapter(), Guid.NewGuid().ToString());
            var activity = new Schema.Activity()
            {
                Type = "message",
                Text = "received from bot",
                From = new Schema.ChannelAccount()
                {
                    Id = "bot",
                    Name = "bot",
                },
                Conversation = new Schema.ConversationAccount(null, null, Guid.NewGuid().ToString(), null, null, null, null),
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
            Assert.Matches(expectation, await response.Streams[0].Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CallStreamingRequestHandlerOverrides()
        {
            var activity = new Activity()
            {
                Type = "message",
                Text = "received from bot",
                ServiceUrl = "http://localhost",
                ChannelId = "ChannelId",
                From = new Schema.ChannelAccount()
                {
                    Id = "bot",
                    Name = "bot",
                },
                Conversation = new Schema.ConversationAccount(null, null, Guid.NewGuid().ToString(), null, null, null, null),
            };

            // Arrange
            var headerDictionaryMock = new Mock<IHeaderDictionary>();
            headerDictionaryMock.Setup(h => h[It.Is<string>(v => v == "Authorization")]).Returns<string>(null);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(r => r.Body).Returns(CreateStream(activity));
            httpRequestMock.Setup(r => r.Headers).Returns(headerDictionaryMock.Object);
            httpRequestMock.Setup(r => r.Method).Returns(HttpMethods.Get);
            httpRequestMock.Setup(r => r.HttpContext.WebSockets.IsWebSocketRequest).Returns(true);

            var httpResponseMock = new Mock<HttpResponse>();

            var botMock = new Mock<IBot>();
            botMock.Setup(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var methodCalls = new List<string>();
            var adapter = new BotFrameworkHttpAdapterSub(methodCalls);
            await adapter.ProcessAsync(httpRequestMock.Object, httpResponseMock.Object, botMock.Object);

            Assert.Contains("ListenAsync()", methodCalls);
            Assert.Contains("ServerDisconnected()", methodCalls);
        }

        private static Stream CreateStream(Activity activity)
        {
            string json = SafeJsonConvert.SerializeObject(activity, MessageSerializerSettings.Create());
            var stream = new MemoryStream();
            var textWriter = new StreamWriter(stream);
            textWriter.Write(json);
            textWriter.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
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

        private class BotFrameworkHttpAdapterSub : BotFrameworkHttpAdapter
        {
            private List<string> _methodCalls;

            public BotFrameworkHttpAdapterSub(List<string> methodCalls)
            : base()
            {
                _methodCalls = methodCalls;
            }

            public override StreamingRequestHandler CreateStreamingRequestHandler(IBot bot, WebSocket socket, string audience)
            {
                var socketMock = new Mock<WebSocket>();
                return new StreamingRequestHandlerSub(bot, this, socketMock.Object, audience, Logger, _methodCalls);
            }
        }

        private class StreamingRequestHandlerSub : StreamingRequestHandler
        {
            private List<string> _methodCalls;

            private bool _disconnected;

            public StreamingRequestHandlerSub(IBot bot, IStreamingActivityProcessor activityProcessor, WebSocket socket, string audience, ILogger logger = null, List<string> methodCalls = null)
                : base(bot, activityProcessor, socket, audience, logger)
            {
                _methodCalls = methodCalls;
            }

            public override async Task ListenAsync()
            {
                _methodCalls.Add("ListenAsync()");
                await base.ListenAsync();

                // Wait for disconnect to complete in another thread (to avoid race condition between Listen and Disconnect)
                await Task.WhenAny(WaitForDisconnect(), Task.Delay(TimeSpan.FromSeconds(30), CancellationToken.None));
            }

            public override async Task ListenAsync(CancellationToken cancellationToken)
            {
                _methodCalls.Add("ListenAsync()");
                await base.ListenAsync(cancellationToken);

                // Wait for disconnect to complete in another thread (to avoid race condition between Listen and Disconnect)
                await Task.WhenAny(WaitForDisconnect(), Task.Delay(TimeSpan.FromSeconds(30), cancellationToken));
            }

            protected override void ServerDisconnected(object sender, DisconnectedEventArgs e)
            {
                _methodCalls.Add("ServerDisconnected()");
                _disconnected = true;

                base.ServerDisconnected(sender, e);
            }

            private Task WaitForDisconnect()
            {
                while (!_disconnected)
                {
                    Thread.Sleep(100);
                }

                return Task.CompletedTask;
            }
        }
    }
}
