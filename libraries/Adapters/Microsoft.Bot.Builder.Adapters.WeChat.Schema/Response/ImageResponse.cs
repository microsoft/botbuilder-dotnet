namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema.Response
{
    public class ImageResponse : ResponseMessage
    {
        public ImageResponse(Image image)
        {
            Image = image;
        }

        public ImageResponse(string mediaId)
        {
            Image = new Image(mediaId);
        }

        public override ResponseMessageType MsgType => ResponseMessageType.Image;

        public Image Image { get; set; }
    }
}
