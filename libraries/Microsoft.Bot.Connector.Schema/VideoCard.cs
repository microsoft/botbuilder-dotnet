// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
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
            Media = media;
            Buttons = buttons;
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
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets subtitle of this card.
        /// </summary>
        /// <value>The subtitle of this card.</value>
        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets text of this card.
        /// </summary>
        /// <value>The text of this card.</value>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets thumbnail placeholder.
        /// </summary>
        /// <value>The thumbnail placeholder.</value>
        [JsonPropertyName("image")]
        public ThumbnailUrl Image { get; set; }

        /// <summary>
        /// Gets or sets media URLs for this card. When this field contains
        /// more than one URL, each URL is an alternative format of the same
        /// content.
        /// </summary>
        /// <value>The media URLs for this card.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("media")]
        public IList<MediaUrl> Media { get; set; }

        /// <summary>
        /// Gets or sets actions on this card.
        /// </summary>
        /// <value>The actions of this card.</value>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property setter is required for the collection to be deserialized")]
        [JsonPropertyName("buttons")]
        public IList<CardAction> Buttons { get; set; }

        /// <summary>
        /// Gets or sets this content may be shared with others (default:true).
        /// </summary>
        /// <value>Boolean indicating if content may be shared with others.</value>
        [JsonPropertyName("shareable")]
        public bool? Shareable { get; set; }

        /// <summary>
        /// Gets or sets should the client loop playback at end of content
        /// (default:true).
        /// </summary>
        /// <value>Boolean indicating if client should loop playback at end of content.</value>
        [JsonPropertyName("autoloop")]
        public bool? Autoloop { get; set; }

        /// <summary>
        /// Gets or sets should the client automatically start playback of
        /// media in this card (default:true).
        /// </summary>
        /// <value>Boolean indicating if client should automatically start playback of media.</value>
        [JsonPropertyName("autostart")]
        public bool? Autostart { get; set; }

        /// <summary>
        /// Gets or sets aspect ratio of thumbnail/media placeholder. Allowed
        /// values are "16:9" and "4:3".
        /// </summary>
        /// <value>The aspect ratio of the thumbnail/media placeholder.</value>
        [JsonPropertyName("aspect")]
        public string Aspect { get; set; }

        /// <summary>
        /// Gets or sets describes the length of the media content without
        /// requiring a receiver to open the content. Formatted as an ISO 8601
        /// Duration field.
        /// </summary>
        /// <value>The duration of the media content.</value>
        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        /// <summary>
        /// Gets or sets supplementary parameter for this card.
        /// </summary>
        /// <value>The supplementary parameter for this card.</value>
        [JsonPropertyName("value")]
        public object Value { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
