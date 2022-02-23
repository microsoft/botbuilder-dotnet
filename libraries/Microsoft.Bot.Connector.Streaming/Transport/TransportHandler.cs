// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Payloads;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector.Streaming.Transport
{
    internal class TransportHandler : IObservable<(Header Header, ReadOnlySequence<byte> Payload)>, IDisposable
    {
        private readonly IDuplexPipe _transport;
        private readonly ILogger _logger;

        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1);
        private readonly TimeSpan _semaphoreTimeout = TimeSpan.FromSeconds(10);
        private readonly byte[] _sendHeaderBuffer = new byte[TransportConstants.MaxHeaderLength];

        private IObserver<(Header, ReadOnlySequence<byte>)> _observer;
        private bool _disposedValue;

        public TransportHandler(IDuplexPipe transport, ILogger logger)
        {
            _transport = transport;
            _logger = logger;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to catch all exceptions in the message loop.")]
        public async Task ListenAsync(CancellationToken cancellationToken)
        {
            var input = _transport.Input;
            bool aborted = false;

            while (!cancellationToken.IsCancellationRequested)
            {
                ReadResult result;

                result = await input.ReadAsync().ConfigureAwait(false);

                if (result.IsCanceled)
                {
                    break;
                }

                var buffer = result.Buffer;

                try
                {
                    if (!buffer.IsEmpty)
                    {
                        while (TryParseHeader(ref buffer, out Header header))
                        {
                            Log.PayloadReceived(_logger, header);

                            ReadOnlySequence<byte> payload = ReadOnlySequence<byte>.Empty;

                            if (header.PayloadLength > 0)
                            {
                                while (buffer.Length < header.PayloadLength)
                                {
                                    input.AdvanceTo(buffer.Start, buffer.End);

                                    result = await input.ReadAsync().ConfigureAwait(false);

                                    if (result.IsCanceled)
                                    {
                                        break;
                                    }

                                    if (!result.Buffer.IsEmpty)
                                    {
                                        buffer = result.Buffer;
                                    }

                                    if (result.IsCompleted)
                                    {
                                        break;
                                    }
                                }

                                if (buffer.Length < header.PayloadLength)
                                {
                                    break;
                                }

                                payload = buffer.Slice(buffer.Start, header.PayloadLength);
                                buffer = buffer.Slice(header.PayloadLength);
                            }

                            _observer.OnNext((header, payload));
                        }
                    }

                    if (result.IsCompleted)
                    {
                        if (buffer.IsEmpty)
                        {
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Don't treat OperationCanceledException as an error, it's basically a "control flow"
                    // exception to stop things from running.
                }
                catch (Exception ex)
                {
                    Log.ReadFrameFailed(_logger, ex);

                    // This failure means we are tearing down the connection, so return and let the cancellation 
                    // and draining take place.
                    await input.CompleteAsync(ex).ConfigureAwait(false);
                    aborted = true;

                    Log.ListenError(_logger, ex);

                    return;
                }
                finally
                {
                    if (!aborted)
                    {
                        input.AdvanceTo(buffer.Start, buffer.End);
                    }
                }
            }

            await input.CompleteAsync().ConfigureAwait(false);

            await _transport.Output.CompleteAsync().ConfigureAwait(false);
            Log.ListenCompleted(_logger);
        }

        public Task StopAsync()
        {
            _transport.Input.CancelPendingRead();
            return Task.CompletedTask;
        }

        public virtual async Task SendResponseAsync(Guid id, ResponsePayload response, CancellationToken cancellationToken = default)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));

            var responseHeader = new Header()
            {
                Type = PayloadTypes.Response,
                Id = id,
                PayloadLength = (int)responseBytes.Length,
                End = true,
            };

            await WriteAsync(
                header: responseHeader,
                writeFunc: async pipeWriter => await pipeWriter.WriteAsync(responseBytes).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public virtual async Task SendRequestAsync(Guid id, RequestPayload request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

            var requestHeader = new Header()
            {
                Type = PayloadTypes.Request,
                Id = id,
                PayloadLength = (int)requestBytes.Length,
                End = true,
            };

            await WriteAsync(
                header: requestHeader,
                writeFunc: async pipeWriter => await pipeWriter.WriteAsync(requestBytes).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public virtual async Task SendStreamAsync(Guid id, Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (stream.Length > TransportConstants.MaxPayloadLength)
            {
                // Break stream into chunks of size `TransportConstants.MaxPayloadLength`
                await SendStreamInChunksAsync(id, stream).ConfigureAwait(false);
            }
            else
            {
                // No chunking needed, copy the entire stream to pipe directly
                var streamHeader = new Header
                {
                    Type = PayloadTypes.Stream,
                    Id = id,
                    PayloadLength = (int)stream.Length,
                    End = true
                };

                await WriteAsync(streamHeader, pipeWriter => stream.CopyToAsync(pipeWriter)).ConfigureAwait(false);
            }
        }

        public IDisposable Subscribe(IObserver<(Header, ReadOnlySequence<byte>)> observer)
        {
            if (_observer != null)
            {
                throw new InvalidOperationException("The protocol expects only a single observer.");
            }

            _observer = observer ?? throw new ArgumentNullException(nameof(observer));

            return null;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _writeLock?.Dispose();
                }

                _disposedValue = true;
            }
        }

        private static bool TryParseHeader(ref ReadOnlySequence<byte> buffer, out Header header)
        {
            if (buffer.IsEmpty)
            {
                header = null;
                return false;
            }

            var length = Math.Min(TransportConstants.MaxHeaderLength, buffer.Length);
            var headerBuffer = buffer.Slice(0, length);

            if (headerBuffer.Length != TransportConstants.MaxHeaderLength)
            {
                header = null;
                return false;
            }

            // Optimization opportunity: instead of headerBuffer.ToArray() which does a 48 byte heap allocation,
            // do a best effort attempt to use MemoryMashal.TryGetArray. Since it has a lot of corner cases, 
            // keeping it simple for now and we can optimize further if data says we required it.
            // Alternatively we can have a 48 byte buffer that we reuse, considering that we always
            // have a single thread running a given transportHandler instance.
            header = HeaderSerializer.Deserialize(headerBuffer.ToArray(), TransportConstants.MaxHeaderLength);

            buffer = buffer.Slice(TransportConstants.MaxHeaderLength);

            return true;
        }

        private async Task SendStreamInChunksAsync(Guid id, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var chunk = new byte[TransportConstants.MaxPayloadLength];

            var remaining = stream.Length;
            while (remaining > 0)
            {
                var current = Math.Min(remaining, TransportConstants.MaxPayloadLength);

                current = await stream.ReadAsync(chunk, 0, (int)current).ConfigureAwait(false);

                var streamHeader = new Header
                {
                    Type = PayloadTypes.Stream,
                    Id = id,
                    PayloadLength = (int)current,
                    End = !(remaining > current)
                };

                var payload = new ReadOnlyMemory<byte>(chunk, 0, (int)current);

                await WriteAsync(
                        header: streamHeader,
                        writeFunc: async pipeWriter => await pipeWriter.WriteAsync(payload).ConfigureAwait(false))
                    .ConfigureAwait(false);

                remaining -= current;
            }
        }

        private async Task WriteAsync(Header header, Func<PipeWriter, Task> writeFunc, CancellationToken cancellationToken = default)
        {
            var output = _transport.Output;
            Log.SendingPayload(_logger, header);

            if (await _writeLock.WaitAsync(_semaphoreTimeout, cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    HeaderSerializer.Serialize(header, _sendHeaderBuffer, 0);
                    await output.WriteAsync(_sendHeaderBuffer).ConfigureAwait(false);
                    await writeFunc(output).ConfigureAwait(false);
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            else
            {
                Log.SemaphoreTimeOut(_logger, header);
            }
        }

        private static class Log
        {
            private static readonly Action<ILogger, Guid, char, int, bool, Exception> _payloadReceived =
                LoggerMessage.Define<Guid, char, int, bool>(LogLevel.Debug, new EventId(1, nameof(PayloadReceived)), "Payload received. Header: ID {Guid} Type {Char} Payload length:{Int32}. End :{Boolean}.");

            private static readonly Action<ILogger, Exception> _readFrameFailed =
                LoggerMessage.Define(LogLevel.Error, new EventId(2, nameof(ReadFrameFailed)), "Failed to read frame from transport.");

            private static readonly Action<ILogger, Guid, char, int, bool, Exception> _payloadSending =
                LoggerMessage.Define<Guid, char, int, bool>(LogLevel.Debug, new EventId(3, nameof(SendingPayload)), "Sending Payload. Header: ID {Guid} Type {Char} Payload length:{Int32}. End :{Boolean}.");

            private static readonly Action<ILogger, Guid, char, int, bool, Exception> _semaphoreTimeOut =
                LoggerMessage.Define<Guid, char, int, bool>(LogLevel.Error, new EventId(4, nameof(SemaphoreTimeOut)), "Timed out trying to acquire write semaphore. Header: ID {Guid} Type {Char} Payload length:{Int32}. End :{Boolean}.");

            private static readonly Action<ILogger, Exception> _listenError =
                LoggerMessage.Define(LogLevel.Error, new EventId(5, nameof(ListenError)), "TransportHandler encountered an error and will stop listening.");

            private static readonly Action<ILogger, Exception> _listenCompleted =
                LoggerMessage.Define(LogLevel.Information, new EventId(6, nameof(ListenCompleted)), "TransportHandler listen task completed.");

            public static void PayloadReceived(ILogger logger, Header header) => _payloadReceived(logger, header.Id, header.Type, header.PayloadLength, header.End, null);

            public static void ReadFrameFailed(ILogger logger, Exception ex) => _readFrameFailed(logger, ex);

            public static void SendingPayload(ILogger logger, Header header) => _payloadSending(logger, header.Id, header.Type, header.PayloadLength, header.End, null);

            public static void SemaphoreTimeOut(ILogger logger, Header header) => _semaphoreTimeOut(logger, header.Id, header.Type, header.PayloadLength, header.End, null);

            public static void ListenError(ILogger logger, Exception ex) => _listenError(logger, ex);

            public static void ListenCompleted(ILogger logger) => _listenCompleted(logger, null);
        }
    }
}
