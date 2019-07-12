using System;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class WeChatAccessToken
    {
        public WeChatAccessToken(string appId, string secret)
        {
            this.AppId = appId;
            this.Secret = secret;
            this.ExpireTime = DateTimeOffset.MinValue;
        }

        public string AppId { get; set; }

        public string Token { get; set; }

        public string Secret { get; set; }

        public DateTimeOffset ExpireTime { get; set; }
    }
}
