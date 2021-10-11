// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Bot.Streaming.PayloadTransport;
using Microsoft.Bot.Streaming.Transport;
using Microsoft.Bot.Streaming.UnitTests.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Streaming.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public async Task StreamingE2E()
        {
            var requestHandler = new TestRequestHandler();

            var incomingStream = new MemoryStream();
            var outgoingStream = new MemoryStream();

            var transport = new TestStreamTransport(outgoingStream);
            var server = new TestStreamServer(transport, requestHandler);

            await server.StartAsync();

            await SendMessage("hello streaming!", outgoingStream);
        }

        private async Task SendMessage(string text, Stream stream)
        {
            var activity = MessageFactory.Text(text);

            string jsonActivity = JsonConvert.SerializeObject(activity);
            byte[] jsonActivityBytes = Encoding.UTF8.GetBytes(jsonActivity);
            var activityBytes = new ArraySegment<byte>(jsonActivityBytes);
            var encodedActivity = Encoding.UTF8.GetString(activityBytes.Array);

            StreamingRequest request = new StreamingRequest()
            {
                Verb = StreamingRequest.POST,
                Path = "api/messages",
                Streams = new List<ResponseMessageStream>() { new ResponseMessageStream() { Content = new StringContent(jsonActivity, Encoding.UTF8, "application/json") } },
            };

            // Now that you have your request, you need to send:
            // 1. Request packet: RequestPayload Header + Request Bytes
            // 2. Stream packet: Stream Header + Stream Bytes

            // Prepare the request packet
            var payload = new RequestPayload()
            {
                Verb = request.Verb,
                Path = request.Path,
                Streams = new List<StreamDescription>(),
            };

            payload.Streams.Add(GetStreamDescription(request.Streams.First()));

            string requestPayloadString = JsonConvert.SerializeObject(payload);
            byte[] requestPayloadBytes = Encoding.UTF8.GetBytes(requestPayloadString);

            var header = new Header()
            {
                Type = PayloadTypes.Request,   // the ‘request’ header type is the character ‘A’
                Id = Guid.NewGuid(),
                PayloadLength = requestPayloadBytes.Length,   // you are first just sending the RequestPayload
                End = true,
            };

            byte[] sendHeaderBuffer = new byte[TransportConstants.MaxHeaderLength];
            int headerLength = HeaderSerializer.Serialize(header, sendHeaderBuffer, 0);

            await stream.WriteAsync(sendHeaderBuffer, 0, headerLength);

            header = new Header()
            {
                Type = PayloadTypes.Stream,             // the ‘stream header type is the character ‘S’
                Id = request.Streams.First().Id,    // Id is the Id of the stream
                PayloadLength = activityBytes.Count,
                End = true,
            };

            sendHeaderBuffer = new byte[TransportConstants.MaxHeaderLength];
            headerLength = HeaderSerializer.Serialize(header, sendHeaderBuffer, 0);

            await stream.WriteAsync(sendHeaderBuffer, 0, headerLength);
            await stream.WriteAsync(jsonActivityBytes);
        }

#pragma warning disable SA1204 // Static elements should appear before instance elements
        private static StreamDescription GetStreamDescription(ResponseMessageStream stream)
#pragma warning restore SA1204 // Static elements should appear before instance elements
        {
            var description = new StreamDescription()
            {
                Id = stream.Id.ToString("D")
            };

            if (stream.Content.Headers.TryGetValues(HeaderNames.ContentType, out IEnumerable<string> contentType))
            {
                description.ContentType = contentType?.FirstOrDefault();
            }

            if (stream.Content.Headers.TryGetValues(HeaderNames.ContentLength, out IEnumerable<string> contentLength))
            {
                var value = contentLength?.FirstOrDefault();
                if (value != null && int.TryParse(value, out int length))
                {
                    description.Length = length;
                }
            }
            else
            {
                description.Length = (int?)stream.Content.Headers.ContentLength;
            }

            return description;
        }

        private class TestRequestHandler : RequestHandler
        {
            public override async Task<StreamingResponse> ProcessRequestAsync(ReceiveRequest request, ILogger<RequestHandler> logger, object context = null, CancellationToken cancellationToken = default)
            {
                var response = new StreamingResponse();
                var body = await request.ReadBodyAsStringAsync().ConfigureAwait(false);
                var activity = JsonConvert.DeserializeObject<Activity>(body, SerializationSettings.DefaultDeserializationSettings);

                var responseActivity = MessageFactory.Text($"You said: {activity.Text ?? "empty"}");
                var invokeResponse = new InvokeResponse<Activity>() { Body = responseActivity, Status = 200 };
                response.SetBody(invokeResponse.Body);
                response.StatusCode = 200;

                return response;
            }
        }

        private class TestStreamServer : IStreamingTransportServer
        {
            private readonly RequestHandler _requestHandler;
            private readonly RequestManager _requestManager;
            private readonly IPayloadSender _sender;
            private readonly IPayloadReceiver _receiver;
            private readonly ProtocolAdapter _protocolAdapter;
            private readonly TestStreamTransport _transport;

            public TestStreamServer(TestStreamTransport transport, RequestHandler requestHandler)
            {
                _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
                _requestManager = new RequestManager();
                _sender = new PayloadSender();
                _sender.Disconnected += OnConnectionDisconnected;
                _receiver = new PayloadReceiver();
                _receiver.Disconnected += OnConnectionDisconnected;
                _protocolAdapter = new ProtocolAdapter(_requestHandler, _requestManager, _sender, _receiver);
                _transport = transport;
            }

            public event DisconnectedEventHandler Disconnected;

            public async Task<ReceiveResponse> SendAsync(StreamingRequest request, CancellationToken cancellationToken = default)
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                if (!_sender.IsConnected || !_receiver.IsConnected)
                {
                    throw new InvalidOperationException("The server is not connected.");
                }

                return await _protocolAdapter.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            }

            public async Task StartAsync()
            {
                _sender.Connect(_transport);
                _receiver.Connect(_transport);

                await Task.CompletedTask;
            }

            private void OnConnectionDisconnected(object sender, DisconnectedEventArgs e)
            {
                Disconnected?.Invoke(sender, e);
            }
        }

        private class TestStreamTransport : ITransportSender, ITransportReceiver
        {
            private readonly Stream _stream;

            // To detect redundant calls to dispose
            private bool _disposed;

            public TestStreamTransport(Stream stream)
            {
                _stream = stream;
            }

            public bool IsConnected { get; set; }

            public bool IsClosed { get; set; } = false;

            public void Close()
            {
                IsClosed = true;
                _stream.Close();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public async Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
            {
                try
                {
                    if (_stream != null)
                    {
                        var length = await _stream.ReadAsync(buffer, offset, count).ConfigureAwait(false);
                        return length;
                    }
                }
                catch (ObjectDisposedException)
                {
                    // _stream was disposed by a disconnect
                }

                return 0;
            }

            public async Task<int> SendAsync(byte[] buffer, int offset, int count)
            {
                try
                {
                    if (_stream != null)
                    {
                        await _stream.WriteAsync(buffer, offset, count).ConfigureAwait(false);
                        return count;
                    }
                }
                catch (ObjectDisposedException)
                {
                    // _stream was disposed by a Disconnect call
                }
                catch (IOException)
                {
                    // _stream was disposed by a disconnect of a broken pipe
                }

                return 0;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    // Dispose managed objects owned by the class here.
                    _stream?.Dispose();
                }

                _disposed = true;
            }
        }

        private class SimpleProcessor : IStreamingActivityProcessor
        {
            public async Task<InvokeResponse> ProcessStreamingActivityAsync(Activity activity, BotCallbackHandler botCallbackHandler, CancellationToken cancellationToken = default)
            {
                await botCallbackHandler(null, CancellationToken.None).ConfigureAwait(false);

                return await Task.FromResult((InvokeResponse)activity.Value).ConfigureAwait(false);
            }
        }

        private class MessageBot : IBot
        {
            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
                => await turnContext.SendActivityAsync(MessageFactory.Text("do.not.go.gentle.into.that.good.night"));
        }
    }
}
