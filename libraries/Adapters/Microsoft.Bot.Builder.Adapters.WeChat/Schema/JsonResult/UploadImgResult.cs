using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResult
{
    public class UploadImgResult : WeChatJsonResult
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
