using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResult
{
    public class UploadForeverMediaResult : WeChatJsonResult
    {
        [JsonProperty("media_id")]
        public string MediaId { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
