namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class Voice
    {
        public Voice(string mediaId)
        {
            MediaId = mediaId;
        }

        public string MediaId { get; set; }
    }
}
