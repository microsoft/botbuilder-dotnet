// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests
{
    [TestClass]
    public class RequestTests
    {
        [TestMethod]
        public void ReceiveRequest_ctor_Empty_Streams()
        {
            var r = new ReceiveRequest();
            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(0, r.Streams.Count);
        }

        [TestMethod]
        public void ReceiveRequest_ctor_NullProperties()
        {
            var r = new ReceiveRequest();
            Assert.IsNull(r.Verb);
            Assert.IsNull(r.Path);
        }

        [TestMethod]
        public void Request_NullProperties()
        {
            var r = new StreamingRequest();
            Assert.IsNull(r.Verb);
            Assert.IsNull(r.Path);
        }

        [TestMethod]
        public void Request_AddStream_Null_Throws()
        {
            var r = new StreamingRequest();

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                r.AddStream(null);
            });
        }

        [TestMethod]
        public void Request_AddStream_Success()
        {
            var r = new StreamingRequest();
            var s = new StringContent("hi");

            r.AddStream(s);

            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(s, r.Streams[0].Content);
        }

        [TestMethod]
        public void Request_AddStream_ExistingList_Success()
        {
            var r = new StreamingRequest();
            var s = new StringContent("hi");
            var s2 = new StringContent("hello");

            r.Streams = new List<ResponseMessageStream> { new ResponseMessageStream() { Content = s2 } };

            r.AddStream(s);

            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(2, r.Streams.Count);
            Assert.AreEqual(s2, r.Streams[0].Content);
            Assert.AreEqual(s, r.Streams[1].Content);
        }

        [TestMethod]
        public void Request_Create_Get_Success()
        {
            var r = StreamingRequest.CreateGet();

            Assert.AreEqual(StreamingRequest.GET, r.Verb);
            Assert.IsNull(r.Path);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Request_Create_Post_Success()
        {
            var r = StreamingRequest.CreatePost();

            Assert.AreEqual(StreamingRequest.POST, r.Verb);
            Assert.IsNull(r.Path);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Request_Create_Delete_Success()
        {
            var r = StreamingRequest.CreateDelete();

            Assert.AreEqual(StreamingRequest.DELETE, r.Verb);
            Assert.IsNull(r.Path);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Request_Create_Put_Success()
        {
            var r = StreamingRequest.CreatePut();

            Assert.AreEqual(StreamingRequest.PUT, r.Verb);
            Assert.IsNull(r.Path);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Request_Create_WithBody_Success()
        {
            var s = new StringContent("hi");
            var r = StreamingRequest.CreateRequest(StreamingRequest.POST, "123", s);

            Assert.AreEqual(StreamingRequest.POST, r.Verb);
            Assert.AreEqual("123", r.Path);
            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(s, r.Streams[0].Content);
        }

        [TestMethod]
        public async Task RequestExtensions_SetBodyString_Success()
        {
            var r = new StreamingRequest();
            r.SetBody("123");

            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(typeof(StringContent), r.Streams[0].Content.GetType());

            var s = await r.Streams[0].Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.AreEqual("123", s);
        }

        [TestMethod]
        public void RequestExtensions_SetBodyString_Null_Does_Not_Throw()
        {
            Exception ex = null;

            var r = new StreamingRequest();
            try
            {
                r.SetBody((string)null);
            }
            catch (Exception caughtEx)
            {
                ex = caughtEx;
            }
            finally
            {
                Assert.AreEqual(ex, null);
            }
        }

        [TestMethod]
        public async Task RequestExtensions_SetBody_Success()
        {
            var r = new StreamingRequest();
            var a = new Activity() { Text = "hi", Type = "message" };
            r.SetBody(a);

            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(typeof(StringContent), r.Streams[0].Content.GetType());

            var s = await r.Streams[0].Content.ReadAsAsync<Activity>().ConfigureAwait(false);
            Assert.AreEqual(a.Text, s.Text);
            Assert.AreEqual(a.Type, s.Type);
        }

        [TestMethod]
        public void RequestExtensions_SetBody_Null_Does_Not_Throw()
        {
            var r = new StreamingRequest();
            Exception ex = null;

            try
            {
                r.SetBody(null);
            }
            catch (Exception caughtEx)
            {
                ex = caughtEx;
            }
            finally
            {
                Assert.AreEqual(ex, null);
            }
        }
    }
}
