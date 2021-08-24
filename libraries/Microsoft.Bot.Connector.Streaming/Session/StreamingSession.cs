// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Payloads;
using Microsoft.Bot.Connector.Streaming.Transport;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Streaming.Session
{
    internal class StreamingSession
    {
        private readonly ConcurrentDictionary<Guid, StreamDefinition> _streamDefinitions = new ConcurrentDictionary<Guid, StreamDefinition>();
        private readonly ConcurrentDictionary<Guid, ReceiveRequest> _requests = new ConcurrentDictionary<Guid, ReceiveRequest>();
        private readonly ConcurrentDictionary<Guid, ReceiveResponse> _responses = new ConcurrentDictionary<Guid, ReceiveResponse>();
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>> _pendingResponses = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();

        private readonly RequestHandler _receiver;
        private readonly TransportHandler _sender;

        private readonly ILogger _logger;

        public StreamingSession(RequestHandler receiver, TransportHandler sender, ILogger logger)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _sender.Subscribe(new ProtocolDispatcher(this));

            _logger = logger ?? NullLogger.Instance;
        }

        public async Task<ReceiveResponse> SendRequestAsync(StreamingRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var payload = new RequestPayload()
            {
                Verb = request.Verb,
                Path = request.Path,
            };

            if (request.Streams != null)
            {
                payload.Streams = new List<StreamDescription>();
                foreach (var contentStream in request.Streams)
                {
                    var description = GetStreamDescription(contentStream);

                    payload.Streams.Add(description);
                }
            }

            var requestId = Guid.NewGuid();

            var responseCompletionSource = new TaskCompletionSource<ReceiveResponse>();
            _pendingResponses.TryAdd(requestId, responseCompletionSource);

            // Send request
            await _sender.SendRequestAsync(requestId, payload, cancellationToken).ConfigureAwait(false);

            foreach (var stream in request.Streams)
            {
                await _sender.SendStreamAsync(stream.Id, await stream.Content.ReadAsStreamAsync().ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
            }

            return await responseCompletionSource.Task.ConfigureAwait(false);
        }

        public async Task SendResponseAsync(Header header, StreamingResponse response, CancellationToken cancellationToken)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (header.Type != PayloadTypes.Response)
            {
                throw new InvalidOperationException($"StreamingSession SendResponseAsync expected Response payload, but instead received a payload of type {header.Type}");
            }

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var payload = new ResponsePayload()
            {
                StatusCode = response.StatusCode,
            };

            if (response.Streams != null)
            {
                payload.Streams = new List<StreamDescription>();
                foreach (var contentStream in response.Streams)
                {
                    var description = GetStreamDescription(contentStream);

                    payload.Streams.Add(description);
                }
            }

            await _sender.SendResponseAsync(header.Id, payload, cancellationToken).ConfigureAwait(false);
        }

        public virtual void ReceiveRequest(Header header, ReceiveRequest request)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (header.Type != PayloadTypes.Request)
            {
                throw new InvalidOperationException($"StreamingSession cannot receive payload of type {header.Type} as request.");
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Log.PayloadReceived(_logger, header);

            _requests.TryAdd(header.Id, request);

            if (request.Streams.Any())
            {
                foreach (var streamDefinition in request.Streams)
                {
                    _streamDefinitions.TryAdd(streamDefinition.Id, streamDefinition as StreamDefinition);
                }
            }
            else
            {
                ProcessRequest(header.Id, request);
            }
        }

        public virtual void ReceiveResponse(Header header, ReceiveResponse response)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (header.Type != PayloadTypes.Response)
            {
                throw new InvalidOperationException($"StreamingSession cannot receive payload of type {header.Type} as response");
            }

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            Log.PayloadReceived(_logger, header);

            if (response.Streams.Count == 0)
            {
                if (_pendingResponses.TryRemove(header.Id, out TaskCompletionSource<ReceiveResponse> responseTask))
                {
                    responseTask.SetResult(response);
                }
            }
            else
            {
                _responses.TryAdd(header.Id, response);

                foreach (var streamDefinition in response.Streams)
                {
                    _streamDefinitions.TryAdd(streamDefinition.Id, streamDefinition as StreamDefinition);
                }
            }
        }

        public virtual void ReceiveStream(Header header, ArraySegment<byte> payload)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (header.Type != PayloadTypes.Stream)
            {
                throw new InvalidOperationException($"StreamingSession cannot receive payload of type {header.Type} as stream");
            }

            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            Log.PayloadReceived(_logger, header);

            // Find request for incoming stream header
            if (_streamDefinitions.TryGetValue(header.Id, out StreamDefinition streamDefinition))
            {
                streamDefinition.Stream.Write(payload.Array, payload.Offset, payload.Count);

                // Is this the end of this stream?
                if (header.End)
                {
                    // Mark this stream as completed
                    if (streamDefinition is StreamDefinition streamDef)
                    {
                        streamDef.Complete = true;
                        streamDef.Stream.Seek(0, SeekOrigin.Begin);

                        List<IContentStream> streams = null;

                        // Find the request / response
                        if (streamDef.PayloadType == PayloadTypes.Request)
                        {
                            if (_requests.TryGetValue(streamDef.PayloadId, out ReceiveRequest req))
                            {
                                streams = req.Streams;
                            }
                        }
                        else if (streamDef.PayloadType == PayloadTypes.Response)
                        {
                            if (_responses.TryGetValue(streamDef.PayloadId, out ReceiveResponse res))
                            {
                                streams = res.Streams;
                            }
                        }

                        if (streams != null)
                        {
                            // Have we completed all the streams we expect for this request?
                            bool allStreamsDone = streams.All(s => s is StreamDefinition streamDef && streamDef.Complete);

                            // If we received all the streams, then it's time to pass this request to the request handler!
                            // For example, if this request is a send activity, the request handler will deserialize the first stream
                            // into an activity and pass to the adapter.
                            if (allStreamsDone)
                            {
                                if (streamDef.PayloadType == PayloadTypes.Request)
                                {
                                    if (_requests.TryRemove(streamDef.PayloadId, out ReceiveRequest request))
                                    {
                                        ProcessRequest(streamDef.PayloadId, request);
                                    }
                                }
                                else if (streamDef.PayloadType == PayloadTypes.Response)
                                {
                                    if (_responses.TryRemove(streamDef.PayloadId, out ReceiveResponse response))
                                    {
                                        if (_pendingResponses.TryRemove(streamDef.PayloadId, out TaskCompletionSource<ReceiveResponse> responseTask))
                                        {
                                            responseTask.SetResult(response);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Log.OrphanedStream(_logger, header);
            }
        }

        private static StreamDescription GetStreamDescription(ResponseMessageStream stream)
        {
            var description = new StreamDescription()
            {
                Id = stream.Id.ToString("D"),
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

        private static ArraySegment<byte> GetArraySegment(ReadOnlySequence<byte> sequence)
        {
            if (sequence.IsSingleSegment)
            {
                if (MemoryMarshal.TryGetArray(sequence.First, out ArraySegment<byte> segment))
                {
                    return segment;
                }
            }

            // Can be optimized by not copying but should be uncommon. If perf data shows that we are hitting this
            // code branch, then we can optimize and avoid copies and heap allocations.
            return new ArraySegment<byte>(sequence.ToArray());
        }

        private void ProcessRequest(Guid id, ReceiveRequest request)
        {
            _ = Task.Run(async () =>
            {
                var streamingResponse = await _receiver.ProcessRequestAsync(request, null).ConfigureAwait(false);
                await SendResponseAsync(new Header() { Id = id, Type = PayloadTypes.Response }, streamingResponse, CancellationToken.None).ConfigureAwait(false);

                request.Streams.ForEach(s => _streamDefinitions.TryRemove(s.Id, out StreamDefinition removedStream));
            });
        }

        internal class ProtocolDispatcher : IObserver<(Header Header, ReadOnlySequence<byte> Payload)>
        {
            private readonly StreamingSession _streamingSession;

            public ProtocolDispatcher(StreamingSession streamingSession)
            {
                _streamingSession = streamingSession ?? throw new ArgumentNullException(nameof(streamingSession));
            }

            public void OnCompleted()
            {
                throw new NotImplementedException();
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            public void OnNext((Header Header, ReadOnlySequence<byte> Payload) frame)
            {
                var header = frame.Header;
                var payload = frame.Payload;

                switch (header.Type)
                {
                    case PayloadTypes.Stream:
                        _streamingSession.ReceiveStream(header, GetArraySegment(payload));

                        break;
                    case PayloadTypes.Request:

                        var requestPayload = DeserializeTo<RequestPayload>(payload);
                        var request = new ReceiveRequest()
                        {
                            Verb = requestPayload.Verb,
                            Path = requestPayload.Path,
                            Streams = new List<IContentStream>(),
                        };

                        CreatePlaceholderStreams(header, request.Streams, requestPayload.Streams);
                        _streamingSession.ReceiveRequest(header, request);

                        break;

                    case PayloadTypes.Response:

                        var responsePayload = DeserializeTo<ResponsePayload>(payload);
                        var response = new ReceiveResponse()
                        {
                            StatusCode = responsePayload.StatusCode,
                            Streams = new List<IContentStream>(),
                        };

                        CreatePlaceholderStreams(header, response.Streams, responsePayload.Streams);
                        _streamingSession.ReceiveResponse(header, response);

                        break;

                    case PayloadTypes.CancelAll:
                        break;

                    case PayloadTypes.CancelStream:
                        break;
                }
            }

            private static T DeserializeTo<T>(ReadOnlySequence<byte> payload)
            {
                // Perf note: ToArray() allocates a buffer in the heap and makes a copy. However, the request payload itself 
                // constrained in terms of size so we can do ToArray(). For Stream payloads we do not do allocations given that 
                // size is more unbounded.
                // When we move out of 2.1 we could use the Span<byte> overload of GetString that uses the span in the stack avoiding heap
                // allocations alltogether. Another alternative is to use the Utf8JsonTextReader from the native json library which takes 
                // stack-allocated bytes instead of json.net.
                using (var stream = new MemoryStream(payload.ToArray()))
                using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    // We don't just deserialize the utf8 string because the legacy client wraps the json with double quotes. This  can
                    // be optimized but for now solves the issue of supporting new and legacy clients
                    return JsonSerializer.Create(SerializationSettings.DefaultDeserializationSettings).Deserialize<T>(jsonReader);
                }       
            }

            private static void CreatePlaceholderStreams(Header header, List<IContentStream> placeholders, List<StreamDescription> streamInfo)
            {
                if (streamInfo != null)
                {
                    foreach (var streamDescription in streamInfo)
                    {
                        if (!Guid.TryParse(streamDescription.Id, out Guid id))
                        {
                            throw new InvalidDataException($"Stream description id '{streamDescription.Id}' is not a Guid");
                        }

                        placeholders.Add(new StreamDefinition()
                        {
                            ContentType = streamDescription.ContentType,
                            Length = streamDescription.Length,
                            Id = Guid.Parse(streamDescription.Id),
                            Stream = new MemoryStream(),
                            PayloadType = header.Type,
                            PayloadId = header.Id
                        });
                    }
                }
            }
        }

        internal class StreamDefinition : IContentStream
        {
            public Guid Id { get; set; }

            public string ContentType { get; set; }

            public int? Length { get; set; }

            public Stream Stream { get; set; }

            public bool Complete { get; set; }

            public char PayloadType { get; set; }

            public Guid PayloadId { get; set; }
        }

        private class Log
        {
            private static readonly Action<ILogger, Header, Exception> _orphanedStream =
                LoggerMessage.Define<Header>(LogLevel.Error, new EventId(1, nameof(OrphanedStream)), "Stream has no associated payload. Header: {Header}.");

            private static readonly Action<ILogger, Header, Exception> _payloadReceived =
                LoggerMessage.Define<Header>(LogLevel.Debug, new EventId(2, nameof(PayloadReceived)), "Payload received in session. Header: {Header}.");

            public static void OrphanedStream(ILogger logger, Header header) => _orphanedStream(logger, header, null);

            public static void PayloadReceived(ILogger logger, Header header) => _payloadReceived(logger, header, null);
        }
    }
}
