namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class Video
    {
        public Video(string mediaId, string title = null, string description = null)
        {
            MediaId = mediaId;
            Title = title;
            Description = description;
        }

        public string MediaId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
