using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Test.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Test
{
    [TestClass]
    public class WeChatClientTest
    {
        private readonly WeChatClient testClient = MockDataUtility.MockWeChatClient();
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

        /// <summary>
        /// Text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task SendTextAsyncTest()
        {
            var result = await testClient.SendTextAsync(openId, content).ConfigureAwait(false);
            Assert.AreEqual(0, result.ErrorCode);
            result = await testClient.SendTextAsync(openId, content, customerServiceAccount: "test").ConfigureAwait(false);
            Assert.AreEqual(0, result.ErrorCode);
        }

        /// <summary>
        /// Image.
        /// </summary>
        [TestMethod]
        public void SendImageAsyncTest()
        {
            var result = testClient.SendImageAsync(openId, mediaId).Result;
            Assert.AreEqual(0, result.ErrorCode);
            result = testClient.SendImageAsync(openId, mediaId, customerServiceAccount: "test").Result;
            Assert.AreEqual(0, result.ErrorCode);
        }

        /// <summary>
        /// Music.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task SendMusicAsyncTest()
        {
            var result = await testClient.SendMusicAsync(openId, title, description, musicUrl, highQualityMusicUrl, thumbMediaId);
            Assert.AreEqual(0, result.ErrorCode);
            result = await testClient.SendMusicAsync(openId, title, description, musicUrl, highQualityMusicUrl, thumbMediaId, customerServiceAccount: "test");
            Assert.AreEqual(0, result.ErrorCode);
        }

        /// <summary>
        /// Video.
        /// </summary>
        [TestMethod]
        public void SendVideoAsyncTest()
        {
            var result = testClient.SendVideoAsync(openId, mediaId, title, description).Result;
            Assert.AreEqual(0, result.ErrorCode);
            result = testClient.SendVideoAsync(openId, mediaId, title, description, customerServiceAccount: "test").Result;
            Assert.AreEqual(0, result.ErrorCode);
        }

        /// <summary>
        /// Voice.
        /// </summary>
        [TestMethod]
        public void SendVoiceAsyncTest()
        {
            var result = testClient.SendVoiceAsync(openId, mediaId).Result;
            Assert.AreEqual(0, result.ErrorCode);
            result = testClient.SendVoiceAsync(openId, mediaId, customerServiceAccount: "test").Result;
            Assert.AreEqual(0, result.ErrorCode);
        }

        /// <summary>
        /// News.
        /// </summary>
        [TestMethod]
        public void SendNewsAsyncTest()
        {
            var result = testClient.SendNewsAsync(openId, articles).Result;
            Assert.AreEqual(0, result.ErrorCode);
            result = testClient.SendNewsAsync(openId, articles, customerServiceAccount: "test").Result;
            Assert.AreEqual(0, result.ErrorCode);
        }

        /// <summary>
        /// MPNews.
        /// </summary>
        [TestMethod]
        public void SendMPNewsAsyncTest()
        {
            var result = testClient.SendMpNewsAsync(openId, mediaId).Result;
            Assert.AreEqual(0, result.ErrorCode);
            result = testClient.SendMpNewsAsync(openId, mediaId, customerServiceAccount: "test").Result;
            Assert.AreEqual(0, result.ErrorCode);
        }
    }
}
