using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResult
{
    public abstract class JsonResultBase : IJsonResult
    {
        [JsonProperty("errmsg")]
        public virtual string ErrorMessage { get; set; }

        public abstract int ErrorCodeValue { get; }

        public virtual object P2PData { get; set; }
    }
}
