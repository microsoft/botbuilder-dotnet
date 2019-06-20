using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.StreamingExtensions.UnitTests
{
    [TestClass]
    public class ResponseTests
    {
        #region ReceiveResponse

        [TestMethod]
        public void ReceiveResponse_Streams_Zero()
        {
            var r = new ReceiveResponse();
            Assert.IsNull(r.Streams);
        }
     

        [TestMethod]
        public void ReceiveResponse_NullProperties()
        {
            var r = new ReceiveResponse();
            Assert.AreEqual(0, r.StatusCode);
        }

        #endregion

        #region Response

        [TestMethod]
        public void Response_NullProperties()
        {
            var r = new Response();
            Assert.AreEqual(0, r.StatusCode);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Response_AddStream_Null_Throws()
        {
            var r = new Response();

            Assert.ThrowsException<ArgumentNullException>(() => {
                r.AddStream(null);
            });
        }

        [TestMethod]
        public void Response_AddStream_Success()
        {
            var r = new Response();
            var s = new StringContent("hi");

            r.AddStream(s);

            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(s, r.Streams[0].Content);
        }

        [TestMethod]
        public void Response_AddStream_ExistingList_Success()
        {
            var r = new Response();
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
        public void Response_NotFound_Success()
        {
            var r = Response.NotFound();

            Assert.AreEqual((int)HttpStatusCode.NotFound, r.StatusCode);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Response_Forbidden_Success()
        {
            var r = Response.Forbidden();

            Assert.AreEqual((int)HttpStatusCode.Forbidden, r.StatusCode);
            Assert.IsNull(r.Streams);
        }
        
        [TestMethod]
        public void Response_OK_Success()
        {
            var r = Response.OK();

            Assert.AreEqual((int)HttpStatusCode.OK, r.StatusCode);
            Assert.IsNull(r.Streams);
        }
        
        [TestMethod]
        public void Response_InternalServerError_Success()
        {
            var r = Response.InternalServerError();

            Assert.AreEqual((int)HttpStatusCode.InternalServerError, r.StatusCode);
            Assert.IsNull(r.Streams);
        }

        [TestMethod]
        public void Response_Create_WithBody_Success()
        {
            var s = new StringContent("hi");
            var r = Response.CreateResponse(HttpStatusCode.OK, s);
            
            Assert.AreEqual((int)HttpStatusCode.OK, r.StatusCode);
            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(s, r.Streams[0].Content);
        }

        #endregion

        #region ResponseExtensions
        
        [TestMethod]
        public async Task  ResponseExtensions_SetBodyString_Success()
        {
            var r = new Response();
            r.SetBody("123");

            Assert.IsNotNull(r.Streams);
            Assert.AreEqual(1, r.Streams.Count);
            Assert.AreEqual(typeof(StringContent), r.Streams[0].Content.GetType());

            var s = await r.Streams[0].Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.AreEqual("123", s);
        }

        [TestMethod]
        public void ResponseExtensions_SetBodyString_Null_Throws()
        {
            var r = new Response();
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                r.SetBody((string)null);
            });
        }

        [TestMethod]
        public async Task ResponseExtensions_SetBody_Success()
        {
            var r = new Response();
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
        public void ResponseExtensions_SetBody_Null_Throws()
        {
            var r = new Response();
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                r.SetBody(null);
            });
        }

        [TestMethod]
        public void ReceiveBase_ReadBodyAsString_NoContent_Null()
        {
            var r = new ReceiveResponse();
            r.Streams = new List<IContentStream>();
           
            var result = r.ReadBodyAsString();

            Assert.IsNull(result);
        }
        
        #endregion
    }
}
