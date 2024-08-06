// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Streaming.Payloads;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests.Payloads
{
    public class RequestManagerTests
    {
        [Fact]
        public void RequestManager_Ctor_EmptyDictionary()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var rm = new RequestManager(d);

            Assert.Empty(d);
        }

        [Fact]
        public async Task RequestManager_SignalResponse_ReturnsFalseWhenNoGuid()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var rm = new RequestManager(d);

            var r = await rm.SignalResponseAsync(Guid.NewGuid(), null);

            Assert.False(r);
        }

        [Fact]
        public async Task RequestManager_SignalResponse_ReturnsTrueWhenGuid()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var tcs = new TaskCompletionSource<ReceiveResponse>();
            d.TryAdd(g, tcs);
            var rm = new RequestManager(d);

            var r = await rm.SignalResponseAsync(g, null);

            Assert.True(r);
        }

        [Fact]
        public async Task RequestManager_SignalResponse_NullResponseIsOk()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var tcs = new TaskCompletionSource<ReceiveResponse>();
            d.TryAdd(g, tcs);
            var rm = new RequestManager(d);

            await rm.SignalResponseAsync(g, null);

            Assert.Null(await tcs.Task);
        }

        [Fact]
        public async Task RequestManager_SignalResponse_Response()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var tcs = new TaskCompletionSource<ReceiveResponse>();
            d.TryAdd(g, tcs);
            var rm = new RequestManager(d);

            var resp = new ReceiveResponse();
            await rm.SignalResponseAsync(g, resp);

            Assert.True(resp.Equals(await tcs.Task));
        }

        [Fact]
        public async Task RequestManager_GetResponse_ReturnsNullOnDuplicateCall()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var tcs = new TaskCompletionSource<ReceiveResponse>();
            d.TryAdd(g, tcs);
            var rm = new RequestManager(d);

            var r = await rm.GetResponseAsync(g, CancellationToken.None);

            Assert.Null(r);
        }

        [Fact]
        public async Task RequestManager_GetResponse_ReturnsResponseAsync()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var rm = new RequestManager(d);

            var resp = new ReceiveResponse();

            await Task.WhenAll(
                Task.Run(async () =>
                {
                    var r = await rm.GetResponseAsync(g, CancellationToken.None);
                    Assert.True(resp.Equals(r));
                }),
                Task.Run(() =>
                {
                    TaskCompletionSource<ReceiveResponse> value;
                    while (!d.TryGetValue(g, out value))
                    {
                        // Wait for a value;
                    }

                    value.SetResult(resp);
                }));
        }

        [Fact]
        public async Task RequestManager_GetResponse_ReturnsNullResponseAsync()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var rm = new RequestManager(d);

            await Task.WhenAll(
                Task.Run(async () =>
                {
                    var r = await rm.GetResponseAsync(g, CancellationToken.None);
                    Assert.Null(r);
                }),
                Task.Run(() =>
                {
                    TaskCompletionSource<ReceiveResponse> value;
                    while (!d.TryGetValue(g, out value))
                    {
                        // Wait for a value.;
                    }

                    value.SetResult(null);
                }));
        }

        [Fact]
        public async Task RequestManager_GetResponse_ThrowsOnCancelledTaskAsync()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var rm = new RequestManager(d);

            await Task.WhenAll(
                Task.Run(async () =>
                {
                    await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                    {
                        var r = await rm.GetResponseAsync(g, CancellationToken.None);
                    });
                }),
                Task.Run(() =>
                {
                    TaskCompletionSource<ReceiveResponse> value;
                    while (!d.TryGetValue(g, out value))
                    {
                        // Wait for a value.;
                    }

                    value.SetCanceled();
                }));
        }

        [Fact]
        public async Task RequestManager_GetResponse_ThrowsOnErrorTaskAsync()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var rm = new RequestManager(d);

            await Task.WhenAll(
                Task.Run(async () =>
                {
                    await Assert.ThrowsAsync<AggregateException>(async () =>
                    {
                        var r = await rm.GetResponseAsync(g, CancellationToken.None);
                    });
                }),
                Task.Run(() =>
                {
                    TaskCompletionSource<ReceiveResponse> value;
                    while (!d.TryGetValue(g, out value))
                    {
                        // Wait for a value.;
                    }

                    value.SetException(new AggregateException(new InvalidOperationException()));
                }));
        }

        [Fact]
        public async Task RequestManager_GetResponse_ThrowsOnTimeout()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var rm = new RequestManager(d);

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                using (var cs = new CancellationTokenSource(100))
                {
                    var r = await rm.GetResponseAsync(g, cs.Token);
                }
            });
        }

        [Fact]
        public void RequestManager_RejectAllResponses_RejectsAllRequests()
        {
            var exception = new Exception("Disconnected");
            var task1 = new TaskCompletionSource<ReceiveResponse>();
            var task2 = new TaskCompletionSource<ReceiveResponse>();
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            d.TryAdd(Guid.NewGuid(), task1);
            d.TryAdd(Guid.NewGuid(), task2);
            var rm = new RequestManager(d);

            rm.RejectAllResponses(exception);

            Assert.Equal(exception, task1.Task.Exception.InnerException);
            Assert.Equal(exception, task2.Task.Exception.InnerException);
            Assert.Empty(d);
        }
    }
}
