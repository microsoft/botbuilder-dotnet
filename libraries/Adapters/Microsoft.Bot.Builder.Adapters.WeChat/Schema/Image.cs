namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class Image
    {
        public Image()
        {
        }

        public Image(string mediaId)
        {
            MediaId = mediaId;
        }

        public string MediaId { get; set; }
    }
}
