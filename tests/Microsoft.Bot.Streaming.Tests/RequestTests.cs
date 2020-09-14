// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming.Payloads;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Streaming.UnitTests
{
    public class RequestTests
    {
        [Fact]
        public void ReceiveRequest_ctor_Empty_Streams()
        {
            var r = new ReceiveRequest();
            Assert.NotNull(r.Streams);
            Assert.Empty(r.Streams);
        }

        [Fact]
        public void ReceiveRequest_ctor_NullProperties()
        {
            var r = new ReceiveRequest();
            Assert.Null(r.Verb);
            Assert.Null(r.Path);
        }

        [Fact]
        public void Request_NullProperties()
        {
            var r = new StreamingRequest();
            Assert.Null(r.Verb);
            Assert.Null(r.Path);
        }

        [Fact]
        public void Request_AddStream_Null_Throws()
        {
            var r = new StreamingRequest();

            Assert.Throws<ArgumentNullException>(() =>
            {
                r.AddStream(null);
            });
        }

        [Fact]
        public void Request_AddStream_Success()
        {
            var r = new StreamingRequest();
            var s = new StringContent("hi");

            r.AddStream(s);

            Assert.NotNull(r.Streams);
            Assert.Single(r.Streams);
            Assert.Equal(s, r.Streams[0].Content);
        }

        [Fact]
        public void Request_AddStream_ExistingList_Success()
        {
            var r = new StreamingRequest();
            var s = new StringContent("hi");
            var s2 = new StringContent("hello");

            r.Streams = new List<ResponseMessageStream> { new ResponseMessageStream() { Content = s2 } };

            r.AddStream(s);

            Assert.NotNull(r.Streams);
            Assert.Equal(2, r.Streams.Count);
            Assert.Equal(s2, r.Streams[0].Content);
            Assert.Equal(s, r.Streams[1].Content);
        }

        [Fact]
        public void Request_Create_Get_Success()
        {
            var r = StreamingRequest.CreateGet();

            Assert.Equal(StreamingRequest.GET, r.Verb);
            Assert.Null(r.Path);
            Assert.Null(r.Streams);
        }

        [Fact]
        public void Request_Create_Post_Success()
        {
            var r = StreamingRequest.CreatePost();

            Assert.Equal(StreamingRequest.POST, r.Verb);
            Assert.Null(r.Path);
            Assert.Null(r.Streams);
        }

        [Fact]
        public void Request_Create_Delete_Success()
        {
            var r = StreamingRequest.CreateDelete();

            Assert.Equal(StreamingRequest.DELETE, r.Verb);
            Assert.Null(r.Path);
            Assert.Null(r.Streams);
        }

        [Fact]
        public void Request_Create_Put_Success()
        {
            var r = StreamingRequest.CreatePut();

            Assert.Equal(StreamingRequest.PUT, r.Verb);
            Assert.Null(r.Path);
            Assert.Null(r.Streams);
        }

        [Fact]
        public void Request_Create_WithBody_Success()
        {
            var s = new StringContent("hi");
            var r = StreamingRequest.CreateRequest(StreamingRequest.POST, "123", s);

            Assert.Equal(StreamingRequest.POST, r.Verb);
            Assert.Equal("123", r.Path);
            Assert.NotNull(r.Streams);
            Assert.Single(r.Streams);
            Assert.Equal(s, r.Streams[0].Content);
        }

        [Fact]
        public async Task RequestExtensions_SetBodyString_Success()
        {
            var r = new StreamingRequest();
            r.SetBody("123");

            Assert.NotNull(r.Streams);
            Assert.Single(r.Streams);
            Assert.Equal(typeof(StringContent), r.Streams[0].Content.GetType());

            var s = await r.Streams[0].Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Equal("123", s);
        }

        [Fact]
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
                Assert.Null(ex);
            }
        }

        [Fact]
        public async Task RequestExtensions_SetBody_Success()
        {
            var r = new StreamingRequest();
            var a = new Activity() { Text = "hi", Type = "message" };
            r.SetBody(a);

            Assert.NotNull(r.Streams);
            Assert.Single(r.Streams);
            Assert.Equal(typeof(StringContent), r.Streams[0].Content.GetType());

            var s = JsonConvert.DeserializeObject<Activity>(await r.Streams[0].Content.ReadAsStringAsync().ConfigureAwait(false));
            Assert.Equal(a.Text, s.Text);
            Assert.Equal(a.Type, s.Type);
        }

        [Fact]
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
                Assert.Null(ex);
            }
        }
    }
}
