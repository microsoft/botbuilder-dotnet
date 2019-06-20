using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Protocol.Payloads;
using Microsoft.Bot.Protocol.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Bot.Protocol.Payloads
{
    public class ReceiveRequestAssembler : PayloadAssembler
    {
        private readonly Func<Guid, ReceiveRequest, Task> _onCompleted;
        private readonly IStreamManager _streamManager;
        private readonly int? _length;

        public ReceiveRequestAssembler(Header header, IStreamManager streamManager, Func<Guid, ReceiveRequest, Task> onCompleted)
            : base(header.Id)
        {
            _streamManager = streamManager;
            _onCompleted = onCompleted;

            _length = header.End ? (int?)header.PayloadLength : null;
        }

        public override Stream CreatePayloadStream()
        {
            if (_length.HasValue)
            {
                return new MemoryStream(_length.Value);
            }
            else
            {
                return new MemoryStream();
            }
        }

        public override void OnReceive(Header header, Stream stream, int contentLength)
        {
            // Call base functionality first so that we can fire off a new Task when completed
            base.OnReceive(header, stream, contentLength);

            if (header.End)
            {
                // Move stream back to the beginning for reading
                stream.Position = 0;

                // Execute the request on a seperate Task
                Background.Run(() => ProcessRequest(stream));
            }
            // else: still receiving data into the stream
        }

        private async Task ProcessRequest(Stream stream)
        {
            using (var textReader = new StreamReader(stream))
            {
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    var requestPayload = Serializer.Deserialize<RequestPayload>(jsonReader);

                    var request = new ReceiveRequest()
                    {
                        Verb = requestPayload.Verb,
                        Path = requestPayload.Path,
                        Streams = new List<IContentStream>()
                    };

                    if (requestPayload.Streams != null)
                    {
                        foreach (var streamDescription in requestPayload.Streams)
                        {
                            if (!Guid.TryParse(streamDescription.Id, out Guid id))
                            {
                                throw new InvalidDataException($"Stream description id '{streamDescription.Id}' is not a Guid");
                            }

                            var streamAssembler = _streamManager.GetPayloadAssembler(id);
                            streamAssembler.ContentType = streamDescription.Type;
                            streamAssembler.ContentLength = streamDescription.Length;

                            request.Streams.Add(new ContentStream(id, streamAssembler)
                            {
                                Length = streamDescription.Length,
                                Type = streamDescription.Type
                            });
                        }
                    }

                    await _onCompleted(this.Id, request).ConfigureAwait(false);
                }
            }
        }
    }
}
