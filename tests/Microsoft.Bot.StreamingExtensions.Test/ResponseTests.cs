// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Bot.StreamingExtensions.Payloads;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests
{
    [TestClass]
    public class ResponseTests
    {
        [TestMethod]
        public void ReceiveResponse_Streams_Zero()
        {
            var r = new ReceiveResponse();
            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(0, r.Streams.Count);
        }

        [TestMethod]
        public void ReceiveResponse_NullProperties()
        {
            var r = new ReceiveResponse();
            Assert.AreEqual(0, r.StatusCode);
        }

        [TestMethod]
        public void Response_NullProperties()
        {
            var r = new StreamingResponse();
            Assert.AreEqual(0, r.StatusCode);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Response_AddStream_Null_Throws()
        {
            var r = new StreamingResponse();

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                r.AddStream(null);
            });
        }

        [TestMethod]
        public void Response_AddStream_Success()
        {
            var r = new StreamingResponse();
            var s = new StringContent("hi");

            r.AddStream(s);

            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(s, r.Streams[0].Content);
        }

        [TestMethod]
        public void Response_AddStream_ExistingList_Success()
        {
            var r = new StreamingResponse();
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
        public void Response_NotFound_Success()
        {
            var r = StreamingResponse.NotFound();

            Assert.AreEqual((int)HttpStatusCode.NotFound, r.StatusCode);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Response_Forbidden_Success()
        {
            var r = StreamingResponse.Forbidden();

            Assert.AreEqual((int)HttpStatusCode.Forbidden, r.StatusCode);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Response_OK_Success()
        {
            var r = StreamingResponse.OK();

            Assert.AreEqual((int)HttpStatusCode.OK, r.StatusCode);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Response_InternalServerError_Success()
        {
            var r = StreamingResponse.InternalServerError();

            Assert.AreEqual((int)HttpStatusCode.InternalServerError, r.StatusCode);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Response_Create_WithBody_Success()
        {
            var s = new StringContent("hi");
            var r = StreamingResponse.CreateResponse(HttpStatusCode.OK, s);

            Assert.AreEqual((int)HttpStatusCode.OK, r.StatusCode);
            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(s, r.Streams[0].Content);
        }

        [TestMethod]
        public async Task ResponseExtensions_SetBodyString_Success()
        {
            var r = new StreamingResponse();
            r.SetBody("123");

            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(typeof(StringContent), r.Streams[0].Content.GetType());

            var s = await r.Streams[0].Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.AreEqual("123", s);
        }

        [TestMethod]
        public void ResponseExtensions_SetBodyString_Null_Does_Not_Throw()
        {
            var r = new StreamingResponse();
            Exception ex = null;

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
        public async Task ResponseExtensions_SetBody_Success()
        {
            var r = new StreamingResponse();
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
        public void ResponseExtensions_SetBody_Null_Does_Not_Throw()
        {
            var r = new StreamingResponse();
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

        [TestMethod]
        public void ReceiveBase_ReadBodyAsString_NoContent_EmptyString()
        {
            var r = new ReceiveResponse();
            r.Streams = new List<IContentStream>();

            var result = r.ReadBodyAsString();

            Assert.AreEqual(string.Empty, result);
        }
    }
}
