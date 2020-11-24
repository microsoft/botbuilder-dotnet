﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Messaging extension attachment.
    /// </summary>
    public partial class MessagingExtensionAttachment : Attachment
    {
        /// <summary>
        /// Initializes a new instance of the MessagingExtensionAttachment
        /// class.
        /// </summary>
        public MessagingExtensionAttachment()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MessagingExtensionAttachment
        /// class.
        /// </summary>
        /// <param name="contentType">mimetype/Contenttype for the file</param>
        /// <param name="contentUrl">Content Url</param>
        /// <param name="content">Embedded content</param>
        /// <param name="name">(OPTIONAL) The name of the attachment</param>
        /// <param name="thumbnailUrl">(OPTIONAL) Thumbnail associated with attachment</param>
        /// <param name="preview">A preview attachment.</param>
        public MessagingExtensionAttachment(string contentType = default(string), string contentUrl = default(string), object content = default(object), string name = default(string), string thumbnailUrl = default(string), Attachment preview = default(Attachment))
            : base(contentType, contentUrl, content, name, thumbnailUrl)
        {
            Preview = preview;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "preview")]
        public Attachment Preview { get; set; }

    }
}
