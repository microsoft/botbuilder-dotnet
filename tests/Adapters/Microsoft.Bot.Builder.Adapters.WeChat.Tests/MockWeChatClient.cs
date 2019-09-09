// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResults;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Tests
{
    internal class MockWeChatClient : WeChatClient
    {
        public MockWeChatClient(
            WeChatSettings settings,
            IStorage storage,
            ILogger logger = null)
            : base(settings, storage, logger)
        {
        }

        public override Task<byte[]> SendHttpRequestAsync(HttpMethod method, string url, object data = null, string token = null, int timeout = 10000)
        {
            var result = JsonConvert.SerializeObject(MockDataUtility.WeChatJsonResult);
            var byteResult = Encoding.UTF8.GetBytes(result);

            if (url.Contains("cgi-bin/token"))
            {
                var tokenResult = new AccessTokenResult()
                {
                    ExpireIn = 7200,
                    Token = "testToken",
                };
                result = JsonConvert.SerializeObject(tokenResult);
                byteResult = Encoding.UTF8.GetBytes(result);
            }
            else if (url.Contains("upload?access_token"))
            {
                result = JsonConvert.SerializeObject(MockDataUtility.MockTempMediaResult(MediaTypes.Image));
                byteResult = Encoding.UTF8.GetBytes(result);
            }
            else if (url.Contains("add_material"))
            {
                result = JsonConvert.SerializeObject(MockDataUtility.MockForeverMediaResult("foreverMedia"));
                byteResult = Encoding.UTF8.GetBytes(result);
            }
            else if (url.Contains("uploadnews"))
            {
                result = JsonConvert.SerializeObject(MockDataUtility.MockTempMediaResult(MediaTypes.News));
                byteResult = Encoding.UTF8.GetBytes(result);
            }
            else if (url.Contains("add_news"))
            {
                result = JsonConvert.SerializeObject(MockDataUtility.MockForeverMediaResult("foreverNews"));
                byteResult = Encoding.UTF8.GetBytes(result);
            }
            else if (url.Contains("uploadimg"))
            {
                result = JsonConvert.SerializeObject(MockDataUtility.MockForeverMediaResult("foreverImage"));
                byteResult = Encoding.UTF8.GetBytes(result);
            }

            return Task.FromResult(byteResult);
        }
    }
}
