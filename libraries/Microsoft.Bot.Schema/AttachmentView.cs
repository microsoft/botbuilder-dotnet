// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;

    /// <summary>
    /// Attachment View name and size.
    /// </summary>
    public class AttachmentView
    {
        /// <summary>Initializes a new instance of the <see cref="AttachmentView"/> class.</summary>
        /// <param name="viewId">Id of the attachment.</param>
        /// <param name="size">Size of the attachment.</param>
        public AttachmentView(string viewId = default, int? size = default)
        {
            ViewId = viewId;
            Size = size;
        }

        /// <summary>
        /// Gets or sets id of the attachment.
        /// </summary>
        /// <value>The ID of the attachment.</value>
        [JsonProperty(PropertyName = "viewId")]
        public string ViewId { get; set; }

        /// <summary>
        /// Gets or sets size of the attachment.
        /// </summary>
        /// <value>The size of the attachment.</value>
        [JsonProperty(PropertyName = "size")]
        public int? Size { get; set; }
    }
}
