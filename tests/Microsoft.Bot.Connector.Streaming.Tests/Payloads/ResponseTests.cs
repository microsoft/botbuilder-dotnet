// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Streaming.Payloads;
using Microsoft.Bot.Schema;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Connector.Streaming.Tests.Payloads
{
    public class ResponseTests
    {
        [Fact]
        public void ReceiveResponse_Streams_Zero()
        {
            var r = new ReceiveResponse();
            Assert.NotNull(r.Streams);
            Assert.Empty(r.Streams);
        }

        [Fact]
        public void ReceiveResponse_NullProperties()
        {
            var r = new ReceiveResponse();
            Assert.Equal(0, r.StatusCode);
        }

        [Fact]
        public void Response_NullProperties()
        {
            var r = new StreamingResponse();
            Assert.Equal(0, r.StatusCode);
            Assert.Empty(r.Streams);
        }

        [Fact]
        public void Response_AddStream_Null_Throws()
        {
            var r = new StreamingResponse();

            Assert.Throws<ArgumentNullException>(() =>
            {
                r.AddStream(null);
            });
        }

        [Fact]
        public void Response_AddStream_Success()
        {
            var r = new StreamingResponse();
            var s = new StringContent("hi");

            r.AddStream(s);

            Assert.NotNull(r.Streams);
            Assert.Single(r.Streams);
            Assert.Equal(s, r.Streams[0].Content);
        }

        [Fact]
        public void Response_AddStream_ExistingList_Success()
        {
            var r = new StreamingResponse();
            var s = new StringContent("hi");
            var s2 = new StringContent("hello");

            r.Streams.AddRange(new List<ResponseMessageStream> { new ResponseMessageStream { Content = s2 } });

            r.AddStream(s);

            Assert.NotNull(r.Streams);
            Assert.Equal(2, r.Streams.Count);
            Assert.Equal(s2, r.Streams[0].Content);
            Assert.Equal(s, r.Streams[1].Content);
        }

        [Fact]
        public void Response_NotFound_Success()
        {
            var r = StreamingResponse.NotFound();

            Assert.Equal((int)HttpStatusCode.NotFound, r.StatusCode);
            Assert.Empty(r.Streams);
        }

        [Fact]
        public void Response_Forbidden_Success()
        {
            var r = StreamingResponse.Forbidden();

            Assert.Equal((int)HttpStatusCode.Forbidden, r.StatusCode);
            Assert.Empty(r.Streams);
        }

        [Fact]
        public void Response_OK_Success()
        {
            var r = StreamingResponse.OK();

            Assert.Equal((int)HttpStatusCode.OK, r.StatusCode);
            Assert.Empty(r.Streams);
        }

        [Fact]
        public void Response_InternalServerError_Success()
        {
            var r = StreamingResponse.InternalServerError();

            Assert.Equal((int)HttpStatusCode.InternalServerError, r.StatusCode);
            Assert.Empty(r.Streams);
        }

        [Fact]
        public void Response_Create_WithBody_Success()
        {
            var s = new StringContent("hi");
            var r = StreamingResponse.CreateResponse(HttpStatusCode.OK, s);

            Assert.Equal((int)HttpStatusCode.OK, r.StatusCode);
            Assert.NotNull(r.Streams);
            Assert.Single(r.Streams);
            Assert.Equal(s, r.Streams[0].Content);
        }

        [Fact]
        public async Task ResponseExtensions_SetBodyString_Success()
        {
            var r = new StreamingResponse();
            r.SetBody("123");

            Assert.NotNull(r.Streams);
            Assert.Single(r.Streams);
            Assert.Equal(typeof(StringContent), r.Streams[0].Content.GetType());

            var s = await r.Streams[0].Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Equal("123", s);
        }

        [Fact]
        public void ResponseExtensions_SetBodyString_Null_Does_Not_Throw()
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
                Assert.Null(ex);
            }
        }

        [Fact]
        public void ResponseExtensions_SetBodyObject_Null_Does_Not_Throw()
        {
            var r = new StreamingResponse();
            Exception ex = null;

            try
            {
                r.SetBody(null as object);
            }
            catch (Exception caughtEx)
            {
                ex = caughtEx;
            }
            finally
            {
                Assert.Null(ex);
            }
        }

        [Fact]
        public async Task ResponseExtensions_SetBody_Success()
        {
            var r = new StreamingResponse();
            var a = new Activity { Text = "hi", Type = "message" };
            r.SetBody(a);

            Assert.NotNull(r.Streams);
            Assert.Single(r.Streams);
            Assert.Equal(typeof(StringContent), r.Streams[0].Content.GetType());

            var s = JsonConvert.DeserializeObject<Activity>(await r.Streams[0].Content.ReadAsStringAsync().ConfigureAwait(false));
            Assert.Equal(a.Text, s.Text);
            Assert.Equal(a.Type, s.Type);
        }

        [Fact]
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
                Assert.Null(ex);
            }
        }

        [Fact]
        public void ReceiveBase_ReadBodyAsString_NoContent_EmptyString()
        {
            var r = new ReceiveResponse();

            var result = r.ReadBodyAsString();

            Assert.Equal(string.Empty, result);
        }

        [Fact]

        public void ReceiveExtensions_ReadBodyAsJson_Streams()
        {
            var activity = new Activity { Type = ActivityTypes.Message };
            var stringInput = JsonConvert.SerializeObject(activity);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(stringInput));
            var mockContentStream = new Mock<IContentStream>();
            mockContentStream.Setup(e => e.Stream).Returns(stream);

            var response = new ReceiveResponse();
            response.Streams.AddRange(new List<IContentStream> { mockContentStream.Object });

            var result = response.ReadBodyAsJson<Activity>();

            Assert.NotNull(result);
            Assert.Equal(activity.Type, result.Type);
        }

        [Fact]

        public void ReceiveExtensions_ReadBodyAsJson_Streams_Zero()
        {
            var response = new ReceiveResponse
            {
                StatusCode = 3,
            };
            var result = response.ReadBodyAsJson<dynamic>();

            Assert.Null(result);
            Assert.Equal(3, response.StatusCode);
        }

        [Fact]

        public void ReceiveExtensions_ReadBodyAsString_Streams()
        {
            const string stringInput = "message";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(stringInput));
            var mockContentStream = new Mock<IContentStream>();
            mockContentStream.Setup(e => e.Stream).Returns(stream);

            var response = new ReceiveResponse();
            response.Streams.AddRange(new List<IContentStream> { mockContentStream.Object });

            var result = response.ReadBodyAsString();

            Assert.NotNull(result);
            Assert.Equal(stringInput, result);
        }
    }
}
