// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests.Payloads
{
    [TestClass]
    public class RequestManagerTests
    {
        [TestMethod]
        public void RequestManager_Ctor_EmptyDictionary()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var rm = new RequestManager(d);

            Assert.AreEqual(0, d.Count);
        }

        [TestMethod]
        public async Task RequestManager_SignalResponse_ReturnsFalseWhenNoGuid()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var rm = new RequestManager(d);

            var r = await rm.SignalResponse(Guid.NewGuid(), null);

            Assert.IsFalse(r);
        }

        [TestMethod]
        public async Task RequestManager_SignalResponse_ReturnsTrueWhenGuid()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var tcs = new TaskCompletionSource<ReceiveResponse>();
            d.TryAdd(g, tcs);
            var rm = new RequestManager(d);

            var r = await rm.SignalResponse(g, null);

            Assert.IsTrue(r);
        }

        [TestMethod]
        public async Task RequestManager_SignalResponse_NullResponseIsOk()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var tcs = new TaskCompletionSource<ReceiveResponse>();
            d.TryAdd(g, tcs);
            var rm = new RequestManager(d);

            var r = await rm.SignalResponse(g, null);

            Assert.IsNull(tcs.Task.Result);
        }

        [TestMethod]
        public async Task RequestManager_SignalResponse_Response()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var tcs = new TaskCompletionSource<ReceiveResponse>();
            d.TryAdd(g, tcs);
            var rm = new RequestManager(d);

            var resp = new ReceiveResponse();
            var r = await rm.SignalResponse(g, resp);

            Assert.IsTrue(resp.Equals(tcs.Task.Result));
        }

        [TestMethod]
        public async Task RequestManager_GetResponse_ReturnsNullOnDuplicateCall()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var tcs = new TaskCompletionSource<ReceiveResponse>();
            d.TryAdd(g, tcs);
            var rm = new RequestManager(d);

            var r = await rm.GetResponseAsync(g, CancellationToken.None);

            Assert.IsNull(r);
        }

        [TestMethod]
        public void RequestManager_GetResponse_ReturnsResponse()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var rm = new RequestManager(d);

            var resp = new ReceiveResponse();

            Task.WaitAll(
                Task.Run(async () =>
                {
                    var r = await rm.GetResponseAsync(g, CancellationToken.None);
                    Assert.IsTrue(resp.Equals(r));
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

        [TestMethod]
        public void RequestManager_GetResponse_ReturnsNullResponse()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var rm = new RequestManager(d);

            Task.WaitAll(
                Task.Run(async () =>
                {
                    var r = await rm.GetResponseAsync(g, CancellationToken.None);
                    Assert.IsNull(r);
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

        [TestMethod]
        public void RequestManager_GetResponse_ThrowsOnCancelledTask()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var rm = new RequestManager(d);

            Task.WaitAll(
                Task.Run(async () =>
                {
                    await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
                    {
                        var r = await rm.GetResponseAsync(g, CancellationToken.None);
                        Assert.Fail();
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

        [TestMethod]
        public void RequestManager_GetResponse_ThrowsOnErrorTask()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var rm = new RequestManager(d);

            Task.WaitAll(
                Task.Run(() =>
                {
                    Assert.ThrowsExceptionAsync<AggregateException>(async () =>
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

                    value.SetException(new InvalidOperationException());
                }));
        }

        [TestMethod]
        public async Task RequestManager_GetResponse_ThrowsOnTimeout()
        {
            var d = new ConcurrentDictionary<Guid, TaskCompletionSource<ReceiveResponse>>();
            var g = Guid.NewGuid();
            var rm = new RequestManager(d);

            await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
            {
                using (var cs = new CancellationTokenSource(100))
                {
                    var r = await rm.GetResponseAsync(g, cs.Token);
                }
            });
        }
    }
}
