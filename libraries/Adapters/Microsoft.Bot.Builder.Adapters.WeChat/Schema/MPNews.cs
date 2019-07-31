namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class MPNews
    {
        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        /// <value>
        /// The title of the MPNews.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets ThumbMediaId.
        /// </summary>
        /// <value>
        /// The thumbnail image media id of the MPNews.
        /// </value>
        public string ThumbMediaId { get; set; }

        /// <summary>
        /// Gets or sets Author.
        /// </summary>
        /// <value>
        /// The author of the MPNews.
        /// </value>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets ContentSourceUrl.
        /// </summary>
        /// <value>
        /// The content source of the MPNews.
        /// </value>
        public string ContentSourceUrl { get; set; }

        /// <summary>
        /// Gets or sets Content.
        /// </summary>
        /// <value>
        /// The content of the MPNews, can contain the html tag.
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
