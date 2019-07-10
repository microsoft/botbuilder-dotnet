namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class MpNews
    {
        public string Title { get; set; }

        public string ThumbMediaId { get; set; }

        public string Author { get; set; }

        public string ContentSourceUrl { get; set; }

        /// <summary>
        /// Gets or sets Content.
        /// </summary>
        /// <value>
        /// The content of the MpNews, can contain the html tag.
        /// </value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets Digest.
        /// </summary>
        /// <value>
        /// News description.
        /// </value>
        public string Digest { get; set; }

        /// <summary>
        /// Gets or sets ShowCoverPic.
        /// </summary>
        /// <value>
        /// If show the cover image, Can only be 1 or 0.
        /// </value>
        public string ShowCoverPic { get; set; }
    }
}
