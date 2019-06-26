// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests
{
    [TestClass]
    public class RequestTests
    {
        [TestMethod]
        public void ReceiveRequest_ctor_NullStreams()
        {
            var r = new ReceiveRequest();
            Assert.IsNull(r.Streams);
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
            var r = new Request();
            Assert.IsNull(r.Verb);
            Assert.IsNull(r.Path);
        }

        [TestMethod]
        public void Request_AddStream_Null_Throws()
        {
            var r = new Request();

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                r.AddStream(null);
            });
        }

        [TestMethod]
        public void Request_AddStream_Success()
        {
            var r = new Request();
            var s = new StringContent("hi");

            r.AddStream(s);

            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(s, r.Streams[0].Content);
        }

        [TestMethod]
        public void Request_AddStream_ExistingList_Success()
        {
            var r = new Request();
            var s = new StringContent("hi");
            var s2 = new StringContent("hello");

            r.Streams = new List<HttpContentStream> { new HttpContentStream() { Content = s2 } };

            r.AddStream(s);

            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(2, r.Streams.Count);
            Assert.AreEqual(s2, r.Streams[0].Content);
            Assert.AreEqual(s, r.Streams[1].Content);
        }

        [TestMethod]
        public void Request_Create_Get_Success()
        {
            var r = Request.CreateGet();

            Assert.AreEqual(Request.GET, r.Verb);
            Assert.IsNull(r.Path);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Request_Create_Post_Success()
        {
            var r = Request.CreatePost();

            Assert.AreEqual(Request.POST, r.Verb);
            Assert.IsNull(r.Path);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Request_Create_Delete_Success()
        {
            var r = Request.CreateDelete();

            Assert.AreEqual(Request.DELETE, r.Verb);
            Assert.IsNull(r.Path);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Request_Create_Put_Success()
        {
            var r = Request.CreatePut();

            Assert.AreEqual(Request.PUT, r.Verb);
            Assert.IsNull(r.Path);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Request_Create_WithBody_Success()
        {
            var s = new StringContent("hi");
            var r = Request.CreateRequest(Request.POST, "123", s);

            Assert.AreEqual(Request.POST, r.Verb);
            Assert.AreEqual("123", r.Path);
            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(s, r.Streams[0].Content);
        }

        [TestMethod]
        public async Task RequestExtensions_SetBodyString_Success()
        {
            var r = new Request();
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

            var r = new Request();
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
            var r = new Request();
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
            var r = new Request();
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
