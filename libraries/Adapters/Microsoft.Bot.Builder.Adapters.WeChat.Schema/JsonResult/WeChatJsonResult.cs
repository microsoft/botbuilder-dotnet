using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResult
{
    public class WeChatJsonResult : JsonResultBase
    {
        [JsonProperty("errcode")]
        public int ErrorCode { get; set; }

        public override int ErrorCodeValue
        {
            get { return ErrorCode; }
        }

        public override string ToString()
        {
            return string.Format("WeChatJsonResult：{{ErrorCode:'{0}',ErrorCode_name:'{1}',ErrorMessage:'{2}'}}", ErrorCode, ErrorCode.ToString(), ErrorMessage);
        }
    }
}
