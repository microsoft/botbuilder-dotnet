// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Thumbnail URL.
    /// </summary>
    public partial class ThumbnailUrl
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
        public ThumbnailUrl(string url = default(string), string alt = default(string))
        {
            Url = url;
            Alt = alt;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets URL pointing to the thumbnail to use for media content.
        /// </summary>
        /// <value>The URL pointing to the thumbnail to use for media content.</value>
        [JsonProperty(PropertyName = "url")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string Url { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets HTML alt text to include on this thumbnail image.
        /// </summary>
        /// <value>The HTML alt text to include on this thumbnail image.</value>
        [JsonProperty(PropertyName = "alt")]
        public string Alt { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
