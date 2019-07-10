namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class Article
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// Gets or sets PicUrl.
        /// </summary>
        /// <value>
        /// Should be JPG or PNG type.
        /// </value>
        public string PicUrl { get; set; }
    }
}
