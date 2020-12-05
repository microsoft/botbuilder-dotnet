// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card image.
    /// </summary>
    public partial class O365ConnectorCardImage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardImage"/> class.
        /// </summary>
        public O365ConnectorCardImage()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardImage"/> class.
        /// </summary>
        /// <param name="image">URL for the image.</param>
        /// <param name="title">Alternative text for the image.</param>
        public O365ConnectorCardImage(string image = default(string), string title = default(string))
        {
            Image = image;
            Title = title;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets URL for the image.
        /// </summary>
        /// <value>The URL for the image.</value>
        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets alternative text for the image.
        /// </summary>
        /// <value>The alternative text for the image.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
