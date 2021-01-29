// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary> Attachment data. </summary>
    public partial class AttachmentData
    {
        /// <summary>Initializes a new instance of the <see cref="AttachmentData"/> class.</summary>
        public AttachmentData()
        {
            CustomInit();
        }

        /// <summary>Initializes a new instance of the <see cref="AttachmentData"/> class.</summary>
        /// <param name="type">Content-Type of the attachment.</param>
        /// <param name="name">Name of the attachment.</param>
        /// <param name="originalBase64">Attachment content.</param>
        /// <param name="thumbnailBase64">Attachment thumbnail.</param>
        public AttachmentData(string type = default(string), string name = default(string), byte[] originalBase64 = default(byte[]), byte[] thumbnailBase64 = default(byte[]))
        {
            Type = type;
            Name = name;
            OriginalBase64 = originalBase64;
            ThumbnailBase64 = thumbnailBase64;
            CustomInit();
        }

        /// <summary>Gets or sets content-type of the attachment.</summary>
        /// <value>The attachment content-type.</value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>Gets or sets name of the attachment.</summary>
        /// <value>The name of the attachment.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>Gets or sets attachment content.</summary>
        /// <value>The attachment content.</value>
        [JsonProperty(PropertyName = "originalBase64")]
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking backwards compat)
        public byte[] OriginalBase64 { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>Gets or sets attachment thumbnail.</summary>
        /// <value>The attachment thumbnail.</value>
        [JsonProperty(PropertyName = "thumbnailBase64")]
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] ThumbnailBase64 { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>An initialization method that performs custom operations like setting defaults.</summary>
        partial void CustomInit();
    }
}
