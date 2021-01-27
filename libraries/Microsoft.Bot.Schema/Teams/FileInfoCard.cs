// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// File info card.
    /// </summary>
    public partial class FileInfoCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfoCard"/> class.
        /// </summary>
        public FileInfoCard()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfoCard"/> class.
        /// </summary>
        /// <param name="uniqueId">Unique Id for the file.</param>
        /// <param name="fileType">Type of file.</param>
        /// <param name="etag">ETag for the file.</param>
        public FileInfoCard(string uniqueId = default(string), string fileType = default(string), object etag = default(object))
        {
            UniqueId = uniqueId;
            FileType = fileType;
            Etag = etag;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets unique Id for the file.
        /// </summary>
        /// <value>The unique ID for the file.</value>
        [JsonProperty(PropertyName = "uniqueId")]
        public string UniqueId { get; set; }

        /// <summary>
        /// Gets or sets type of file.
        /// </summary>
        /// <value>The type of file.</value>
        [JsonProperty(PropertyName = "fileType")]
        public string FileType { get; set; }

        /// <summary>
        /// Gets or sets eTag for the file.
        /// </summary>
        /// <value>The eTag for the file.</value>
        [JsonProperty(PropertyName = "etag")]
        public object Etag { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
