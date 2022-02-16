// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>Metadata for an attachment.</summary>
    public partial class AttachmentInfo
    {
        /// <summary>Initializes a new instance of the <see cref="AttachmentInfo"/> class.</summary>
        public AttachmentInfo()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="AttachmentInfo"/> class.</summary>
        /// <param name="name">Name of the attachment.</param>
        /// <param name="type">ContentType of the attachment.</param>
        /// <param name="views">attachment views.</param>
        public AttachmentInfo(string name = default, string type = default, IList<AttachmentView> views = default)
        {
            Name = name;
            Type = type;
            Views = views ?? new List<AttachmentView>();
            CustomInit();
        }

        /// <summary>Gets or sets name of the attachment.</summary>
        /// <value>The name of the attachment.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>Gets or sets contentType of the attachment.</summary>
        /// <value>The content type of the attachment.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>Gets attachment views.</summary>
        /// <value> The attachment views.</value>
        [JsonProperty(PropertyName = "views")]
        public IList<AttachmentView> Views { get; private set; } = new List<AttachmentView>();

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        partial void CustomInit();
    }
}
