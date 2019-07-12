namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    public class MpNewsResponse : ResponseMessage
    {
        public MpNewsResponse(string mediaId)
        {
            MediaId = mediaId;
        }

        public override ResponseMessageType MsgType => ResponseMessageType.MpNews;

        public string MediaId { get; set; }
    }
}
