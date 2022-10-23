// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector.Streaming.Transport
{
    internal class NamedPipeTransport : StreamingTransport
    {
        private const string ServerIncomingPath = ".incoming";
        private const string ServerOutgoingPath = ".outgoing";

        private readonly string _pipeName;

        private PipeStream _receiver;
        private PipeStream _sender;
        private bool _disposedValue;
        private int _tryTimes;

        public NamedPipeTransport(string pipeName, IDuplexPipe application, ILogger logger)
            : base(application, logger)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
            {
                throw new ArgumentNullException(nameof(pipeName));
            }

            _pipeName = pipeName;
        }

        public override async Task ConnectAsync(Action<bool> connectionStatusChanged = null, CancellationToken cancellationToken = default)
        {
            Log.NamedPipeOpened(Logger);

            try
            {
                await RetryToSucceedAsync(CreateNamedPipeServerAsync, cancellationToken).ConfigureAwait(false);
                connectionStatusChanged?.Invoke(true);
                await ProcessAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Log.NamedPipeClosed(Logger);
                connectionStatusChanged?.Invoke(false);
            }
        }

        public override async Task ConnectAsync(string url, Action<bool> connectionStatusChanged = null, IDictionary<string, string> requestHeaders = null, CancellationToken cancellationToken = default)
        {
            Log.NamedPipeOpened(Logger);

            try
            {
                await RetryToSucceedAsync(CreateNamedPipeClientAsync, cancellationToken).ConfigureAwait(false);
                connectionStatusChanged?.Invoke(true);
                await ProcessAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connectionStatusChanged?.Invoke(false);
                Log.NamedPipeClosed(Logger);
            }
        }

        protected override async Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            try
            {
                var count = buffer.Length;

                var temp = new byte[count];
                var result = await _receiver.ReadAsync(temp, offset: 0, count, cancellationToken).ConfigureAwait(false);
                temp.CopyTo(buffer.Span);

                // NamedPipe disconnection not aloway throw InvalidOperationException
                return _receiver.IsConnected ? result : -1;
            }
            catch (InvalidOperationException)
            {
                // NamedPipe is disconnected
                return -1;
            }
        }

        protected override async Task SendAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var count = (int)buffer.Length;
            await _sender.WriteAsync(buffer.ToArray(), offset: 0, count, cancellationToken).ConfigureAwait(false);
        }

        protected override bool CanSend()
        {
            return _sender.CanWrite;
        }

        protected override Task CloseOutputAsync(Exception error, CancellationToken cancellationToken)
        {
            _sender.Close();
            return Task.CompletedTask;
        }

        protected override void Abort()
        {
            _receiver.Close();
            _sender.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _receiver?.Dispose();
                    _sender?.Dispose();
                }

                _disposedValue = true;
            }
        }

        private async Task RetryToSucceedAsync(Func<CancellationToken, Task> createNamedPipeFactory, CancellationToken cancellationToken)
        {
            _tryTimes = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                _tryTimes++;

                // happends when customers use app.UsedNamedPipe in their bot, but do not enable ase.
                if (_tryTimes > 100)
                {
                    return;
                }

                // To avoid NamedPipeServer and NamedPipeClient dead lock, add timeout when create NamePipe
                using (var source = new CancellationTokenSource())
                {
                    try
                    {
                        // NamedPipeServer and NamedPipeClient dead lock can be break by time out
                        // Add this random to avoid a special dead lock when machine run slow.
                        // At least 5 seconds, because :
                        // if ASE update to new version but customer still use old version bot(SDK). Quick retry will make this connection never connect.
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds((new Random(DateTime.Now.Millisecond).NextDouble() * _tryTimes * 2) + 5), cancellationToken);
                        var createTask = createNamedPipeFactory(source.Token);

                        if (timeoutTask == await Task.WhenAny(timeoutTask, createTask).ConfigureAwait(false))
                        {
                            Log.NamedPipeCreateTimeout(Logger);
                            source.Cancel();
                            continue;
                        }

                        // check createTask finish code. (Task.WhenAny can't catch exception in createTask)
                        await createTask.ConfigureAwait(false);

                        // connect succeed
                        return;
                    }
#pragma warning disable CA1031 // Catch all exceptions and retry
                    catch (Exception e)
#pragma warning restore CA1031
                    {
                        Log.NamedPipeCreateException(Logger, e);
                        source.Cancel();
                    }
                }
            }
        }

        private async Task CreateNamedPipeClientAsync(CancellationToken cancellationToken)
        {
            _sender?.Dispose();
            _receiver?.Dispose();

            _sender = new NamedPipeClientStream(
                serverName: ".",
                pipeName: _pipeName + ServerIncomingPath,
                PipeDirection.Out,
                options: System.IO.Pipes.PipeOptions.WriteThrough | System.IO.Pipes.PipeOptions.Asynchronous);
            await ((NamedPipeClientStream)_sender).ConnectAsync(cancellationToken).ConfigureAwait(false);

            _receiver = new NamedPipeClientStream(
                serverName: ".",
                pipeName: _pipeName + ServerOutgoingPath,
                PipeDirection.In,
                options: System.IO.Pipes.PipeOptions.WriteThrough | System.IO.Pipes.PipeOptions.Asynchronous);
            await ((NamedPipeClientStream)_receiver).ConnectAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task CreateNamedPipeServerAsync(CancellationToken cancellationToken)
        {
            _sender?.Dispose();
            _receiver?.Dispose();

            _receiver = new NamedPipeServerStream(
                pipeName: _pipeName + ServerIncomingPath,
                PipeDirection.In,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                options: System.IO.Pipes.PipeOptions.WriteThrough | System.IO.Pipes.PipeOptions.Asynchronous);
            await ((NamedPipeServerStream)_receiver).WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

            _sender = new NamedPipeServerStream(
                pipeName: _pipeName + ServerOutgoingPath,
                PipeDirection.Out,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                options: System.IO.Pipes.PipeOptions.WriteThrough | System.IO.Pipes.PipeOptions.Asynchronous);
            await ((NamedPipeServerStream)_sender).WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Log messages for <see cref="NamedPipeTransport"/>.
        /// </summary>
        /// <remarks>
        /// Messages implemented using <see cref="LoggerMessage.Define(LogLevel, EventId, string)"/> to maximize performance.
        /// For more information, see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/loggermessage?view=aspnetcore-5.0.
        /// </remarks>
        private static class Log
        {
            private static readonly Action<ILogger, Exception> _namedPipeCreateTimeout = LoggerMessage.Define(
                LogLevel.Information, new EventId(1, nameof(NamedPipeOpened)), "Create NamedPipe Timeout.");

            private static readonly Action<ILogger, Exception> _namedPipeCreateException = (_, e) => LoggerMessage.Define(
                LogLevel.Information, new EventId(1, nameof(NamedPipeOpened)), $"Create NamedPipe with exception:{e}");

            private static readonly Action<ILogger, Exception> _namedPipeOpened = LoggerMessage.Define(
                LogLevel.Information, new EventId(1, nameof(NamedPipeOpened)), "Named Pipe transport connection opened.");

            private static readonly Action<ILogger, Exception> _namedPipeClosed = LoggerMessage.Define(
                LogLevel.Information, new EventId(2, nameof(NamedPipeClosed)), "Named Pipe transport connection closed.");

            public static void NamedPipeCreateException(ILogger logger, Exception e) => _namedPipeCreateException(logger, e);

            public static void NamedPipeCreateTimeout(ILogger logger) => _namedPipeCreateTimeout(logger, null);

            public static void NamedPipeOpened(ILogger logger) => _namedPipeOpened(logger, null);

            public static void NamedPipeClosed(ILogger logger) => _namedPipeClosed(logger, null);
        }
    }
}
