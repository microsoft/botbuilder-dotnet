using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResult
{
    public class UploadTemporaryMediaResult : WeChatJsonResult
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("media_id")]
        public string MediaId { get; set; }

        [JsonProperty("thumb_media_id")]
        public string ThumbMediaId { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        // By wechat description temporary media will expired in 3 days
        public long ExpiredTime => CreatedAt + (3 * 24 * 60 * 60);
    }
}
