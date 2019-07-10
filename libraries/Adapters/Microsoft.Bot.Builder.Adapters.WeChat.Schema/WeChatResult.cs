using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class WeChatResult
    {
        /// <summary>
        /// Gets or Sets error message.
        /// </summary>
        /// <value>
        /// Error code will be "ok" if no error occur.
        /// </value>
        [JsonProperty("errmsg")]
        public virtual string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets error code.
        /// </summary>
        /// <value>
        /// Error code defined by wechat, return 0 if successed.
        /// </value>
        [JsonProperty("errcode")]
        public int ErrorCode { get; set; }
    }
}
