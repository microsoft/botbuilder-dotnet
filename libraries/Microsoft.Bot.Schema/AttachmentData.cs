// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    /// <summary> Attachment data. </summary>
    public class AttachmentData
    {
        /// <summary>Initializes a new instance of the <see cref="AttachmentData"/> class.</summary>
        /// <param name="type">Content-Type of the attachment.</param>
        /// <param name="name">Name of the attachment.</param>
        /// <param name="originalBase64">Attachment content.</param>
        /// <param name="thumbnailBase64">Attachment thumbnail.</param>
        public AttachmentData(string type = default, string name = default, byte[] originalBase64 = default, byte[] thumbnailBase64 = default)
        {
            Type = type;
            Name = name;

            if (originalBase64 != null)
            {
                OriginalBase64 = new Collection<byte>(originalBase64);
            }

            if (thumbnailBase64 != null)
            {
                ThumbnailBase64 = new Collection<byte>(thumbnailBase64);
            }
        }

        /// <summary>Gets or sets content-type of the attachment.</summary>
        /// <value>The attachment content-type.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>Gets or sets name of the attachment.</summary>
        /// <value>The name of the attachment.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>Gets attachment content.</summary>
        /// <value>The attachment content.</value>
        [JsonProperty(PropertyName = "originalBase64")]
        public Collection<byte> OriginalBase64 { get; private set; } = new Collection<byte>();

        /// <summary>Gets attachment thumbnail.</summary>
        /// <value>The attachment thumbnail.</value>
        [JsonProperty(PropertyName = "thumbnailBase64")]
        public Collection<byte> ThumbnailBase64 { get; private set; } = new Collection<byte>();
    }
}
