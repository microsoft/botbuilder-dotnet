// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Debugging.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReaderWriterLock = Microsoft.Bot.Builder.Dialogs.Debugging.Base.ReaderWriterLock;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    internal abstract class DebugTransport : IDisposable
    {
        private const string Prefix = @"Content-Length: ";
        private static readonly Encoding Encoding = Encoding.ASCII;

        private readonly ReaderWriterLock _connected = new ReaderWriterLock(writer: true);
        private readonly SemaphoreSlim _readable = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _writable = new SemaphoreSlim(1, 1);

        // To detect redundant calls to dispose
        private bool _disposed;
        private StreamReader _reader;
        private StreamWriter _writer;

        protected DebugTransport(ILogger logger)
        {
            Logger = logger ?? NullLogger.Instance;
        }

        protected ILogger Logger { get; set; }

        /// <summary>
        /// Disposes the object instance a releases any related objects owned by the class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes objects used by the class.
        /// </summary>
        /// <param name="disposing">A Boolean that indicates whether the method call comes from a Dispose method (its value is true) or from a finalizer (its value is false).</param>
        /// <remarks>
        /// The disposing parameter should be false when called from a finalizer, and true when called from the IDisposable.Dispose method.
        /// In other words, it is true when deterministically called and false when non-deterministically called.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed objects owned by the class here.
                _connected?.Dispose();
                _readable?.Dispose();
                _writable?.Dispose();
                _reader?.Dispose();
                _writer?.Dispose();
            }

            _disposed = true;
        }

        protected async Task ListenAsync(IPEndPoint point, CancellationToken cancellationToken)
        {
            var listener = new TcpListener(point);
            listener.Start();
            using (cancellationToken.Register(listener.Stop))
            {
                var local = (IPEndPoint)listener.LocalEndpoint;

                // output is parsed on launch by "vscode-dialog-debugger\src\ts\extension.ts"
                Console.WriteLine($"{nameof(DebugTransport)}\t{local.Address}\t{local.Port}");
                Trace.TraceInformation($"{nameof(DebugTransport)}\t{local.Address}\t{local.Port}");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        using (var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false))
                        {
                            using (var stream = client.GetStream())
                            {
                                using (_reader = new StreamReader(stream, Encoding))
                                {
                                    using (_writer = new StreamWriter(stream, Encoding))
                                    {
                                        using (cancellationToken.Register(() =>
                                        {
                                            stream.Close();
                                            _reader.Close();
                                            _writer.Close();
                                            client.Close();
                                        }))
                                        {
                                            _connected.ExitWrite();

                                            try
                                            {
                                                await AcceptAsync(cancellationToken).ConfigureAwait(false);
                                            }
                                            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                                            {
                                            }
                                            finally
                                            {
                                                await _connected.EnterWriteAsync(cancellationToken).ConfigureAwait(false);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types (we just log the exception and we continue the execution)
                    catch (Exception error)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        Logger.LogError(error, error.Message);
                    }
                }
            }
        }

        protected abstract Task AcceptAsync(CancellationToken cancellationToken);

        protected async Task<JToken> ReadAsync(CancellationToken cancellationToken)
        {
            var acquired = await _connected.TryEnterReadAsync(cancellationToken).ConfigureAwait(false);

            if (!acquired)
            {
                throw new InvalidOperationException();
            }

            try
            {
                using (await _readable.WithWaitAsync(cancellationToken).ConfigureAwait(false))
                {
                    var line = await _reader.ReadLineAsync().ConfigureAwait(false);
                    if (line == null)
                    {
                        throw new EndOfStreamException();
                    }

                    var empty = await _reader.ReadLineAsync().ConfigureAwait(false);
                    if (empty.Length > 0)
                    {
                        throw new InvalidOperationException();
                    }

                    if (!line.StartsWith(Prefix, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException();
                    }

                    var count = int.Parse(line.Substring(Prefix.Length), CultureInfo.InvariantCulture);

                    var buffer = new char[count];
                    var index = 0;
                    while (index < count)
                    {
                        var bytes = await _reader.ReadAsync(buffer, index, count - index).ConfigureAwait(false);
                        index += bytes;
                    }

                    var json = new string(buffer);
                    var token = JToken.Parse(json);
                    Logger.LogTrace($"READ: {token.ToString(Formatting.None)}");
                    return token;
                }
            }
            finally
            {
                await _connected.ExitReadAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        protected async Task SendAsync(JToken token, CancellationToken cancellationToken)
        {
            var acquired = await _connected.TryEnterReadAsync(cancellationToken).ConfigureAwait(false);
            if (!acquired)
            {
                return;
            }

            try
            {
                using (await _writable.WithWaitAsync(cancellationToken).ConfigureAwait(false))
                {
                    var json = token.ToString(Formatting.None);
                    Logger.LogTrace($"SEND: {json}");
                    var buffer = Encoding.GetBytes(json);
                    await _writer.WriteAsync(Prefix + buffer.Length).ConfigureAwait(false);
                    await _writer.WriteAsync("\r\n\r\n").ConfigureAwait(false);
                    await _writer.WriteAsync(json).ConfigureAwait(false);
                    await _writer.FlushAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await _connected.ExitReadAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
