// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Video card.
    /// </summary>
    public partial class VideoCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoCard"/> class.
        /// </summary>
        public VideoCard()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoCard"/> class.
        /// </summary>
        /// <param name="title">Title of this card.</param>
        /// <param name="subtitle">Subtitle of this card.</param>
        /// <param name="text">Text of this card.</param>
        /// <param name="image">Thumbnail placeholder.</param>
        /// <param name="media">Media URLs for this card. When this field
        /// contains more than one URL, each URL is an alternative format of
        /// the same content.</param>
        /// <param name="buttons">Actions on this card.</param>
        /// <param name="shareable">This content may be shared with others
        /// (default:true).</param>
        /// <param name="autoloop">Should the client loop playback at end of
        /// content (default:true).</param>
        /// <param name="autostart">Should the client automatically start
        /// playback of media in this card (default:true).</param>
        /// <param name="aspect">Aspect ratio of thumbnail/media placeholder.
        /// Allowed values are "16:9" and "4:3".</param>
        /// <param name="duration">Describes the length of the media content
        /// without requiring a receiver to open the content. Formatted as an
        /// ISO 8601 Duration field.</param>
        /// <param name="value">Supplementary parameter for this card.</param>
        public VideoCard(string title = default, string subtitle = default, string text = default, ThumbnailUrl image = default, IList<MediaUrl> media = default, IList<CardAction> buttons = default, bool? shareable = default, bool? autoloop = default, bool? autostart = default, string aspect = default, object value = default, string duration = default)
        {
            Title = title;
            Subtitle = subtitle;
            Text = text;
            Image = image;
            Media = media ?? new List<MediaUrl>();
            Buttons = buttons ?? new List<CardAction>();
            Shareable = shareable;
            Autoloop = autoloop;
            Autostart = autostart;
            Aspect = aspect;
            Duration = duration;
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets title of this card.
        /// </summary>
        /// <value>The title of this card.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets subtitle of this card.
        /// </summary>
        /// <value>The subtitle of this card.</value>
        [JsonProperty(PropertyName = "subtitle")]
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets text of this card.
        /// </summary>
        /// <value>The text of this card.</value>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets thumbnail placeholder.
        /// </summary>
        /// <value>The thumbnail placeholder.</value>
        [JsonProperty(PropertyName = "image")]
        public ThumbnailUrl Image { get; set; }

        /// <summary>
        /// Gets media URLs for this card. When this field contains
        /// more than one URL, each URL is an alternative format of the same
        /// content.
        /// </summary>
        /// <value>The media URLs for this card.</value>
        [JsonProperty(PropertyName = "media")]
        public IList<MediaUrl> Media { get; private set; } = new List<MediaUrl>();

        /// <summary>
        /// Gets actions on this card.
        /// </summary>
        /// <value>The actions of this card.</value>
        [JsonProperty(PropertyName = "buttons")]
        public IList<CardAction> Buttons { get; private set; } = new List<CardAction>();

        /// <summary>
        /// Gets or sets this content may be shared with others (default:true).
        /// </summary>
        /// <value>Boolean indicating if content may be shared with others.</value>
        [JsonProperty(PropertyName = "shareable")]
        public bool? Shareable { get; set; }

        /// <summary>
        /// Gets or sets should the client loop playback at end of content
        /// (default:true).
        /// </summary>
        /// <value>Boolean indicating if client should loop playback at end of content.</value>
        [JsonProperty(PropertyName = "autoloop")]
        public bool? Autoloop { get; set; }

        /// <summary>
        /// Gets or sets should the client automatically start playback of
        /// media in this card (default:true).
        /// </summary>
        /// <value>Boolean indicating if client should automatically start playback of media.</value>
        [JsonProperty(PropertyName = "autostart")]
        public bool? Autostart { get; set; }

        /// <summary>
        /// Gets or sets aspect ratio of thumbnail/media placeholder. Allowed
        /// values are "16:9" and "4:3".
        /// </summary>
        /// <value>The aspect ratio of the thumbnail/media placeholder.</value>
        [JsonProperty(PropertyName = "aspect")]
        public string Aspect { get; set; }

        /// <summary>
        /// Gets or sets describes the length of the media content without
        /// requiring a receiver to open the content. Formatted as an ISO 8601
        /// Duration field.
        /// </summary>
        /// <value>The duration of the media content.</value>
        [JsonProperty(PropertyName = "duration")]
        public string Duration { get; set; }

        /// <summary>
        /// Gets or sets supplementary parameter for this card.
        /// </summary>
        /// <value>The supplementary parameter for this card.</value>
        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
