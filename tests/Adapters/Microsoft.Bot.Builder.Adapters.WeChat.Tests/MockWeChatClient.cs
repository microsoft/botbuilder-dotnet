// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResults;
using Microsoft.Bot.Builder.Adapters.WeChat.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Tests
{
    public class MockWeChatClient : WeChatClient
    {
        public MockWeChatClient(
            string appId,
            string appSecret,
            IStorage storage,
            ILogger logger = null,
            IAttachmentHash attachmentHash = null)
            : base(appId, appSecret, storage, logger, attachmentHash)
        {
        }

        public override Task<byte[]> SendHttpRequestAsync(HttpMethod method, string url, object data = null, string token = null, int timeout = 10000)
        {
            if (url.Contains("token"))
            {
                var tokenResult = new AccessTokenResult()
                {
                    ExpireIn = 7200,
                    Token = "testToken",
                };
                var result = JsonConvert.SerializeObject(tokenResult);
                var byteResult = Encoding.UTF8.GetBytes(result);
                return Task.FromResult(byteResult);
            }
            else if (url.Contains("media"))
            {
                var result = JsonConvert.SerializeObject(MockDataUtility.MockTempMediaResult(Schema.MediaTypes.Image));
                var byteResult = Encoding.UTF8.GetBytes(result);
                return Task.FromResult(byteResult);
            }
            else if (url.Contains("news"))
            {
                var result = JsonConvert.SerializeObject(MockDataUtility.MockTempMediaResult(Schema.MediaTypes.News));
                var byteResult = Encoding.UTF8.GetBytes(result);
                return Task.FromResult(byteResult);
            }
            else
            {
                var result = JsonConvert.SerializeObject(MockDataUtility.WeChatJsonResult);
                var byteResult = Encoding.UTF8.GetBytes(result);
                return Task.FromResult(byteResult);
            }
        }
    }
}
