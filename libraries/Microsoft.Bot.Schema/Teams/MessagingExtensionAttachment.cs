﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Messaging extension attachment.
    /// </summary>
    public class MessagingExtensionAttachment : Attachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionAttachment"/> class.
        /// </summary>
        /// <param name="contentType">mimetype/Contenttype for the file.</param>
        /// <param name="contentUrl">Content Url.</param>
        /// <param name="content">Embedded content.</param>
        /// <param name="name">(OPTIONAL) The name of the attachment.</param>
        /// <param name="thumbnailUrl">(OPTIONAL) Thumbnail associated with attachment.</param>
        /// <param name="preview">A preview attachment.</param>
        public MessagingExtensionAttachment(string contentType = default, string contentUrl = default, object content = default, string name = default, string thumbnailUrl = default, Attachment preview = default)
            : base(contentType, contentUrl, content, name, thumbnailUrl)
        {
            Preview = preview;
        }

        /// <summary>
        /// Gets or sets the preview.
        /// </summary>
        /// <value>The preview.</value>
        [JsonProperty(PropertyName = "preview")]
        public Attachment Preview { get; set; }
    }
}
