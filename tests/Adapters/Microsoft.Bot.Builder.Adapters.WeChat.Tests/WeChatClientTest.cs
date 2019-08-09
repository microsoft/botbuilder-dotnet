// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Tests.TestUtilities;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Tests
{
    public class WeChatClientTest
    {
        private readonly WeChatClient testClient = MockDataUtility.GetMockWeChatClient();
        private readonly string openId = "testuser";
        private readonly string content = "test";
        private readonly string mediaId = string.Empty;
        private readonly string title = string.Empty;
        private readonly string description = string.Empty;
        private readonly string musicUrl = string.Empty;
        private readonly string highQualityMusicUrl = string.Empty;
        private readonly string thumbMediaId = string.Empty;
        private readonly List<Article> articles = new List<Article>()
        {
            new Article
            {
                Title = "title",
                Description = "Description",
                Url = "testUrl",
                PicUrl = "picUrl",
            },
        };

        [Fact]
        public async Task GetAccessTokenTest()
        {
            var storage = new MemoryStorage();
            var mockClient = new MockWeChatClient("wx77f941c869071d99", "secret", storage);
            await mockClient.SendHttpRequestAsync(HttpMethod.Get, "https://dev.botframework.com");
            var tokenResult = await mockClient.GetAccessTokenAsync();
            Assert.Equal("testToken", tokenResult);
        }

        [Fact]
        public async Task UploadMediaTest()
        {
            var storage = new MemoryStorage();
            var mockClient = new MockWeChatClient("wx77f941c869071d99", "secret", storage);
            var mockAttachemntData = MockDataUtility.GetMockAttachmentData();
            var result1 = await mockClient.UploadMediaAsync(mockAttachemntData, true, 10000);
            var result2 = await mockClient.UploadNewsAsync(new News[] { new News { Title = "test" } }, true);
            var result3 = await mockClient.UploadMediaAsync(mockAttachemntData, false, 10000);
            var result4 = await mockClient.UploadNewsAsync(new News[] { new News { Title = "test" } }, false);

            // Assert.Equal("testToken", result1.MediaId);
            // Assert.Equal("bmwipabotwx", result2.MediaId);
            // Assert.Equal("testToken", result3.MediaId);
            // Assert.Equal("bmwipabotwx", result4.MediaId);
        }

        /// <summary>
        /// Text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SendTextAsyncTest()
        {
            var result = await testClient.SendTextAsync(openId, content).ConfigureAwait(false);
            Assert.Equal(0, result.ErrorCode);
            result = await testClient.SendTextAsync(openId, content, customerServiceAccount: "test").ConfigureAwait(false);
            Assert.Equal(0, result.ErrorCode);
        }

        /// <summary>
        /// Image.
        /// </summary>
        [Fact]
        public void SendImageAsyncTest()
        {
            var result = testClient.SendImageAsync(openId, mediaId).Result;
            Assert.Equal(0, result.ErrorCode);
            result = testClient.SendImageAsync(openId, mediaId, customerServiceAccount: "test").Result;
            Assert.Equal(0, result.ErrorCode);
        }

        /// <summary>
        /// Music.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SendMusicAsyncTest()
        {
            var result = await testClient.SendMusicAsync(openId, title, description, musicUrl, highQualityMusicUrl, thumbMediaId);
            Assert.Equal(0, result.ErrorCode);
            result = await testClient.SendMusicAsync(openId, title, description, musicUrl, highQualityMusicUrl, thumbMediaId, customerServiceAccount: "test");
            Assert.Equal(0, result.ErrorCode);
        }

        /// <summary>
        /// Video.
        /// </summary>
        [Fact]
        public void SendVideoAsyncTest()
        {
            var result = testClient.SendVideoAsync(openId, mediaId, title, description).Result;
            Assert.Equal(0, result.ErrorCode);
            result = testClient.SendVideoAsync(openId, mediaId, title, description, customerServiceAccount: "test").Result;
            Assert.Equal(0, result.ErrorCode);
        }

        /// <summary>
        /// Voice.
        /// </summary>
        [Fact]
        public void SendVoiceAsyncTest()
        {
            var result = testClient.SendVoiceAsync(openId, mediaId).Result;
            Assert.Equal(0, result.ErrorCode);
            result = testClient.SendVoiceAsync(openId, mediaId, customerServiceAccount: "test").Result;
            Assert.Equal(0, result.ErrorCode);
        }

        /// <summary>
        /// News.
        /// </summary>
        [Fact]
        public void SendNewsAsyncTest()
        {
            var result = testClient.SendNewsAsync(openId, articles).Result;
            Assert.Equal(0, result.ErrorCode);
            result = testClient.SendNewsAsync(openId, articles, customerServiceAccount: "test").Result;
            Assert.Equal(0, result.ErrorCode);
        }

        /// <summary>
        /// MPNews.
        /// </summary>
        [Fact]
        public void SendMPNewsAsyncTest()
        {
            var result = testClient.SendMPNewsAsync(openId, mediaId).Result;
            Assert.Equal(0, result.ErrorCode);
            result = testClient.SendMPNewsAsync(openId, mediaId, customerServiceAccount: "test").Result;
            Assert.Equal(0, result.ErrorCode);
        }
    }
}
