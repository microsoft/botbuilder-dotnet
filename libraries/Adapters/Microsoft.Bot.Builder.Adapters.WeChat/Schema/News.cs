// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

/// <summary>
/// Model for News message.
/// </summary>
namespace Microsoft.Bot.Builder.Adapters.WeChat.Schema
{
    public class News
    {
        /// <summary>
        /// Gets or sets ThumbMediaId.
        /// </summary>
        /// <value>
        /// Thumbnail image id.
        /// </value>
        [JsonProperty("thumb_media_id")]
        public string ThumbMediaId { get; set; }

        /// <summary>
        /// Gets or sets Author.
        /// </summary>
        /// <value>
        /// Author of the news.
        /// </value>
        [JsonProperty("author")]
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        /// <value>
        /// News title.
        /// </value>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets ContentSourceUrl.
        /// </summary>
        /// <value>
        /// Link to open when user click open original article.
        /// </value>
        [JsonProperty("content_source_url")]
        public string ContentSourceUrl { get; set; }

        /// <summary>
        /// Gets or sets Content.
        /// </summary>
        /// <value>
        /// News content, support HTML.
        /// </value>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        /// <value>
        /// News description.
        /// </value>
        [JsonProperty("digest")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets ShowCoverPicture.
        /// </summary>
        /// <value>
        /// Show cover picture in news detail, 1 is ture, 0 is false.
        /// Must be a string.
        /// </value>
        [JsonProperty("show_cover_pic")]
        public string ShowCoverPicture { get; set; }

        /// <summary>
        /// Gets or sets ThumbUrl.
        /// </summary>
        /// <value>
        /// Thumbnail image url.
        /// </value>
        [JsonProperty("thumb_url")]
        public string ThumbUrl { get; set; }

        /// <summary>
        /// Gets or sets NeedOpenComment.
        /// </summary>
        /// <value>
        /// Flag if open comment for news, 1 is true, 0 is false.
        /// </value>
        [JsonProperty("need_open_comment")]
        public int NeedOpenComment { get; set; }

        /// <summary>
        /// Gets or sets OnlyFansCanComment.
        /// </summary>
        /// <value>
        /// Flag only fans can comment, 1 is true, 0 is false.
        /// </value>
        [JsonProperty("only_fans_can_comment")]
        public int OnlyFansCanComment { get; set; }
    }
}
