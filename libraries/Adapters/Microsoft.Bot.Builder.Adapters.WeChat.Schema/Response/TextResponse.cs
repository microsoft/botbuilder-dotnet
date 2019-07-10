namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    public class TextResponse : ResponseMessage
    {
        public override ResponseMessageType MsgType => ResponseMessageType.Text;

        public string Content { get; set; }
    }
}
