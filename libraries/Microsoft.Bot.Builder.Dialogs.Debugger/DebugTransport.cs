using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugger
{
    public abstract class DebugTransport
    {
        private readonly ReaderWriterLock connected = new ReaderWriterLock(writer: true);
        private readonly SemaphoreSlim readable = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim writable = new SemaphoreSlim(1, 1);
        private StreamReader reader;
        private StreamWriter writer;

        private const string Prefix = @"Content-Length: ";
        private static readonly Encoding Encoding = Encoding.UTF8;

        protected readonly ILogger logger;

        protected DebugTransport(ILogger logger)
        {
            this.logger = logger ?? NullLogger.Instance;
        }

        protected async Task ListenAsync(IPEndPoint point, CancellationToken cancellationToken)
        {
            var listener = new TcpListener(point);
            listener.Start();
            using (cancellationToken.Register(listener.Stop))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        using (var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false))
                        using (var stream = client.GetStream())
                        using (reader = new StreamReader(stream, Encoding))
                        using (writer = new StreamWriter(stream, Encoding))
                        using (cancellationToken.Register(() =>
                        {
                            stream.Close();
                            reader.Close();
                            writer.Close();
                            client.Close();
                        }))
                        {
                            connected.ExitWrite();

                            try
                            {
                                await AcceptAsync(cancellationToken).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                            {
                            }
                            finally
                            {
                                await connected.EnterWriteAsync(cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception error)
                    {
                        this.logger.LogError(error, error.Message);
                    }
                }
            }
        }

        protected abstract Task AcceptAsync(CancellationToken cancellationToken);

        protected async Task<JToken> ReadAsync(CancellationToken cancellationToken)
        {
            var acquired = await connected.TryEnterReadAsync(cancellationToken).ConfigureAwait(false);

            if (!acquired)
            {
                throw new InvalidOperationException();
            }

            try
            {
                using (await readable.WithWaitAsync(cancellationToken).ConfigureAwait(false))
                {
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (line == null)
                    {
                        throw new EndOfStreamException();
                    }

                    var empty = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (empty.Length > 0)
                    {
                        throw new InvalidOperationException();
                    }

                    if (!line.StartsWith(Prefix))
                    {
                        throw new InvalidOperationException();
                    }

                    var count = int.Parse(line.Substring(Prefix.Length));

                    var buffer = new char[count];
                    int index = 0;
                    while (index < count)
                    {
                        var bytes = await reader.ReadAsync(buffer, index, count - index).ConfigureAwait(false);
                        index += bytes;
                    }

                    var json = new string(buffer);
                    var token = JToken.Parse(json);
                    logger.LogTrace($"READ: {token.ToString(Formatting.None)}");
                    return token;
                }
            }
            finally
            {
                await connected.ExitReadAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        protected async Task SendAsync(JToken token, CancellationToken cancellationToken)
        {
            var acquired = await connected.TryEnterReadAsync(cancellationToken).ConfigureAwait(false);
            if (!acquired)
            {
                return;
            }

            try
            {
                using (await writable.WithWaitAsync(cancellationToken).ConfigureAwait(false))
                {
                    logger.LogTrace($"SEND: {token.ToString(Formatting.None)}");
                    var json = token.ToString();
                    var buffer = Encoding.GetBytes(json);
                    var length = buffer.Length + writer.NewLine.Length;
                    await writer.WriteLineAsync(Prefix + length).ConfigureAwait(false);
                    await writer.WriteLineAsync().ConfigureAwait(false);
                    await writer.WriteLineAsync(json).ConfigureAwait(false);
                    await writer.FlushAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await connected.ExitReadAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
