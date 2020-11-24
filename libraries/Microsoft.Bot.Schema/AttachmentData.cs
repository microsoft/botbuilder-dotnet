// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Attachment data
    /// </summary>
    public partial class AttachmentData
    {
        /// <summary>
        /// Initializes a new instance of the AttachmentData class.
        /// </summary>
        public AttachmentData()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AttachmentData class.
        /// </summary>
        /// <param name="type">Content-Type of the attachment</param>
        /// <param name="name">Name of the attachment</param>
        /// <param name="originalBase64">Attachment content</param>
        /// <param name="thumbnailBase64">Attachment thumbnail</param>
        public AttachmentData(string type = default(string), string name = default(string), byte[] originalBase64 = default(byte[]), byte[] thumbnailBase64 = default(byte[]))
        {
            Type = type;
            Name = name;
            OriginalBase64 = originalBase64;
            ThumbnailBase64 = thumbnailBase64;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets content-Type of the attachment
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets name of the attachment
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets attachment content
        /// </summary>
        [JsonProperty(PropertyName = "originalBase64")]
        public byte[] OriginalBase64 { get; set; }

        /// <summary>
        /// Gets or sets attachment thumbnail
        /// </summary>
        [JsonProperty(PropertyName = "thumbnailBase64")]
        public byte[] ThumbnailBase64 { get; set; }

    }
}
