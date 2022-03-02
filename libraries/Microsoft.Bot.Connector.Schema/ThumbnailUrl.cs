// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// Thumbnail URL.
    /// </summary>
    public class ThumbnailUrl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailUrl"/> class.
        /// </summary>
        public ThumbnailUrl()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailUrl"/> class.
        /// </summary>
        /// <param name="url">URL pointing to the thumbnail to use for media
        /// content.</param>
        /// <param name="alt">HTML alt text to include on this thumbnail
        /// image.</param>
        public ThumbnailUrl(string url = default, string alt = default)
        {
            Url = url;
            Alt = alt;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets URL pointing to the thumbnail to use for media content.
        /// </summary>
        /// <value>The URL pointing to the thumbnail to use for media content.</value>
        [JsonPropertyName("url")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string Url { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets HTML alt text to include on this thumbnail image.
        /// </summary>
        /// <value>The HTML alt text to include on this thumbnail image.</value>
        [JsonPropertyName("alt")]
        public string Alt { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
        }
    }
}
