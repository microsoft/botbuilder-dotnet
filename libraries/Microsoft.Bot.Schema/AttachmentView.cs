// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Attachment View name and size.
    /// </summary>
    public partial class AttachmentView
    {
        /// <summary>Initializes a new instance of the <see cref="AttachmentView"/> class.</summary>
        public AttachmentView()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="AttachmentView"/> class.</summary>
        /// <param name="viewId">Id of the attachment.</param>
        /// <param name="size">Size of the attachment.</param>
        public AttachmentView(string viewId = default(string), int? size = default(int?))
        {
            ViewId = viewId;
            Size = size;
            CustomInit();
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

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        partial void CustomInit();
    }
}
